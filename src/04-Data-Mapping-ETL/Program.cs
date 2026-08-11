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

        // NOTE: HTTPS validation bypassed for local development in restricted network environments.
        // Production MUST enforce strict TLS validation. HIPAA 164.312(e)(1) requires it.
        var handler = new System.Net.Http.HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = 
                (message, cert, chain, errors) => true
        };

        var client = new FhirClient(fhirServerUrl, new System.Net.Http.HttpClient(handler));

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
