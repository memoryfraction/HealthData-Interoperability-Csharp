using HealthDataInteropSharedLibrary.SmartOnFHIR;
using HealthDataInteropSharedLibrary.Shared;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HealthData.Interop.Tests.SmartAuthTests;

/// <summary>
/// [EN] Unit tests for OidcOptions via SmartOnFhirAuthService constructor validation.
/// Tests that invalid configurations are rejected at construction time.
/// [CN] 通过SmartOnFhirAuthService构造函数验证OidcOptions的单元测试。
/// 测试无效配置在构建时被拒绝。
/// </summary>
[TestClass]
public sealed class OidcOptionsValidationTests
{
    [TestMethod]
    public void Constructor_WithMissingTokenEndpoint_ShouldThrowViaValidate()
    {
        var options = new OidcOptions
        {
            ClientId = "test-client",
            ClientSecret = "secret"
        };

        Action act = () => _ = new SmartOnFhirAuthService(options, new HttpClient());
        act.Should().Throw<ArgumentException>("TokenEndpoint is required");
    }

    [TestMethod]
    public void Constructor_WithMissingClientId_ShouldThrowViaValidate()
    {
        var options = new OidcOptions
        {
            TokenEndpoint = "https://auth.example.com/token",
            ClientSecret = "secret"
        };

        Action act = () => _ = new SmartOnFhirAuthService(options, new HttpClient());
        act.Should().Throw<ArgumentException>("ClientId is required");
    }

    [TestMethod]
    public void Constructor_ConfidentialClientWithoutSecret_ShouldThrowViaValidate()
    {
        var options = new OidcOptions
        {
            TokenEndpoint = "https://auth.example.com/token",
            ClientId = "test-client",
            IsConfidentialClient = true
        };

        Action act = () => _ = new SmartOnFhirAuthService(options, new HttpClient());
        act.Should().Throw<InvalidOperationException>("Missing ClientSecret for confidential client");
    }

    [TestMethod]
    public void Constructor_PublicClientWithoutSecret_ShouldSucceed()
    {
        var options = new OidcOptions
        {
            TokenEndpoint = "https://auth.example.com/token",
            ClientId = "public-client",
            IsConfidentialClient = false
        };

        Action act = () => _ = new SmartOnFhirAuthService(options, new HttpClient());
        act.Should().NotThrow("Public clients do not require a secret");
    }

    [TestMethod]
    public void OidcOptions_DefaultScope_ShouldBeSmartOnFhirDefault()
    {
        var options = new OidcOptions();
        options.Scope.Should().Be("openid profile patient/*.read");
        options.IsConfidentialClient.Should().BeTrue();
    }
}

/// <summary>
/// [EN] Unit tests for SmartOnFhirAuthService constructor and dispose safety.
/// Note: Actual OAuth2 network calls are not tested in unit tests; those require 
/// a real or mock OIDC provider (integration test scope).
/// [CN] SmartOnFhirAuthService构造函数和dispose安全性的单元测试。
/// 注意：实际的OAuth2网络调用不在单元测试中测试；需要真实或模拟的OIDC提供商（集成测试范围）。
/// </summary>
[TestClass]
public sealed class SmartOnFhirAuthServiceTests
{
    [TestMethod]
    public void Constructor_WithValidOptions_ShouldSucceed()
    {
        var options = new OidcOptions
        {
            TokenEndpoint = "https://auth.example.com/token",
            ClientId = "test-client",
            ClientSecret = "secret",
            IsConfidentialClient = true
        };

        using var httpClient = new HttpClient();
        using var service = new SmartOnFhirAuthService(options, httpClient);

        service.Should().NotBeNull();
    }

    [TestMethod]
    public void Constructor_WithNullOptions_ShouldThrow()
    {
        Action act = () =>
            _ = new SmartOnFhirAuthService(null!, new HttpClient());

        act.Should().Throw<ArgumentNullException>("options parameter must not be null");
    }

    [TestMethod]
    public void Constructor_WithNullHttpClient_ShouldThrow()
    {
        var options = new OidcOptions
        {
            TokenEndpoint = "https://auth.example.com/token",
            ClientId = "test-client",
            ClientSecret = "secret"
        };

        Action act = () =>
            _ = new SmartOnFhirAuthService(options, null!);

        act.Should().Throw<ArgumentNullException>("httpClient parameter must not be null");
    }

    [TestMethod]
    public void Constructor_WithInvalidOptions_ShouldThrow()
    {
        var options = new OidcOptions
        {
            ClientId = "test",
            ClientSecret = "secret"
        };

        Action act = () =>
            _ = new SmartOnFhirAuthService(options, new HttpClient());

        act.Should().Throw<ArgumentException>("Options validation should fail");
    }

    [TestMethod]
    public void CreateAuthenticatedFhirClientAsync_WithEmptyUrl_ShouldThrow()
    {
        var options = new OidcOptions
        {
            TokenEndpoint = "https://auth.example.com/token",
            ClientId = "test-client",
            ClientSecret = "secret"
        };

        using var service = new SmartOnFhirAuthService(options, new HttpClient());

        Func<Task> act = async () => await service.CreateAuthenticatedFhirClientAsync("");

        act.Should().ThrowAsync<ArgumentException>("fhirServerUrl must not be empty");
    }
}
