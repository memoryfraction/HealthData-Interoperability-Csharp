using Firely.Fhir.Validation;
using Hl7.Fhir.Model;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Specification.Terminology;
using Hl7.Fhir.Validation;
using Task = System.Threading.Tasks.Task;


namespace _03_Resource_Validator
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("=== FHIR Resource Validation (Module 03) ===");

			try
			{
				// 1. Setup Source: Where the "dictionary" is
				// 设置源：定义文件的位置
				var coreSource = ZipSource.CreateValidationSource();
				var cachedSource = new CachedResolver(coreSource);

				// 2. Configure Settings and Initialize Validator
				// 注意：在 v6.x 中，Validator 实例需要 Resolver 和 TerminologyService
				var settings = new ValidationSettings();
				var validator = new Validator(
					cachedSource,
					new LocalTerminologyService(cachedSource),
					null,
					settings
				);

				// 3. Create Test Data with errors
				// 创建带有错误的测试数据
				var testPatient = new Patient
				{
					Active = true,
					BirthDate = "1990-13-45", // Error: Invalid month / 错误的月份
					Gender = AdministrativeGender.Male
				};
				testPatient.Telecom.Add(new ContactPoint { System = ContactPoint.ContactPointSystem.Phone }); // Error: Missing value

				// 4. Perform Validation
				// 执行验证
				Console.WriteLine("Validating patient against FHIR R4 rules...");
				var result = validator.Validate(testPatient);

				// 5. Output Results
				// 输出结果
				if (result.Success)
				{
					Console.WriteLine("✅ Result: Resource is valid!");
				}
				else
				{
					Console.WriteLine($"❌ Result: Found {result.Issue.Count} issues.");
					foreach (var issue in result.Issue)
					{
						Console.WriteLine($"[{issue.Severity.ToString().ToUpper()}] {issue.Diagnostics} (At: {string.Join(", ", issue.Location)})");
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Critical technical error: {ex.Message}");
			}
		}
	}
}