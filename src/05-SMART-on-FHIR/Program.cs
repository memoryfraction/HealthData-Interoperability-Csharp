using Hl7.Fhir.Rest;
using Microsoft.Extensions.Configuration;
using HealthDataInteropSharedLibrary.SmartOnFHIR;
using System.Net;

namespace _05_SMART_on_FHIR;

/// <summary>
/// Entry point: Demonstrating SMART-on-FHIR ETL pipeline.
/// 入口点：演示SMART-on-FHIR ETL流水线。
/// </summary>
internal static class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        // --- 1. Load Configuration / 加载配置 ---
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string fhirServerUrl = config["MeldRx:FhirServerUrl"] ?? "https://hapi.fhir.org/baseR4";

                // !!! SECURITY WARNING: TLS Certificate Validation Bypassed / 安全警告：TLS证书验证被禁用 !!!
        // -------------------------------------------------------------------------
        // [EN] The following RemoteCertificateValidationCallback ALWAYS returns true.
        //      This DISABLES HTTPS certificate validation and makes the connection VULNERABLE to MITM attacks.
        //      REASON: Development environment VPN causes network connectivity issues with remote FHIR servers.
        //      RISK: Silent security degradation - ALL TLS errors are silently ignored, including expired/invalid certs.
        //      ACTION REQUIRED FOR PRODUCTION: 1) Enable strict HTTPS/TLS certificate validation (REMOVE this callback). 2) Enforce HTTPS-only connections. 3) Configure server-side HSTS headers.
        //      HIPAA IMPACT: Disabling TLS validation violates HIPAA §164.312(e)(1) transmission security rule.
        // [CN] 下面的 RemoteCertificateValidationCallback 始终返回 true，这会禁用 HTTPS 证书验证。
        //      原因：开发环境 VPN 导致与远程 FHIR 服务器的网络连接问题。
        //      风险：所有 TLS 错误被静默忽略，包括过期/无效证书。可能导致中间人攻击 (MITM)。
        //      必须操作：1) 部署到测试/生产环境前移除此回调。2) **强制开启 HTTPS**，所有 FHIR 客户端端点必须使用 HTTPS（不允许 HTTP 回退）。3) 配置服务器端 HSTS 头。4) 最低 TLS 1.2+。
        //      HIPAA 影响：禁用 TLS 验证违反 HIPAA §164.312(e)(1) 传输安全规定。
        // -------------------------------------------------------------------------
        // --- 2. Initialize Network Handler / 初始化网络处理器 ---
        var handler = new SocketsHttpHandler
        {
            SslOptions = new System.Net.Security.SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
            },
            UseProxy = true,
            ConnectTimeout = System.TimeSpan.FromSeconds(30)
        };

        using var httpClient = new HttpClient(handler)
        {
            DefaultRequestVersion = HttpVersion.Version11,
            Timeout = System.TimeSpan.FromMinutes(2)
        };

        var settings = new FhirClientSettings { PreferredFormat = ResourceFormat.Json };
        var client = new FhirClient(fhirServerUrl, httpClient, settings);

        Console.WriteLine($">>> [Target] {fhirServerUrl}");
        Console.WriteLine(">>> [Status] Starting SMART on FHIR ETL Pipeline...");

        // --- 3. ETL Implementation / ETL 逻辑实现 ---
        string csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "data.csv");

        var service = new SmartFhirEtlService(client);

        try
        {
            var imported = await service.ImportPatientsAsync(csvPath, delayMs: 500);
            Console.WriteLine($">>> [Complete] {imported} patients imported.");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"[Error] {ex.Message}");
        }
    }
}