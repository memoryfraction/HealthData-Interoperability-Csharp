using HealthDataInteropSharedLibrary.BasicClient;
using Hl7.Fhir.Rest;

namespace HealthData.Interop.BasicClient;

/// <summary>
/// Entry point: Demonstrating basic FHIR Patient creation and search.
/// </summary>
internal static class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        Console.WriteLine("=== Basic FHIR Client (Module 01) ===");

        // Use a timestamped patient name to avoid duplicate resource errors on public servers
        var runSuffix = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        var givenName = $"Demo{runSuffix}";
        var familyName = "Patient";

        var service = new FhirBasicService("http://server.fire.ly");

        try
        {
            // Create patient resource on the server
            await service.CreatePatientAsync(new[] { givenName }, familyName, "Male", "1990-01-01", $"demo-{runSuffix}");

            // Search for patients with the name
            var results = await service.SearchPatientsByNameAsync(givenName);

            if (results.Count == 0)
            {
                Console.WriteLine("[Info] No matching patients found. The server may filter results differently.");
            }
            else
            {
                foreach (var patient in results)
                {
                    Console.WriteLine($"Received patient: {FhirBasicService.FormatPatientName(patient)}");
                }
            }

            Console.WriteLine(">>> [Complete] Module 01 finished successfully.");
        }
        catch (FhirOperationException ex)
        {
            Console.WriteLine($">>> [FHIR Error] Server returned an OperationOutcome:");
            Console.WriteLine($">>> [FHIR Error] {ex.Outcome}");
            Console.WriteLine(">>> [Hint] The public test server may have different FHIR version constraints.");
        }
        catch (System.Net.Http.HttpRequestException ex)
        {
            Console.WriteLine($">>> [Network] Unable to connect to FHIR server.");
            Console.WriteLine($">>> [Network] Reason: {ex.Message}");
            Console.WriteLine(">>> [Hint] This is expected if the public test server (server.fire.ly) is unreachable from your network.");
            Console.WriteLine(">>> [Hint] Ensure you have internet connectivity. The public HAPI server at hapi.fhir.org is an alternative.");
        }
        catch (System.TimeoutException ex)
        {
            Console.WriteLine($">>> [Timeout] FHIR server did not respond within the expected time.");
            Console.WriteLine($">>> [Timeout] Details: {ex.Message}");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("disposed") || ex.Message.Contains("parser"))
        {
            Console.WriteLine($">>> [SDK Error] Firely SDK encountered an issue: {ex.Message}");
            Console.WriteLine(">>> [Hint] This may indicate a FHIR version mismatch between the client and server.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> [Error] An unexpected error occurred:");
            Console.WriteLine($">>> [Error] Message: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($">>> [Error] Inner: {ex.InnerException.Message}");
            }
        }
    }
}
