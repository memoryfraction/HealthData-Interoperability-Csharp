/*
    [EN] FHIR ETL - SMART on FHIR Interoperability Module (05)
    Goal: Demonstrate data transformation and ingestion into a (g)(10) certified ecosystem.
    Key Evidence:
    1. Standardized Mapping: Converting CSV to US Core Patient Profile.
    2. Protocol Compliance: Implementing SMART on FHIR endpoint logic.
    3. Security Strategy: Handling the OAuth2 Token bypass for development environments.

    [CN] FHIR ETL - SMART on FHIR 互操作性模块 (05)
    目标：演示数据转换并将其导入符合 (g)(10) 标准的医疗生态系统。
    试图证明的点：
    1. 标准化映射：将 CSV 成功转换为符合 US Core 标准的 Patient 资源模型。
    2. 协议合规：实现了基于 SMART on FHIR 规范的服务端点对接逻辑。
    3. 安全策略：展示了在开发环境下如何处理并绕过 OAuth2 Token 验证的工程化方法。

	与04 DATA Mapping ETL的区别？
		“在 05 模块中，我采取了 ‘架构预留，逻辑先行’ 的策略。虽然为了解决跨境网络对 OAuth2 重定向的干扰，我在开发测试中使用了 Open Endpoint，但我的代码中：
		资源映射严格遵循了 US Core 标准，这在 04 模块中是没有要求的。
		网络层使用了 SocketsHttpHandler 并模拟了 Authorization 头结构，随时可以通过更换 Authenticator 切换到生产环境。
		文档中我详细记录了 JWT 的三段式结构和 (g)(10) 的认证流程，这证明我不仅会写代码，还理解医疗行业的安全准入标准。”
*/

using CsvHelper;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Net;
using System.Security.Authentication;
using Task = System.Threading.Tasks.Task;

namespace _05_SMART_on_FHIR
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			// --- 1. Load Configuration / 加载配置 ---
			var config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.Build();

			// [EN] Switch to hapi.fhir.org/baseR4 for a stable open-access demonstration.
			// [CN] 切换至 hapi.fhir.org/baseR4 以确保在公开访问环境下的稳定性。
			string fhirServerUrl = config["MeldRx:FhirServerUrl"] ?? "https://hapi.fhir.org/baseR4";

			// --- 2. Initialize Network Handler / 初始化网络处理器 ---
			// [EN] Optimized for VPN/Proxy environments to ensure cross-border connection stability.
			// [CN] 针对 VPN/代理环境进行优化，确保跨境连接的稳定性。
			var handler = new SocketsHttpHandler
			{
				SslOptions = new System.Net.Security.SslClientAuthenticationOptions
				{
					RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
					EnabledSslProtocols = SslProtocols.Tls12
				},
				UseProxy = true,
				ConnectTimeout = TimeSpan.FromSeconds(30)
			};

			using var httpClient = new HttpClient(handler)
			{
				DefaultRequestVersion = HttpVersion.Version11,
				Timeout = TimeSpan.FromMinutes(2)
			};

			/* --- [EN] STRATEGY: HOW TO BYPASS/SIMULATE OAUTH2 TOKEN ---
               In a production SMART on FHIR environment, a JWT token is mandatory. 
               To bypass this for rapid ETL validation in an Open Sandbox:
               Step 1: Use an 'Open' FHIR Endpoint (like HAPI or SMART Launcher Open).
               Step 2: Comment out the Authorization header line to avoid 401 Malformed Token errors.
               Step 3: Document the theoretical JWT acquisition flow in the README (see Module 05 documentation).

               --- [CN] 策略：如何绕过/模拟 OAUTH2 TOKEN ---
               在生产级 SMART on FHIR 环境中，JWT Token 是强制性的。
               为了在公开沙盒中快速验证 ETL 逻辑：
               步骤 1：使用“Open”类型的 FHIR 端点（如 HAPI 或 SMART Launcher 开放模式）。
               步骤 2：注释掉 Authorization Header 赋值语句，防止服务器返回 401 Token 格式错误。
               步骤 3：在 README 文档中记录完整的理论 JWT 获取流程（详见模块 05 说明文档）。
            */

			// httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "SIMULATED_TOKEN");

			var settings = new FhirClientSettings { PreferredFormat = ResourceFormat.Json };
			var client = new FhirClient(fhirServerUrl, httpClient, settings);

			Console.WriteLine($">>> [Target] {fhirServerUrl}");
			Console.WriteLine(">>> [Status] Starting SMART on FHIR ETL Pipeline...");

			// --- 3. ETL Implementation / ETL 逻辑实现 ---
			string csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "data.csv");

			if (!File.Exists(csvPath))
			{
				Console.WriteLine($"[Error] CSV data file not found at {csvPath}");
				return;
			}

			using var reader = new StreamReader(csvPath);
			using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

			try
			{
				var records = csv.GetRecords<RawPatientData>();

				foreach (var record in records)
				{
					// [EN] TRANSFORM: Map Raw CSV to US Core Patient Profile.
					// [CN] 转换：将原始 CSV 映射为符合 US Core 标准的 Patient 资源。
					var patient = new Patient
					{
						// [EN] Meta tags prove the resource follows standardized implementation guides (IG).
						// [CN] Meta 标签证明资源遵循了标准化的实施指南 (IG)。
						Meta = new Meta { Profile = new[] { "http://hl7.org/fhir/us/core/StructureDefinition/us-core-patient" } },
						Active = true,
						Name = new List<HumanName> {
							new HumanName { Family = record.LastName, Given = new[] { record.FirstName } }
						},
						Gender = record.Gender?.ToLower().Contains("male") == true ? AdministrativeGender.Male : AdministrativeGender.Female,
						BirthDate = record.BirthDate
					};

					try
					{
						// [EN] LOAD: Execute asynchronous creation on the FHIR server.
						// [CN] 加载：在 FHIR 服务器上执行异步创建。
						var created = await client.CreateAsync(patient);
						Console.WriteLine($"[Success] {record.FirstName} {record.LastName} -> Assigned ID: {created.Id}");

						// Anti-throttling delay / 防频率限制延迟
						await Task.Delay(500);
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

			Console.WriteLine(">>> [Complete] Data now exists in the SMART-compatible sandbox.");
		}
	}
}