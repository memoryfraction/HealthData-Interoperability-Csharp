using Hl7.Fhir.Rest;
using Microsoft.Extensions.Configuration;
using HealthDataInteropSharedLibrary.SmartOnFHIR;
using HealthDataInteropSharedLibrary.Shared;
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

        // SECURITY NOTICE / 安全说明:
        // [EN] STRICT TLS by default. A certificate-validation bypass is available ONLY for local development and is OFF by default.
        //     TO OPT IN (dev only) / 如需开启（仅限开发）: set HEALTHDATA_INSECURE_SKIP_TLS=1 before starting the process.
        //     NEVER set this in production/staging. Disabling TLS validation violates HIPAA 164.312(e)(1).
        // [CN] TLS 证书验证默认严格开启。证书校验绕过仅用于本地开发，且默认关闭。
        //     切勿在生产/测试环境设置。禁用 TLS 验证违反 HIPAA 164.312(e)(1) 传输安全规定。
        if (DevTlsBypass.IsEnabled)
        {
            handler.SslOptions = new System.Net.Security.SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
            };
        }

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
