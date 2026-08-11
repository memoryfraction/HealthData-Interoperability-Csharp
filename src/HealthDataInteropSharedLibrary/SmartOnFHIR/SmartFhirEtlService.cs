using CsvHelper;
using HealthDataInteropSharedLibrary.Shared;
using Hl7.Fhir.Rest;
using System.Globalization;
using Task = System.Threading.Tasks.Task;

namespace HealthDataInteropSharedLibrary.SmartOnFHIR;

/// <summary>
/// [EN] SMART-on-FHIR ETL service: reads CSV patient data and imports into a FHIR server using shared mapper.
/// Encapsulates the full ETL pipeline with US Core profile support.
/// [CN] SMART-on-FHIR ETL服务：读取CSV患者数据并使用共享映射器导入FHIR服务器。
/// 封装支持US Core配置档的完整ETL流水线。
/// </summary>
public sealed class SmartFhirEtlService
{
    private readonly FhirClient _client;
    private readonly FhirPatientMapper _mapper;

    /// <summary>
    /// [EN] Initialize with a FHIR client. Uses shared mapper with US Core profile enabled.
    /// [CN] 使用FHIR客户端初始化。使用启用US Core配置档的共享映射器。
    /// </summary>
    public SmartFhirEtlService(FhirClient client)
    {
        Guard.NotNull(client, nameof(client));
        _client = client;
        _mapper = new FhirPatientMapper(usCoreProfile: true, addTestDataTag: false, addIdentifier: false);
    }

    /// <summary>
    /// [EN] Initialize with a FHIR client and custom identifier system.
    /// The custom idSystem allows each test run to use unique identifiers, preventing duplicate resource errors
    /// on shared public FHIR servers like hapi.fhir.org (HAPI-2840).
    /// 
    /// [CN] 使用FHIR客户端和自定义标识符系统初始化。
    /// 自定义idSystem允许每次测试运行使用唯一标识符，防止在共享公共FHIR服务器上出现重复资源错误。
    /// </summary>
    /// <param name="client">[EN] FHIR client / [CN] FHIR客户端</param>
    /// <param name="idSystem">[EN] Run-specific identifier system URI to avoid duplicates / [CN] 运行特定的标识符系统URI以避免重复</param>
    public SmartFhirEtlService(FhirClient client, string idSystem)
    {
        Guard.NotNull(client, nameof(client));
        Guard.NotNullOrEmpty(idSystem, nameof(idSystem));
        _client = client;
        // [EN] Use custom idSystem so each run generates unique identifiers.
        // Enable identifier so the system+value combination is unique per run.
        // [CN] 使用自定义idSystem使每次运行生成唯一标识符。
        _mapper = new FhirPatientMapper(idSystem: idSystem, usCoreProfile: true, addTestDataTag: false, addIdentifier: true);
    }

    /// <summary>
    /// [EN] Run the ETL pipeline: read CSV, transform to FHIR patients, and create on server.
    /// Returns count of successfully imported records.
    /// [CN] 运行ETL流水线：读取CSV、转换为FHIR患者、在服务器上创建。返回成功导入的记录数。
    /// </summary>
    public async Task<int> ImportPatientsAsync(string csvPath, int? delayMs = null)
    {
        Guard.NotNullOrEmpty(csvPath, nameof(csvPath));

        if (!File.Exists(csvPath))
            throw new FileNotFoundException($"CSV data file not found at {csvPath}");

        var count = 0;

        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        try
        {
            var records = csv.GetRecords<HealthDataInteropSharedLibrary.SmartOnFHIR.RawPatientData>();

            foreach (var record in records)
            {
                var patient = _mapper.MapRaw(record);

                try
                {
                    var created = await _client.CreateAsync(patient);
                    SafeConsole.WriteLine($"[Success] {record.FirstName} {record.LastName} -> Assigned ID: {created.Id}");
                    count++;

                    if (delayMs.HasValue)
                        await Task.Delay(delayMs.Value);
                }
                catch (Exception ex)
                {
                    SafeConsole.WriteLine($"[Failed] Import failed for {record.FirstName}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            SafeConsole.WriteLine($"[Critical] ETL Pipeline Failure: {ex.Message}");
        }

        return count;
    }
}
