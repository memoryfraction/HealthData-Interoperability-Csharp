using HealthDataInteropSharedLibrary.Shared;
using Hl7.Fhir.Rest;
using IdentityModel.Client;
using System.Net.Http.Headers;

namespace HealthDataInteropSharedLibrary.SmartOnFHIR;

/// <summary>
/// [EN] Configuration options for SMART on FHIR OAuth2 / OIDC authentication.
/// Specifies the authorization server, client credentials, and scope.
/// [CN] SMART on FHIR OAuth2 / OIDC认证配置选项。指定授权服务器、客户端凭据和权限范围。
/// </summary>
public sealed class OidcOptions
{
    /// <summary>
    /// [EN] Token endpoint URI (e.g., https://server.com/connect/token).
    /// [CN] 令牌端点URI（例如 https://server.com/connect/token）。
    /// </summary>
    public string TokenEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// [EN] OAuth2 client identifier.
    /// [CN] OAuth2客户端标识符。
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// [EN] OAuth2 client secret (confidential clients only).
    /// [CN] OAuth2客户端密钥（仅限机密客户端）。
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// [EN] SMART on FHIR scope(s), e.g. "openid profile patient/*.read".
    /// [CN] SMART on FHIR权限范围，例如 "openid profile patient/*.read"。
    /// </summary>
    public string Scope { get; set; } = "openid profile patient/*.read";

    /// <summary>
    /// [EN] Whether this is a public (no client secret) or confidential client.
    /// [CN] 是否为公开客户端（无客户端密钥）或机密客户端。
    /// </summary>
    public bool IsConfidentialClient { get; set; } = true;

    /// <summary>
    /// [EN] Validate required fields are populated. Throws if configuration is invalid.
    /// [CN] 验证必填字段是否已填充。配置无效时抛出异常。
    /// </summary>
    internal void Validate()
    {
        HealthDataInteropSharedLibrary.Shared.Guard.NotNullOrEmpty(TokenEndpoint, nameof(TokenEndpoint));
        HealthDataInteropSharedLibrary.Shared.Guard.NotNullOrEmpty(ClientId, nameof(ClientId));

        if (IsConfidentialClient && string.IsNullOrWhiteSpace(ClientSecret))
            throw new InvalidOperationException("IsConfidentialClient is true but ClientSecret is not set.");
    }
}

/// <summary>
/// [EN] Cached access token with expiry information for automatic refresh.
/// [CN] 带过期信息的缓存访问令牌，用于自动刷新。
/// </summary>
internal sealed class CachedToken
{
    /// <summary>
    /// [EN] The bearer access token value.
    /// [CN] 承载者访问令牌值。
    /// </summary>
    public string AccessToken { get; }

    /// <summary>
    /// [EN] Token type returned by the server (typically "Bearer").
    /// [CN] 服务器返回的令牌类型（通常为"Bearer"）。
    /// </summary>
    public string TokenType { get; }

    /// <summary>
    /// [EN] Absolute expiry time of this token in UTC.
    /// [CN] 此令牌的绝对过期时间，使用UTC。
    /// </summary>
    public DateTimeOffset ExpiresUtc { get; }

    public CachedToken(string accessToken, string tokenType, int expiresInSeconds)
    {
        AccessToken = accessToken;
        TokenType = tokenType;
        ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds);
    }

    /// <summary>
    /// [EN] Check whether this token is still valid (with 30-second safety margin).
    /// [CN] 检查此令牌是否仍然有效（带30秒安全余量）。
    /// </summary>
    public bool IsStillValid() => DateTimeOffset.UtcNow.AddSeconds(30) < ExpiresUtc;
}

/// <summary>
/// [EN] SMART on FHIR Authentication Service implementing OAuth2 Client Credentials flow.
/// Manages token acquisition, caching, and automatic refresh before expiry.
/// Wraps FhirClient creation with authenticated HTTP headers per HIPAA 164.312(a)(2)(iii).
/// 
/// [CN] SMART on FHIR认证服务，实现OAuth2客户端凭据流程。
/// 管理令牌获取、缓存和过期前自动刷新。
/// 按照HIPAA §164.312(a)(2)(iii)使用认证的HTTP头包装FhirClient创建。
/// </summary>
public sealed class SmartOnFhirAuthService : IDisposable
{
    private readonly OidcOptions _options;
    private readonly HttpClient _tokenHttpClient;
    private CachedToken? _cachedToken;
    private bool _disposed;

    /// <summary>
    /// [EN] Initialize with OAuth2 configuration and a dedicated HTTP client for token requests.
    /// The provided httpClient is used exclusively for calling the token endpoint, not FHIR API calls.
    /// [CN] 使用OAuth2配置和专用的令牌请求HTTP客户端进行初始化。
    /// 提供的httpClient仅用于调用令牌端点，不用于FHIR API调用。
    /// </summary>
    public SmartOnFhirAuthService(OidcOptions options, HttpClient tokenHttpClient)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();

        _tokenHttpClient = tokenHttpClient ?? throw new ArgumentNullException(nameof(tokenHttpClient));
    }

    /// <summary>
    /// [EN] Get or refresh the access token. Automatically fetches a new token if the cached one is expired.
    /// This is the core OAuth2 Client Credentials Grant flow as specified by SMART on FHIR.
    /// [CN] 获取或刷新访问令牌。如果缓存的令牌已过期，自动获取新令牌。
    /// 这是SMART on FHIR规范的OAuth2客户端凭据授权流程。
    /// </summary>
    /// <returns>[EN] Current valid access token / [CN] 当前有效的访问令牌</returns>
    public async Task<string> GetAccessTokenAsync()
    {
        if (_cachedToken is not null && _cachedToken.IsStillValid())
            return _cachedToken.AccessToken;

        _cachedToken = await FetchTokenAsync();
        return _cachedToken.AccessToken;
    }

    /// <summary>
    /// [EN] Create an authenticated FhirClient with the bearer token attached to the Authorization header.
    /// The returned FhirClient is ready to make authorized requests against the FHIR server.
    /// [CN] 创建带有Bearer令牌的已认证FhirClient，附加到Authorization头。
    /// 返回的FhirClient已准备好对FHIR服务器进行授权请求。
    /// </summary>
    /// <param name="fhirServerUrl">[EN] Base URL of the FHIR server / [CN] FHIR服务器的基础URL</param>
    /// <returns>[EN] Authenticated FhirClient instance / [CN] 已认证的FhirClient实例</returns>
    public async Task<FhirClient> CreateAuthenticatedFhirClientAsync(string fhirServerUrl)
    {
        HealthDataInteropSharedLibrary.Shared.Guard.NotNullOrEmpty(fhirServerUrl, nameof(fhirServerUrl));

        var accessToken = await GetAccessTokenAsync();

        var handler = new SocketsHttpHandler();
        var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            _cachedToken?.TokenType ?? "Bearer",
            accessToken
        );

        var settings = new FhirClientSettings { PreferredFormat = ResourceFormat.Json };
        return new FhirClient(fhirServerUrl, client, settings);
    }

    /// <summary>
    /// [EN] Force a token refresh regardless of current cache state.
    /// Useful for testing or when the server has revoked the current token.
    /// [CN] 强制刷新令牌，忽略当前缓存状态。
    /// 适用于测试或服务器已撤销当前令牌的情况。
    /// </summary>
    public async Task RefreshTokenAsync()
    {
        _cachedToken = await FetchTokenAsync();
    }

    /// <summary>
    /// [EN] Perform the OAuth2 Client Credentials Grant request to obtain a new token.
    /// Posts to the token endpoint with client_id, client_secret (if confidential), and scope.
    /// [CN] 执行OAuth2客户端凭据授权请求以获取新令牌。
    /// 向令牌端点发送client_id、client_secret（如为机密）和scope。
    /// </summary>
    private async Task<CachedToken> FetchTokenAsync()
    {
        var request = new ClientCredentialsTokenRequest
        {
            Address = _options.TokenEndpoint,
            ClientId = _options.ClientId,
            Scope = _options.Scope
        };

        if (_options.IsConfidentialClient && !string.IsNullOrEmpty(_options.ClientSecret))
        {
            request.ClientSecret = _options.ClientSecret;
        }

        var response = await _tokenHttpClient.RequestClientCredentialsTokenAsync(request);

        if (response.IsError)
            throw new InvalidOperationException($"OAuth2 token request failed: {response.Error}");

        return new CachedToken(
            response.AccessToken,
            response.TokenType,
            response.ExpiresIn
        );
    }

    /// <summary>
    /// [EN] Dispose the internal HTTP client.
    /// [CN] 释放内部HTTP客户端。
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _tokenHttpClient.Dispose();
            _disposed = true;
        }
    }
}
