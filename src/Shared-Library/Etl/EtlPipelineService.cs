using Shared_Library.SmartOnFHIR;
using Shared_Library.Shared;
using CsvHelper;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Shared_Library;
using System.Globalization;
using Task = System.Threading.Tasks.Task;

namespace Shared_Library.Etl;

/// <summary>
/// [EN] ETL pipeline service: Extract CSV -> Transform to FHIR Patient -> Load via Transaction Bundle.
/// Provides reusable, testable methods for each phase of the ETL process.
/// [CN] ETL流水线服务：提取CSV -> 转换为FHIR Patient -> 通过事务Bundle加载。
/// 提供ETL流程各阶段的可复用、可测试方法。
/// </summary>
public sealed class EtlPipelineService
{
    private readonly FhirPatientMapper _mapper;
    private readonly FhirClient _client;

    /// <summary>
    /// [EN] Initialize with a FHIR client and patient mapper.
    /// [CN] 使用FHIR客户端和患者映射器初始化。
    /// </summary>
    public EtlPipelineService(FhirClient client, FhirPatientMapper mapper)
    {
        Guard.NotNull(client, nameof(client));
        Guard.NotNull(mapper, nameof(mapper));
        _client = client;
        _mapper = mapper;
    }

    /// <summary>
    /// [EN] Run the full ETL pipeline: extract CSV, transform to FHIR patients, and load via transaction.
    /// [CN] 运行完整的ETL流水线：提取CSV、转换为FHIR患者、通过事务加载。
    /// </summary>
    public async Task<(int Created, int Updated)> RunAsync(string csvPath)
    {
        Guard.NotNullOrEmpty(csvPath, nameof(csvPath));

        Console.WriteLine("[ETL Process] Starting data mapping task...");

        if (!File.Exists(csvPath))
            throw new FileNotFoundException($"CSV file not found at: {csvPath}");

        // --- Extract + Transform ---
        var patientsToImport = await ExtractAndTransformAsync(csvPath);
        Console.WriteLine($"[Extract] Read and mapped {patientsToImport.Count} records.");

        // --- Load: Transaction Bundle ---
        var batchBundle = BuildTransactionBundle(patientsToImport);
        Console.WriteLine("[Load] Sending Transaction Bundle...");

        var response = await _client.TransactionAsync(batchBundle);
        var result = AnalyzeResponse(response);
        Console.WriteLine($"[Success] Load completed. Created: {result.Created}, Updated: {result.Updated}.");

        // --- Verify ---
        await VerifyOnServer(patientsToImport.Count);

        return result;
    }

    /// <summary>
    /// [EN] Extract CSV records and transform each to FHIR Patient via mapper.
    /// [CN] 提取CSV记录并通过映射器转换为FHIR Patient。
    /// </summary>
    public async Task<List<(string LegacyId, Patient Patient)>> ExtractAndTransformAsync(string csvPath)
    {
        Guard.NotNullOrEmpty(csvPath, nameof(csvPath));

        var results = new List<(string LegacyId, Patient Patient)>();

        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var records = csv.GetRecords<Shared_Library.Shared.LegacyPatientRecord>().ToList();

        foreach (var record in records)
        {
            var patient = _mapper.MapLegacy(record);
            results.Add((record.Id, patient));
        }

        return results;
    }

    /// <summary>
    /// [EN] Build a FHIR Transaction Bundle from mapped patients using conditional PUT for idempotent upserts.
    /// [CN] 从已映射的患者构建FHIR事务Bundle，使用条件PUT实现幂等更新。
    /// </summary>
    public static Bundle BuildTransactionBundle(List<(string LegacyId, Patient Patient)> patients)
    {
        Guard.NotNull(patients, nameof(patients));

        var bundle = new Bundle { Type = Bundle.BundleType.Transaction };

        foreach (var item in patients)
        {
            var identifier = item.Patient.Identifier.First();
            bundle.Entry.Add(new Bundle.EntryComponent
            {
                Resource = item.Patient,
                Request = new Bundle.RequestComponent
                {
                    Method = Bundle.HTTPVerb.PUT,
                    Url = $"Patient?identifier={identifier.System}|{identifier.Value}"
                }
            });
        }

        return bundle;
    }

    /// <summary>
    /// [EN] Analyze Transaction response to count Created vs Updated entries.
    /// [CN] 分析事务响应以统计新建与更新条目数。
    /// </summary>
    public static (int Created, int Updated) AnalyzeResponse(Bundle response)
    {
        Guard.NotNull(response, nameof(response));

        int created = 0, updated = 0;
        foreach (var entry in response.Entry)
        {
            if (entry.Response?.Status.Contains("201") == true) created++;
            else if (entry.Response?.Status.Contains("200") == true) updated++;
        }
        return (created, updated);
    }

    /// <summary>
    /// [EN] Verify data on server by searching for test-data tagged patients.
    /// [CN] 通过搜索带测试数据标签的患者验证服务器上的数据。
    /// </summary>
    public async Task VerifyOnServer(int expectedCount)
    {
        Console.WriteLine("[Verify] Fetching updated resources from server...");

        var query = new SearchParams()
            .Where("_tag=SUBSET")
            .OrderBy("-_lastUpdated")
            .LimitTo(expectedCount);

        var searchResult = await _client.SearchAsync<Patient>(query);

        if (searchResult.Entry.Count > 0)
        {
            Console.WriteLine($"[Verify] Confirmed {searchResult.Entry.Count} records on server:");
            foreach (var entry in searchResult.Entry)
            {
                var p = (Patient)entry.Resource;
                Console.WriteLine($" - Patient: {p.Name[0].Family}, Version: {p.Meta?.VersionId}, LastUpdated: {p.Meta?.LastUpdated}");
            }
        }
        else
        {
            Console.WriteLine("[Verify] No records found. Indexing might be delayed on public server.");
        }
    }
}

/// <summary>
/// [EN] Parameter validation helpers.
/// [CN] 参数验证辅助方法。
/// </summary>
internal static class Guard
{
    public static void NotNull(object? value, string name)
    {
        if (value is null)
            throw new ArgumentNullException(name, $"Parameter '{name}' must not be null.");
    }

    public static void NotNullOrEmpty(string? value, string name)
    {
        if (value is null)
            throw new ArgumentNullException(name, $"Parameter '{name}' must not be null.");
        if (value.Length == 0)
            throw new ArgumentException($"Parameter '{name}' must not be empty.", name);
    }
}


