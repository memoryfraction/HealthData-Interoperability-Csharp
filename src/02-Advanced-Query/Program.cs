using Hl7.Fhir.Model; // 包含 FHIR 资源模型 (Required for Encounter, Patient etc.)
using Hl7.Fhir.Rest;  // 包含 REST 客户端和搜索参数 (Required for FhirClient, SearchParams)
using Task = System.Threading.Tasks.Task;

namespace _02_Advanced_Query
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			// 1. Initialize the FHIR Client
			// 1. 初始化 FHIR 客户端
			// Using Firely's public test server
			// 使用 Firely 提供的公共测试服务器
			var client = new FhirClient("https://server.fire.ly");

			// 2. Construct Advanced Search Parameters
			// 2. 构造高级查询逻辑
			var q = new SearchParams();

			// Task A: Chained Parameters
			// 任务 A: 链式查询
			// Find all Encounters where the participating Practitioner's name contains "Smith"
			// 查找所有参与医生（Practitioner）姓名包含 "Smith" 的就诊记录（Encounter）
			q.Where("participant.individual.name:contains=Smith");

			// Task B: Forward Include (_include)
			// 任务 B: 正向包含
			// In addition to the Encounter, fetch the Patient resource it references
			// 在获取就诊记录的同时，顺便取回其引用的患者（Patient）资源
			q.Include("Encounter:patient");

			// Task C: Reverse Include (_revinclude)
			// 任务 C: 反向包含
			// Fetch all Observation resources that point to the Patient found in this search
			// 获取所有引用了本次查询中患者资源的观察指标（Observation）记录
			// Note: In SDK, 'true' indicates a Reverse Include (revinclude)
			// 注意：在 SDK 中，第二个参数设为 'true' 表示执行反向包含
			q.Include("Observation:patient");

			// 3. Execute the search and process the Bundle
			// 3. 执行搜索并处理 Bundle 结果
			// SearchAsync returns a Bundle containing Encounters and the included resources
			// SearchAsync 返回一个包含 Encounter 及其关联资源的 Bundle 集合
			var results = await client.SearchAsync<Encounter>(q);

			Console.WriteLine($"--- Search Results ---");

			if (results.Entry.Count == 0)
			{
				Console.WriteLine("No matching resources found.");
			}

			foreach (var entry in results.Entry)
			{
				// Print the type and ID of each resource found in the bundle
				// 打印 Bundle 中找到的每个资源的类型和 ID
				Console.WriteLine($"Resource found: {entry.Resource.TypeName}/{entry.Resource.Id}");
			}

			Console.WriteLine("--- Search Completed ---");
		}
	}
}