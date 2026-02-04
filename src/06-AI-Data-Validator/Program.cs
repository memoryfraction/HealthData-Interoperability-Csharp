/* * PROJECT: AI-Assisted Medical Data Validator
 * * FIX: Added Regular Expression cleaning to handle non-compliant JSON from Llama3.
 */

using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.SemanticKernel;
using System.Text.Json;
using System.Text.RegularExpressions; // 必须添加

namespace _06_AI_Data_Validator
{
	internal class Program
	{
		static async System.Threading.Tasks.Task Main(string[] args)
		{
			Console.WriteLine("=== [AI-Assisted FHIR Data Mapping & Validation] ===");

			var builder = Kernel.CreateBuilder();
			builder.AddOllamaChatCompletion(
				modelId: "llama3", // 确保你已经 ollama pull llama3
				endpoint: new Uri("http://localhost:11434")
			);
			var kernel = builder.Build();

			string csvPath = Path.Combine(AppContext.BaseDirectory, "DataSamples", "dirty_patients.csv");
			if (!File.Exists(csvPath)) return;

			var lines = File.ReadAllLines(csvPath).Skip(1);

			foreach (var line in lines)
			{
				Console.WriteLine($"\n[Raw Record]: {line}");

				// [EN] Prompt: Be extremely specific about the JSON structure.
				// [中] 提示词：对 JSON 结构进行极其严格的限定。
				string prompt = $$$"""
                    Task: Convert input to JSON.
                    Rules:
                    1. Fix spelling (Jhon -> John).
                    2. Gender must be: male, female, other, or unknown.
                    3. Output ONLY the JSON object. No conversation.
                    
                    Input: {{{line}}}
                    Example Output: {"id":"P001","name":"John Doe","dob":"1980-05-12","gender":"male"}
                    """;

				try
				{
					var aiResponse = await kernel.InvokePromptAsync(prompt);
					string rawResponse = aiResponse.ToString().Trim();

					// [EN] STEP 1: Use Regex to extract only the content inside { }.
					// [中] 步骤 1：使用正则仅截取 { } 内部的内容。
					var match = Regex.Match(rawResponse, @"\{.*\}", RegexOptions.Singleline);
					if (!match.Success) continue;

					string cleanJson = match.Value;

					// [EN] STEP 2: Loose Deserialization settings.
					// [中] 步骤 2：开启宽松的反序列化模式。
					var options = new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true,
						AllowTrailingCommas = true,
						ReadCommentHandling = JsonCommentHandling.Skip
					};

					var dto = JsonSerializer.Deserialize<PatientDto>(cleanJson, options);

					// 4. Clinical Guardrails (Safety Check)
					if (string.IsNullOrEmpty(dto.name) || (DateTime.TryParse(dto.dob, out DateTime dobDate) && dobDate > DateTime.Now))
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine($"[Rejected] Invalid Data: {dto.name}");
						Console.ResetColor();
						continue;
					}

					// 5. FHIR Mapping
					var patient = new Patient
					{
						Id = dto.id ?? "unknown",
						Name = new List<HumanName> { new HumanName { Family = dto.name } },
						BirthDate = dto.dob,
						Gender = Enum.TryParse<AdministrativeGender>(dto.gender, true, out var g) ? g : AdministrativeGender.Unknown
					};

					var serializer = new FhirJsonSerializer();
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine($"[Verified] FHIR JSON:");
					Console.WriteLine(serializer.SerializeToString(patient));
					Console.ResetColor();
				}
				catch (Exception ex)
				{
					Console.WriteLine($"[Error] JSON Error: {ex.Message}");
				}
				Console.WriteLine(new string('-', 60));
			}
		}
	}

	// [EN] Ensure this class is accessible and has all properties.
	// [中] 确保类包含 AI 可能返回的所有字段。
	public class PatientDto
	{
		public string id { get; set; }
		public string name { get; set; }
		public string dob { get; set; }
		public string gender { get; set; }
	}
}