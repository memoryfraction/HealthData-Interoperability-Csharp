using Hl7.Fhir.Rest;
using Microsoft.Extensions.Configuration;
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
