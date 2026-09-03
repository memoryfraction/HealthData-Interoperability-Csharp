using HealthDataInteropSharedLibrary.AdvancedQuery;

namespace _02_Advanced_Query;

/// <summary>
/// Entry point: Demonstrating advanced FHIR query capabilities.
/// </summary>
internal static class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        Console.WriteLine("=== Advanced Query (Module 02) ===");

        var service = new AdvancedQueryService("https://server.fire.ly");

        Console.WriteLine("--- Search Results ---");

        try
        {
            var bundle = await service.SearchEncountersByPractitionerNameAsync("Smith");
            Console.WriteLine(AdvancedQueryService.FormatSearchResult(bundle));
            Console.WriteLine(">>> [Complete] Module 02 finished successfully.");
        }
        catch (System.Net.Http.HttpRequestException ex)
        {
            Console.WriteLine($">>> [Network] Unable to connect to FHIR server.");
            Console.WriteLine($">>> [Network] Reason: {ex.Message}");
            Console.WriteLine(">>> [Hint] This is expected if the server (server.fire.ly) is unreachable from your network.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> [Error] Search failed: {ex.Message}");
        }

        Console.WriteLine("--- Search Completed ---");
    }
}
