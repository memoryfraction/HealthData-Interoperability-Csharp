using Hl7.Fhir.Rest;
using Shared_Library;

namespace _04_Data_Mapping_ETL;

/// <summary>
/// Entry point: Demonstrating ETL data mapping from CSV to FHIR.
/// 入口点：演示从CSV到FHIR的ETL数据映射。
/// </summary>
internal static class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        const string fhirServerUrl = "https://hapi.fhir.org/baseR4";
        string csvPath = Path.Combine(AppContext.BaseDirectory, "Data", "legacy_patients.csv");

        var client = new FhirClient(fhirServerUrl);
        var mapper = new FhirPatientMapper(addTestDataTag: true, testNameMarkers: true);
        var service = new EtlPipelineService(client, mapper);

        try
        {
            var (created, updated) = await service.RunAsync(csvPath);
            Console.WriteLine($"ETL complete. Created: {created}, Updated: {updated}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] {ex.GetType().Name}: {ex.Message}");
        }
    }
}
