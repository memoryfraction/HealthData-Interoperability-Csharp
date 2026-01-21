using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace HealthData.Interop.BasicClient
{
	internal class Program
	{
		/// <summary>
		/// Entry point: Demonstrating basic FHIR Patient creation and search.
		/// 入口点：演示基础的 FHIR Patient 资源创建与查询。
		/// </summary>
		static async System.Threading.Tasks.Task Main(string[] args)
		{
			Console.WriteLine("This is my first FHIR Client");

			// 1. Initialize a new Patient resource using the FHIR R4 model
			// 使用 FHIR R4 模型初始化一个新的患者（Patient）资源
			var patient = new Patient()
			{
				Name = new List<HumanName>()
				{
					new HumanName()
					{
						Given = new List<string>{"John","James" },
						Family = "Doe"
					}
				},
				Gender = AdministrativeGender.Male,
				BirthDate = "1990-01-01",
				Identifier = new List<Identifier>()
				{
					new Identifier()
					{
						Value = "123456790"
					}
				}
			};

			Console.WriteLine(
					$"Sending patient " +
					$"{patient.Name[0].Given.FirstOrDefault()} " +
					$"{patient.Name[0].Family} ...."
				);

			// 2. Initialize the FHIR Client pointing to a public test server (Fire.ly)
			// 初始化 FHIR 客户端，指向 Fire.ly 公开测试服务器
			var client = new FhirClient("http://server.fire.ly");

			try
			{
				// 3. Create the patient resource on the server asynchronously
				// 以异步方式在服务器上创建该患者资源
				await client.CreateAsync(patient);

				// 4. Search for patients with the name "John"
				// 查询名为 "John" 的患者
				var searchResult = await client.SearchAsync("Patient", new string[] { "name=John" });

				// 5. Iterate through the search results (Bundle entries)
				// 遍历查询结果（Bundle 入口）
				foreach (var result in searchResult.Entry)
				{
					// Cast the resource back to a Patient object
					// 将资源转换回 Patient 对象
					if (result.Resource is Patient pat)
					{
						Console.WriteLine($"Received patient: {pat.Name[0].Given.FirstOrDefault()} {pat.Name[0].Family}");
					}
				}
			}
			catch (FhirOperationException ex)
			{
				// Professional error handling for FHIR-specific issues
				// 针对 FHIR 特定问题的专业错误处理
				Console.WriteLine($"FHIR Error: {ex.Outcome}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"General Error: {ex.Message}");
			}
		}
	}
}