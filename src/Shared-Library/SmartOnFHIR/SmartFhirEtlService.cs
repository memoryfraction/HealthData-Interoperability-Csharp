using Shared_Library.Shared;
using CsvHelper;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Shared_Library;
using System.Globalization;
using Task = System.Threading.Tasks.Task;

namespace Shared_Library.SmartOnFHIR;

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
        // [EN] Module 05 pre-refactor created Patients without a business Identifier;
        // keep output parity by disabling the synthetic name-based Identifier.
        // [CN] 模块05重构前创建的Patient不带业务Identifier；
        // 关闭基于姓名拼接的合成Identifier以保持输出一致。
        _mapper = new FhirPatientMapper(usCoreProfile: true, addTestDataTag: false, addIdentifier: false);
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
            var records = csv.GetRecords<Shared_Library.SmartOnFHIR.RawPatientData>();

            foreach (var record in records)
            {
                var patient = _mapper.MapRaw(record);

                try
                {
                    var created = await _client.CreateAsync(patient);
                    Console.WriteLine($"[Success] {record.FirstName} {record.LastName} -> Assigned ID: {created.Id}");
                    count++;

                    if (delayMs.HasValue)
                        await Task.Delay(delayMs.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Failed] Import failed for {record.FirstName}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Critical] ETL Pipeline Failure: {ex.Message}");
        }

        return count;
    }
}
