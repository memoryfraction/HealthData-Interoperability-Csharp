using Hl7.Fhir.Model;

namespace Shared_Library;

/// <summary>
/// [EN] Normalizes raw gender strings (from CSV/AI output) to FHIR AdministrativeGender enum.
/// Handles case-insensitivity, single-letter variants ("M"/"F"), and unrecognized values.
/// [CN] 将原始性别字符串（来自CSV/AI输出）标准化为FHIR AdministrativeGender枚举。
/// 处理大小写不敏感、单字母变体（"M"/"F"）和无法识别的值。
/// </summary>
public static class GenderNormalizer
{
    /// <summary>
    /// [EN] Maps a raw gender string to the corresponding FHIR AdministrativeGender.
    /// Returns Unknown for null, whitespace, or unrecognized input.
    /// [CN] 将原始性别字符串映射到对应的FHIR AdministrativeGender。
    /// 对于null、空白或无法识别的输入返回Unknown。
    /// </summary>
    public static AdministrativeGender Normalize(string? gender)
    {
        if (string.IsNullOrWhiteSpace(gender))
            return AdministrativeGender.Unknown;

        var value = gender.Trim().ToLowerInvariant();
        return value switch
        {
            "male" or "m" or "man" => AdministrativeGender.Male,
            "female" or "f" or "woman" => AdministrativeGender.Female,
            _ => AdministrativeGender.Unknown
        };
    }

    /// <summary>
    /// [EN] Maps a FHIR AdministrativeGender back to a human-readable display string.
    /// Useful for debugging / logging.
    /// [CN] 将FHIR AdministrativeGender映射回可读的显示字符串。
    /// 用于调试/日志记录。
    /// </summary>
    public static string ToDisplayString(AdministrativeGender gender)
    {
        return gender switch
        {
            AdministrativeGender.Male => "Male",
            AdministrativeGender.Female => "Female",
            AdministrativeGender.Other => "Other",
            _ => "Unknown"
        };
    }
}
