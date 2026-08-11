using Hl7.Fhir.Rest;
using Microsoft.Extensions.Configuration;
using HealthDataInteropSharedLibrary.SmartOnFHIR;
using System.Net;

namespace _05_SMART_on_FHIR;

/// <summary>
/// Entry point: Demonstrating SMART-on-FHIR ETL pipeline.
/// </summary>
internal static class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        // --- 1. Load Configuration / 加载配置 ---
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

        if (!File.Exists(configPath))
        {
            Console.WriteLine("[Error] appsettings.json not found at: " + configPath);
            Console.WriteLine("[Hint] Ensure the file is copied to the output directory or run from the project folder.");
            return;
        }

        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string fhirServerUrl = config["MeldRx:FhirServerUrl"] ?? "https://hapi.fhir.org/baseR4";

        // --- 2. Initialize Network Handler / 初始化网络处理器 ---
        var handler = new SocketsHttpHandler
        {
            UseProxy = true,
            ConnectTimeout = System.TimeSpan.FromSeconds(30)
        };

        // ⚠️ SECURITY NOTICE / 安全说明:
        // [EN] Certificate validation callback is set to always return true. This DISABLES HTTPS certificate
        //     validation and makes the connection VULNERABLE to MITM attacks.
        //     REASON: Local development in restricted network environments where connecting to remote FHIR servers
        //     fails due to proxy/firewall/network issues.
        //     RISK: Silent security degradation - ALL TLS errors are silently ignored.
        // 🔒 PRODUCTION REQUIREMENT: NEVER use this in production. HIPAA §164.312(e)(1) requires strict
        //     TLS certificate validation. Production MUST enforce HTTPS-only connections with valid certificates.
        // [CN] 证书验证回调始终返回true。这会禁用HTTPS证书验证，连接易受中间人攻击。
        //     原因：本地开发环境因网络问题无法连接远程FHIR服务器。
        //     风险：所有TLS错误被静默忽略，包括过期/无效证书。
        // 🔒 生产环境要求：绝不在生产环境中使用。HIPAA §164.312(e)(1)要求严格TLS证书验证。
        handler.SslOptions = new System.Net.Security.SslClientAuthenticationOptions
        {
            RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
            EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
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

        // --- 3. Generate unique CSV per run to avoid duplicate resource errors ---
        string csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "data.csv");
        if (!File.Exists(csvPath))
        {
            Console.WriteLine($"[Error] data.csv not found at: {csvPath}");
            return;
        }

        var runSuffix = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        string tempCsvPath = CreateUniquePatientCsv(csvPath, runSuffix);

        // [EN] Use unique identifier system per run to avoid HAPI-2840 duplicate resource errors.
        // hapi.fhir.org deduplicates by checking patient identifiers; using a run-specific
        // identifier system ensures each test run creates distinct resources.
        var service = new SmartFhirEtlService(client, $"http://example.org/test-ids/run-{runSuffix}");

        try
        {
            var imported = await service.ImportPatientsAsync(tempCsvPath, delayMs: 500);
            Console.WriteLine($">>> [Complete] {imported} patients imported.");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"[Error] {ex.Message}");
        }
        finally
        {
            if (File.Exists(tempCsvPath))
            {
                File.Delete(tempCsvPath);
            }
        }
    }

    /// <summary>
    /// [EN] Create a temporary CSV with unique identifiers to avoid HAPI-2840 duplicate resource errors
    /// on the public test server. Appends a run-specific suffix to each patient identifier.
    /// </summary>
    private static string CreateUniquePatientCsv(string sourceCsvPath, string suffix)
    {
        if (!File.Exists(sourceCsvPath))
            throw new FileNotFoundException($"Source CSV not found: {sourceCsvPath}");

        var tempPath = Path.Combine(Path.GetTempPath(), $"patients_{suffix}.csv");

        var lines = File.ReadAllLines(sourceCsvPath);
        for (int i = 1; i < lines.Length; i++) // Skip header, modify data rows
        {
            var parts = lines[i].Split(',');
            if (parts.Length >= 2)
            {
                parts[0] = $"{parts[0]}_{suffix}";  // Unique FirstName
                parts[1] = $"{parts[1]}_{suffix}";  // Unique LastName
                lines[i] = string.Join(",", parts);
            }
        }

        File.WriteAllLines(tempPath, lines);
        Console.WriteLine($">>> [Info] Generated unique test data (suffix: {suffix}) to avoid duplicate resource conflicts.");
        return tempPath;
    }
}
