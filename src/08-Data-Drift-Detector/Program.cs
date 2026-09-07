using System.Globalization;
using CsvHelper;
using HealthDataInteropSharedLibrary.DriftDetection;
using HealthDataInteropSharedLibrary.Etl;
using HealthDataInteropSharedLibrary.Shared;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace _08_Data_Drift_Detector;

internal static class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        Console.WriteLine("=== Data Drift Detector (Module 08) ===");
        Console.WriteLine("Read-only reconciliation: compares the current legacy source of truth (CSV) against the synced FHIR copy.");
        Console.WriteLine("Run Module 04 first so the FHIR server has a baseline to compare against.");
        Console.WriteLine();

        const string fhirServerUrl = "https://hapi.fhir.org/baseR4";
        string csvPath = Path.Combine(AppContext.BaseDirectory, "Data", "legacy_patients_current.csv");

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

        try
        {
            var records = ReadLegacyRecords(csvPath);
            Console.WriteLine($">>> [Info] Loaded {records.Count} legacy record(s) from {Path.GetFileName(csvPath)}.");
            Console.WriteLine();

            var results = new List<PatientDriftResult>();
            foreach (var record in records)
            {
                var patient = await FindSyncedPatientAsync(client, record.Id);
                // [EN] Module 04 seeds this public server via FhirPatientMapper(testNameMarkers: true), so strip the "-Test"/" [TEST]"
                //     name suffixes before comparing to avoid false-positive name drift.
                // [CN] 模块04通过 FhirPatientMapper(testNameMarkers: true) 向该公共服务器写入数据，
                //     因此比对前需去除 "-Test"/" [TEST]" 姓名后缀，避免姓名误报漂移。
                var result = DriftDetectionService.Compare(record, patient, stripTestNameMarkers: true);
                results.Add(result);

                if (!result.FoundInTarget)
                {
                    Console.WriteLine($" - [{result.LegacyId}] MISSING on FHIR server (never synced or deleted).");
                }
                else if (result.DriftedFields.Count == 0)
                {
                    Console.WriteLine($" - [{result.LegacyId}] In sync.");
                }
                else
                {
                    Console.WriteLine($" - [{result.LegacyId}] DRIFT DETECTED ({result.DriftedFields.Count} field(s)):");
                    foreach (var field in result.DriftedFields)
                    {
                        Console.WriteLine($"     {field.FieldName}: legacy='{field.SourceValue}' vs fhir='{field.TargetValue}'");
                    }
                }
            }

            var report = DriftReport.Summarize(results);
            Console.WriteLine();
            Console.WriteLine("=== Drift Summary ===");
            Console.WriteLine($"Records checked:       {report.RecordsChecked}");
            Console.WriteLine($"In sync:               {report.RecordsInSync}");
            Console.WriteLine($"Field drift:           {report.RecordsWithFieldDrift}");
            Console.WriteLine($"Missing on server:     {report.RecordsMissingInTarget}");
            Console.WriteLine();

            if (report.RecordsWithFieldDrift + report.RecordsMissingInTarget > 0)
            {
                Console.WriteLine(">>> [Action] Drift or missing records detected. Re-run Module 04's ETL (or your sync pipeline) to reconcile, or fix the data manually.");
            }
            else
            {
                Console.WriteLine(">>> [Success] All records are in sync with the FHIR server.");
            }
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
            Console.WriteLine(">>> [Hint] Ensure Data/legacy_patients_current.csv is copied to the output directory.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> [Error] Unexpected error during drift detection: {ex.Message}");
        }
    }

    /// <summary>
    /// [EN] Reads the legacy patient CSV into typed records; throws FileNotFoundException up front when the file is missing.
    /// [CN] 将遗留患者CSV读取为类型化记录；文件缺失时提前抛出FileNotFoundException。
    /// </summary>
    static List<LegacyPatientRecord> ReadLegacyRecords(string csvPath)
    {
        if (!File.Exists(csvPath))
            throw new FileNotFoundException("Legacy patient CSV not found.", csvPath);

        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return csv.GetRecords<LegacyPatientRecord>().ToList();
    }

    /// <summary>
    /// [EN] Finds the synced Patient on the server by business identifier, or null when never synced / deleted.
    /// [CN] 通过业务标识符在服务器上查找已同步的Patient；从未同步或已删除时返回null。
    /// </summary>
    static async System.Threading.Tasks.Task<Patient?> FindSyncedPatientAsync(FhirClient client, string legacyId)
    {
        var query = new SearchParams().Where($"identifier={FhirPatientMapper.DefaultIdSystem}|{legacyId}");
        var searchResult = await client.SearchAsync<Patient>(query);

        if (searchResult.Entry.Count > 0)
        {
            var resource = searchResult.Entry[0].Resource;
            return resource is Patient patient ? patient : null;
        }

        return null;
    }
}
