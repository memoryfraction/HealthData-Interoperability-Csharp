using System.Text.Json;
using HealthDataInteropSharedLibrary.AIDataValidator;

namespace _06_AI_Data_Validator;

/// <summary>
/// Entry point: Demonstrating AI-assisted FHIR data mapping and validation.
/// Uses native HttpClient to call local Ollama server (no heavy framework dependencies).
/// 
/// [CN] 入口点：演示AI辅助的FHIR数据映射和验证。使用原生HttpClient调用本地Ollama服务器。
/// </summary>
internal static class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        Console.WriteLine("=== AI-Assisted FHIR Data Mapping & Validation ===");

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:11434/api/chat"),
            Timeout = TimeSpan.FromMinutes(2)
        };

        string csvPath = Path.Combine(AppContext.BaseDirectory, "DataSamples", "dirty_patients.csv");
        if (!File.Exists(csvPath))
        {
            Console.WriteLine($"[Info] CSV file not found at {csvPath}");
            return;
        }

        var aiProvider = async (string prompt) =>
        {
            var payload = new
            {
                model = "llama3",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                stream = false
            };

            var jsonContent = JsonSerializer.Serialize(payload);
            var request = new System.Net.Http.StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("http://localhost:11434/api/chat", request);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[Error] Ollama API returned {(int)response.StatusCode}: {response.ReasonPhrase}");
                return "";
            }

            var body = await response.Content.ReadAsStringAsync();
            using var doc = System.Text.Json.JsonDocument.Parse(body);
            return doc.RootElement.GetProperty("message").GetProperty("content").GetString()?.Trim() ?? "";
        };

        var service = new AiValidatorService(aiProvider);
        var lines = File.ReadAllLines(csvPath).Skip(1);

        foreach (var line in lines)
        {
            Console.WriteLine($"\n[Raw Record]: {line}");

            try
            {
                var patient = await service.ProcessRawRecordAsync(line);

                if (patient is not null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[Verified] FHIR JSON:");
                    Console.WriteLine(AiValidatorService.ToFhirJson(patient));
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Rejected] Invalid or unprocessable data.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Processing Error: {ex.Message}");
            }

            Console.WriteLine(new string('-', 60));
        }
    }
}
