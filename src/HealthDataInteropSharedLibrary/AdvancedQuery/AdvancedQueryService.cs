using HealthDataInteropSharedLibrary.Shared;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace HealthDataInteropSharedLibrary.AdvancedQuery;

/// <summary>
/// [EN] Service for advanced FHIR search queries including chained parameters, _include, and _revinclude.
/// [CN] 高级FHIR搜索查询服务，包括链式参数、_include和_revinclude。
/// </summary>
public sealed class AdvancedQueryService
{
    private readonly FhirClient _client;

    /// <summary>
    /// [EN] Initialize with a FHIR server URL. TLS certificate validation is STRICT by default.
    /// [CN] 使用 FHIR 服务器 URL 初始化。TLS 证书验证默认严格开启。
    /// </summary>
    /// <remarks>
    /// [EN] A certificate-validation bypass is available ONLY for local development and is OFF by default.
    ///     To opt in (dev only), set the environment variable HEALTHDATA_INSECURE_SKIP_TLS=1 before
    ///     starting the process. NEVER set this in production/staging. Disabling TLS validation violates
    ///     HIPAA 164.312(e)(1).
    /// [CN] 证书校验绕过仅用于本地开发，且默认关闭。如需开启（仅限开发），请在启动进程前设置环境变量
    ///     HEALTHDATA_INSECURE_SKIP_TLS=1。切勿在生产/测试环境设置。禁用 TLS 验证违反 HIPAA 164.312(e)(1)。
    /// </remarks>
    public AdvancedQueryService(string fhirServerUrl)
    {
        Guard.NotNullOrEmpty(fhirServerUrl, nameof(fhirServerUrl));

        // SECURITY NOTICE / 安全说明:
        // [EN] TLS certificate validation is STRICT by default. A certificate-validation bypass is available ONLY for local
        //     development and is OFF by default.
        // [CN] TLS 证书验证默认严格开启。证书校验绕过仅用于本地开发，且默认关闭。
        //     TO OPT IN (dev only) / 如需开启（仅限开发）: set HEALTHDATA_INSECURE_SKIP_TLS=1 before starting the process.
        //     NEVER set this in production/staging. Disabling TLS validation violates HIPAA 164.312(e)(1).
        //     切勿在生产/测试环境设置。禁用 TLS 验证违反 HIPAA 164.312(e)(1) 传输安全规定。
        if (DevTlsBypass.IsEnabled)
        {
            var handler = new System.Net.Http.SocketsHttpHandler();
            handler.SslOptions = new System.Net.Security.SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true
            };
            _client = new FhirClient(fhirServerUrl, new System.Net.Http.HttpClient(handler));
        }
        else
        {
            _client = new FhirClient(fhirServerUrl);
        }
    }

    /// <summary>
    /// [EN] Search Encounters by practitioner name with forward and reverse includes.
    /// Fetches Encounter records where participant's practitioner name matches the given search term,
    /// includes related Patient resources, and reverse-includes Observation resources.
    /// [CN] 按医生姓名搜索就诊记录，包含相关的患者资源和反向包含观察指标资源。
    /// </summary>
    public async System.Threading.Tasks.Task<Bundle> SearchEncountersByPractitionerNameAsync(string practitionerName)
    {
        Guard.NotNullOrEmpty(practitionerName, nameof(practitionerName));

        var q = new SearchParams();

        // Chained Parameters: participant.individual.name
        q.Where($"participant.individual.name:contains={practitionerName}");

        // Forward Include: Encounter -> Patient
        q.Include("Encounter:patient");

        // Reverse Include: Observation -> Patient
        q.Include("Observation:patient");

        var results = await _client.SearchAsync<Encounter>(q);
        return results;
    }

    /// <summary>
    /// [EN] Format search result entries for display.
    /// [CN] 格式化搜索结果条目以供显示。
    /// </summary>
    public static string FormatSearchResult(Bundle bundle)
    {
        Guard.NotNull(bundle, nameof(bundle));

        if (bundle.Entry.Count == 0)
            return "No matching resources found.";

        var lines = new List<string>();
        foreach (var entry in bundle.Entry)
        {
            lines.Add($"Resource found: {entry.Resource.TypeName}/{entry.Resource.Id}");
        }
        return string.Join(System.Environment.NewLine, lines);
    }
}

/// <summary>
/// [EN] Parameter validation helpers.
/// [CN] 参数验证辅助方法。
/// </summary>
internal static class Guard
{
    public static void NotNull(object? value, string name)
    {
        if (value is null)
            throw new System.ArgumentNullException(name, $"Parameter '{name}' must not be null.");
    }

    public static void NotNullOrEmpty(string? value, string name)
    {
        if (value is null)
            throw new System.ArgumentNullException(name, $"Parameter '{name}' must not be null.");
        if (value.Length == 0)
            throw new System.ArgumentException($"Parameter '{name}' must not be empty.", name);
    }
}
