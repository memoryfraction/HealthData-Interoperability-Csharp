using Microsoft.SemanticKernel;
using HealthDataInteropSharedLibrary.AIDataValidator;

namespace _06_AI_Data_Validator;

/// <summary>
/// Entry point: Demonstrating AI-assisted FHIR data mapping and validation.
/// 入口点：演示AI辅助的FHIR数据映射和验证。
/// </summary>
internal static class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        Console.WriteLine("=== [AI-Assisted FHIR Data Mapping & Validation] ===");

        var builder = Kernel.CreateBuilder();
        builder.AddOllamaChatCompletion(
            modelId: "llama3",
            endpoint: new Uri("http://localhost:11434")
        );
        var kernel = builder.Build();

        string csvPath = Path.Combine(AppContext.BaseDirectory, "DataSamples", "dirty_patients.csv");
        if (!File.Exists(csvPath)) return;

        var aiProvider = async (string prompt) =>
        {
            var result = await kernel.InvokePromptAsync(prompt);
            return result.ToString().Trim();
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
                    Console.WriteLine($"[Verified] FHIR JSON:");
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