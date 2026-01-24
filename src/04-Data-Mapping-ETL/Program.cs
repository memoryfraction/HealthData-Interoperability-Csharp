/*
 * Purpose: 
 * This module implements a full ETL pipeline to convert legacy CSV data into FHIR R4 Bundles.
 * It uses "Conditional Update" and includes a verification step to confirm success.
 * 目的：
 * 本模块实现了一个完整的 ETL 流水线。它使用“条件更新”并包含验证步骤以确认成功。
 *
 * Tested Features / 测试内容:
 * 1. Extraction: CSV parsing using CsvHelper. (CSV 解析)
 * 2. Idempotency: Conditional PUT to avoid duplicates. (使用条件 PUT 避免重复)
 * 3. Verification: Automated search to confirm server state. (自动化查询以确认服务器状态)
 *
 * Expected Results / 预期结果:
 * 1. First run: 3 Created. Second run: 3 Updated. (首次运行：新建3个；后续运行：更新3个)
 * 2. Console lists verified patient names and version IDs. (控制台列出验证过的患者姓名和版本号)
 */

using System.Globalization;
using CsvHelper;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using _04_Data_Mapping_ETL;

namespace _04_Data_Mapping_ETL
{
	internal class Program
	{
		static async System.Threading.Tasks.Task Main(string[] args)
		{
			// --- 1. Configuration / 配置 ---
			const string fhirServerUrl = "https://hapi.fhir.org/baseR4";
			string csvPath = Path.Combine(AppContext.BaseDirectory, "Data", "legacy_patients.csv");
			const string idSystem = "http://example.org/test-ids";

			var client = new FhirClient(fhirServerUrl);
			var patientsToImport = new List<(string LegacyId, Patient Patient)>();

			Console.WriteLine("[ETL Process] Starting data mapping task...");

			try
			{
				// --- 2. Extract: Read CSV / 提取：读取 CSV ---
				if (!File.Exists(csvPath))
				{
					throw new FileNotFoundException($"CSV file not found at: {csvPath}");
				}

				using (var reader = new StreamReader(csvPath))
				using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
				{
					var records = csv.GetRecords<LegacyPatientRecord>().ToList();
					Console.WriteLine($"[Extract] Read {records.Count} records from CSV.");

					foreach (var record in records)
					{
						var patient = new Patient
						{
							Meta = new Meta { Tag = new List<Coding> { new Coding("http://terminology.hl7.org/CodeSystem/v3-ObservationValue", "SUBSET", "Test Data") } },
							Identifier = new List<Identifier> { new Identifier(idSystem, record.Id) },
							Name = new List<HumanName> { new HumanName().WithGiven($"{record.FirstName}-Test").AndFamily($"{record.LastName} [TEST]") },
							Gender = record.Gender.ToLower() switch { "male" => AdministrativeGender.Male, "female" => AdministrativeGender.Female, _ => AdministrativeGender.Unknown },
							BirthDate = record.BirthDate,
							Telecom = string.IsNullOrWhiteSpace(record.Phone) ? null : new List<ContactPoint> { new ContactPoint(ContactPoint.ContactPointSystem.Phone, null, record.Phone) }
						};
						patientsToImport.Add((record.Id, patient));
					}
				}

				// --- 3. Load: Conditional Update / 加载：条件更新 ---
				var batchBundle = new Bundle { Type = Bundle.BundleType.Transaction };
				foreach (var item in patientsToImport)
				{
					batchBundle.Entry.Add(new Bundle.EntryComponent
					{
						Resource = item.Patient,
						Request = new Bundle.RequestComponent
						{
							Method = Bundle.HTTPVerb.PUT,
							Url = $"Patient?identifier={idSystem}|{item.LegacyId}"
						}
					});
				}

				Console.WriteLine("[Load] Sending Transaction Bundle...");
				var response = await client.TransactionAsync(batchBundle);

				// Analyze Response Status / 分析响应状态
				int created = 0;
				int updated = 0;
				foreach (var entry in response.Entry)
				{
					if (entry.Response.Status.Contains("201")) created++;
					else if (entry.Response.Status.Contains("200")) updated++;
				}
				Console.WriteLine($"[Success] Load completed. Created: {created}, Updated: {updated}.");

				// --- 4. Verify: Automated Query / 验证：自动化查询 ---
				Console.WriteLine("[Verify] Fetching updated resources from server for confirmation...");

				// Search by tag and order by last updated
				// 按标签查询并按最后更新时间排序
				var query = new SearchParams().Where("_tag=SUBSET").OrderBy("-_lastUpdated").LimitTo(patientsToImport.Count);
				Bundle searchResult = await client.SearchAsync<Patient>(query);

				if (searchResult.Entry.Count > 0)
				{
					Console.WriteLine($"[Verify] Confirmed {searchResult.Entry.Count} records on server:");
					foreach (var entry in searchResult.Entry)
					{
						var p = (Patient)entry.Resource;
						// VersionId helps confirm if it's a new version
						// VersionId 助于确认是否产生了新版本
						Console.WriteLine($" - Patient: {p.Name[0].Family}, Version: {p.Meta.VersionId}, LastUpdated: {p.Meta.LastUpdated}");
					}
				}
				else
				{
					Console.WriteLine("[Verify] No records found. Indexing might be delayed on public server.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[Error] {ex.GetType().Name}: {ex.Message}");
			}
			finally
			{
				Console.WriteLine("\nPress any key to exit...");
				Console.ReadKey();
			}
		}
	}
}