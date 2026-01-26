/*
	FHIR ETL Core Logic (Ref: https://youtu.be/UKTisHLeqn4)
	Core Logic: Simulate data retrieval from MeldRx and execute ETL operations.
	MeldRx Identity: A (g)(10) certified, fully-managed FHIR platform handling OAuth2 authentication and resource storage.
	核心业务逻辑：模拟从 MeldRx 获取数据并执行 ETL 操作。
	MeldRx 身份：一个符合 (g)(10) 标准的全托管 FHIR 平台，处理 OAuth2 认证与资源存储。
 */

using CsvHelper;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Net.Http.Headers;

namespace _05_SMART_on_FHIR
{
	internal class Program
	{
		/// <summary>
		/// Entry point for FHIR ETL simulation logic (Ref: https://youtu.be/UKTisHLeqn4).
		/// <para>Steps: 1. Load config from appsettings.json and User Secrets; 2. Init <see cref="FhirClient"/> with SSL Fix; 3. Transform CSV to <see cref="Patient"/> and Load to MeldRx.</para>
		/// <para>步骤：1. 从配置和 User Secrets 加载信息；2. 用 Token 初始化 FHIR 客户端并修复 SSL 验证问题；3. 将 CSV 转换并加载至 MeldRx。</para>
		/// </summary>
		static async System.Threading.Tasks.Task Main(string[] args)
		{
			// --- 1. Load Configuration (Including UserSecrets) ---
			var config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddUserSecrets<Program>() // Access sensitive data from User Secrets
				.Build();

			// 关键点：增加空值保护，防止配置未加载导致程序异常
			string fhirServerUrl = config["MeldRx:FhirServerUrl"] ?? throw new Exception("Missing 'MeldRx:FhirServerUrl' in configuration.");
			string accessToken = config["MeldRx:AccessToken"] ?? throw new Exception("Missing 'MeldRx:AccessToken' in User Secrets.");

			// --- 2. Initialize FHIR Client with SSL Fix (Ref [00:16:18]) ---
			// 修复：针对你遇到的 'The SSL connection could not be established' 报错
			// 在开发环境下，我们需要忽略证书验证以建立连接
			var handler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
			};

			using var httpClient = new HttpClient(handler);
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			var settings = new FhirClientSettings { PreferredFormat = ResourceFormat.Json };
			var client = new FhirClient(fhirServerUrl, httpClient, settings);

			Console.WriteLine(">>> Starting FHIR ETL Process for Test Personnel...");

			// --- 3. ETL Implementation ---
			// 修正：使用 Path.Combine 确保在不同 OS 或执行路径下都能找到文件
			string csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "data.csv");

			if (!File.Exists(csvPath))
			{
				Console.WriteLine($"Error: data.csv file not found at: {csvPath}");
				return;
			}

			using var reader = new StreamReader(csvPath);
			using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

			try
			{
				// 此处直接引用你单独类文件中的 RawPatientData
				var records = csv.GetRecords<RawPatientData>();

				foreach (var record in records)
				{
					// [Transform] Map to FHIR Patient (Ref [00:18:29])
					var patient = new Patient
					{
						Name = new List<HumanName>
						{
							new HumanName { Family = record.LastName, Given = new[] { record.FirstName } }
						},
						// 容错处理：不区分大小写，并处理空值
						Gender = record.Gender?.ToLower().Contains("male") == true ? AdministrativeGender.Male : AdministrativeGender.Female,
						BirthDate = record.BirthDate
					};

					try
					{
						// [Load] Push to MeldRx (Ref [00:17:21])
						var created = await client.CreateAsync(patient);
						Console.WriteLine($"[Success] Imported Patient: {record.FirstName} {record.LastName}, Assigned ID: {created.Id}");
					}
					catch (Exception ex)
					{
						// 增强：打印 InnerException 帮助定位网络/协议层面的具体原因
						Console.WriteLine($"[Failed] Error importing {record.FirstName}: {ex.Message}");
						if (ex.InnerException != null) Console.WriteLine($"   Detail: {ex.InnerException.Message}");
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Critical CSV Error: {ex.Message}");
			}

			Console.WriteLine(">>> ETL Task Completed.");
		}
	}
}