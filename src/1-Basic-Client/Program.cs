using Shared_Library.BasicClient;

namespace HealthData.Interop.BasicClient;

/// <summary>
/// Entry point: Demonstrating basic FHIR Patient creation and search.
/// 入口点：演示基础的 FHIR Patient 资源创建与查询。
/// </summary>
internal static class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("This is my first FHIR Client");

        var service = new FhirBasicService("http://server.fire.ly");

        try
        {
            // Create patient resource on the server
            await service.CreatePatientAsync(new[] { "John", "James" }, "Doe", "Male", "1990-01-01", "123456790");

            // Search for patients with the name "John"
            var results = await service.SearchPatientsByNameAsync("John");

            foreach (var patient in results)
            {
                Console.WriteLine($"Received patient: {FhirBasicService.FormatPatientName(patient)}");
            }
        }
        catch (Hl7.Fhir.Rest.FhirOperationException ex)
        {
            Console.WriteLine($"FHIR Error: {ex.Outcome}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General Error: {ex.Message}");
        }
    }
}
