using Hl7.Fhir.Rest;
using HealthDataInteropSharedLibrary.Etl;
using HealthDataInteropSharedLibrary.Shared;

namespace _04_Data_Mapping_ETL;

internal static class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
        {
        Console.WriteLine("=== Data Mapping ETL (Module 04) ===");

        const string fhirServerUrl = "https://hapi.fhir.org/baseR4";
        string csvPath = Path.Combine(AppContext.BaseDirectory, "Data", "legacy_patients.csv");

        // SECURITY NOTICE / 安全说明:
        // [EN] TLS certificate validation is STRICT by default. A certificate-validation bypass is available ONLY for local
        //     development and is OFF by default.
        // [CN] TLS 证书验证默认严格开启。证书校验绕过仅用于本地开发，且默认关闭。
        //     TO OPT IN (dev only) / 如需开启（仅限开发）: set HEALTHDATA_INSECURE_SKIP_TLS=1 before starting the process.
        //     NEVER set this in production/staging. Disabling TLS validation violates HIPAA 164.312(e)(1).
        //     切勿在生产/测试环境设置。禁用 TLS 验证违反 HIPAA 164.312(e)(1) 传输安全规定。
        FhirClient client;
        if (DevTlsBypass.IsEnabled)
        {
            var handler = new System.Net.Http.HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (message, cert, chain, errors) => true
            };
            client = new FhirClient(fhirServerUrl, new System.Net.Http.HttpClient(handler));
        }
        else
        {
            client = new FhirClient(fhirServerUrl);
        }

        var mapper = new FhirPatientMapper(addTestDataTag: true, testNameMarkers: true);
        var service = new EtlPipelineService(client, mapper);

        try
        {
            var (created, updated) = await service.RunAsync(csvPath);
            Console.WriteLine($">>> [Success] ETL complete. Created: {created}, Updated: {updated}.");
            Console.WriteLine(">>> [Complete] Module 04 finished successfully.");
        }
        catch (System.Net.Http.HttpRequestException ex)
        {
            Console.WriteLine($">>> [Network] Unable to connect to FHIR server at {fhirServerUrl}.");
            Console.WriteLine($">>> [Network] Reason: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($">>> [Network] Inner: {ex.InnerException.Message}");
            }
            Console.WriteLine(">>> [Hint] This is expected if the server is unreachable from your network.");
            Console.WriteLine(">>> [Hint] Verify connectivity or use a local FHIR server like HAPI FHIR JPA.");
        }
        catch (System.IO.FileNotFoundException ex)
        {
            Console.WriteLine($">>> [File Error] Required data file not found: {ex.Message}");
            Console.WriteLine(">>> [Hint] Ensure Data/legacy_patients.csv is copied to the output directory.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> [Error] Unexpected error during ETL: {ex.Message}");
        }
    }
}
