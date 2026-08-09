using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace _06_AI_Data_Validator;

/// <summary>
/// [EN] Service for AI-assisted FHIR data mapping and validation.
/// Cleans dirty CSV records using LLM output, applies clinical guardrails, and produces valid FHIR Patient resources.
/// [CN] AI辅助FHIR数据映射和验证服务。使用LLM输出清理脏CSV记录，应用临床保护规则，生成有效的FHIR Patient资源。
/// </summary>
public sealed class AiValidatorService
{
    private readonly Func<string, Task<string>> _aiProvider;

    /// <summary>
    /// [EN] Initialize with an AI text provider function (e.g., SemanticKernel prompt execution).
    /// The provided function takes a prompt string and returns the AI response.
    /// [CN] 使用AI文本提供器函数初始化（如SemanticKernel提示执行）。提供的函数接收提示字符串并返回AI响应。
    /// </summary>
    public AiValidatorService(Func<string, Task<string>> aiProvider)
    {
        Guard.NotNull(aiProvider, nameof(aiProvider));
        _aiProvider = aiProvider;
    }

    /// <summary>
    /// [EN] Process a raw CSV line through the AI pipeline: AI normalization -> JSON extraction -> FHIR mapping.
    /// Returns null if the record cannot be processed or fails clinical guardrails.
    /// [CN] 通过AI流水线处理原始CSV行：AI标准化 -> JSON提取 -> FHIR映射。
    /// 如果记录无法处理或不符合临床保护规则则返回null。
    /// </summary>
    public async Task<Patient?> ProcessRawRecordAsync(string rawLine)
    {
        Guard.NotNullOrEmpty(rawLine, nameof(rawLine));

        // Always use default prompt template
        string prompt = BuildDefaultPrompt(rawLine);

        try
        {
            var aiResponse = await _aiProvider(prompt);
            string cleanJson = ExtractJson(aiResponse);

            if (string.IsNullOrEmpty(cleanJson))
                return null;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            var dto = JsonSerializer.Deserialize<PatientDto>(cleanJson, options);

            if (!ClinicalGuardrails.Validate(dto))
                return null;

            return MapToPatient(dto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] JSON Error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// [EN] Format FHIR Patient as JSON string for display.
    /// [CN] 将FHIR Patient格式化为JSON字符串以供显示。
    /// </summary>
    public static string ToFhirJson(Patient patient)
    {
        Guard.NotNull(patient, nameof(patient));

        var serializer = new FhirJsonSerializer();
        return serializer.SerializeToString(patient);
    }

    /// <summary>
    /// [EN] Default prompt template for converting raw CSV lines to structured JSON.
    /// [CN] 将原始CSV行转换为结构化JSON的默认提示模板。
    /// </summary>
    public static string BuildDefaultPrompt(string inputLine)
    {
        var exampleJson = "{\\\"id\\\":\\\"P001\\\",\\\"name\\\":\\\"John Doe\\\",\\\"dob\\\":\\\"1980-05-12\\\",\\\"gender\\\":\\\"male\\\"}";
        return "Task: Convert input to JSON." + Environment.NewLine +
               "Rules:" + Environment.NewLine +
               "1. Fix spelling (Jhon -> John)." + Environment.NewLine +
               "2. Gender must be: male, female, other, or unknown." + Environment.NewLine +
               "3. Output ONLY the JSON object. No conversation." + Environment.NewLine +
               Environment.NewLine +
               $"Input: {inputLine}" + Environment.NewLine +
               $"Example Output: {exampleJson}";
    }

    // ====== Internal helpers ======

    /// <summary>
    /// [EN] Use Regex to extract only the JSON object content from AI response.
    /// [CN] 使用正则仅从AI响应中提取JSON对象内容。
    /// </summary>
    private static string ExtractJson(string aiResponse)
    {
        var match = Regex.Match(aiResponse, @"\{.*\}", RegexOptions.Singleline);
        return match.Success ? match.Value : "";
    }

    /// <summary>
    /// [EN] Map validated DTO to FHIR Patient resource.
    /// [CN] 将已验证的DTO映射为FHIR Patient资源。
    /// </summary>
    private static Patient MapToPatient(PatientDto dto)
    {
        return new Patient
        {
            Id = dto.id ?? "unknown",
            Name = new List<HumanName> { new HumanName { Family = dto.name } },
            BirthDate = dto.dob,
            Gender = Enum.TryParse<AdministrativeGender>(dto.gender, true, out var g) ? g : AdministrativeGender.Unknown
        };
    }
}

/// <summary>
/// [EN] Clinical validation guardrails to ensure data quality before FHIR mapping.
/// Rejects records with missing names or future birth dates.
/// [CN] 临床验证保护规则，确保FHIR映射前的数据质量。拒绝缺失姓名或出生日期在未来的记录。
/// </summary>
public static class ClinicalGuardrails
{
    /// <summary>
    /// [EN] Validate that a patient DTO meets clinical safety requirements.
    /// Returns false if name is missing or birth date is in the future.
    /// [CN] 验证患者DTO是否符合临床安全要求。如果姓名为空或出生日期在未来则返回false。
    /// </summary>
    public static bool Validate(PatientDto? dto)
    {
        if (dto is null)
            return false;

        if (string.IsNullOrEmpty(dto.name))
            return false;

        if (DateTime.TryParse(dto.dob, out DateTime dobDate) && dobDate > DateTime.Now)
            return false;

        return true;
    }
}

/// <summary>
/// [EN] DTO for deserializing AI-generated patient JSON.
/// [CN] 反序列化AI生成的患者JSON的DTO。
/// </summary>
public class PatientDto
{
    public string? id { get; set; }
    public string? name { get; set; }
    public string? dob { get; set; }
    public string? gender { get; set; }
}

/// <summary>
/// [EN] Parameter validation helpers.
/// [CN] 参数验证辅助方法。
/// </summary>
internal static class Guard
{
    public static void NotNull(object? value, string name)
    {
        if (value is null)
            throw new ArgumentNullException(name, $"Parameter '{name}' must not be null.");
    }

    public static void NotNullOrEmpty(string? value, string name)
    {
        if (value is null)
            throw new ArgumentNullException(name, $"Parameter '{name}' must not be null.");
        if (value.Length == 0)
            throw new ArgumentException($"Parameter '{name}' must not be empty.", name);
    }
}


