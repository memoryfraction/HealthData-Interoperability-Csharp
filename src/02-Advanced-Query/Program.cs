namespace _02_Advanced_Query;

/// <summary>
/// Entry point: Demonstrating advanced FHIR query capabilities.
/// 入口点：演示高级FHIR查询功能。
/// </summary>
internal static class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        var service = new AdvancedQueryService("https://server.fire.ly");

        Console.WriteLine("--- Search Results ---");

        try
        {
            var bundle = await service.SearchEncountersByPractitionerNameAsync("Smith");
            Console.WriteLine(AdvancedQueryService.FormatSearchResult(bundle));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Search Error: {ex.Message}");
        }

        Console.WriteLine("--- Search Completed ---");
    }
}
