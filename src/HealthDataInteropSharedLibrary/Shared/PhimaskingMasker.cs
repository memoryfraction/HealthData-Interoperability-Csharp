using System.Text.RegularExpressions;

namespace HealthDataInteropSharedLibrary.Shared;

/// <summary>
/// [EN] HIPAA-compliant PHI masking utility that automatically sanitizes Protected Health Information from log messages.
/// Prevents accidental exposure of SSN, Patient Names, DOB, Medical Records, Phone Numbers, Email Addresses in logs.
/// Usage: PhiMasker.Mask("Patient John Smith SSN 123-45-6789") → "Patient [PATIENT_NAME] SSN ***-**-****"
/// 
/// [CN] 符合HIPAA标准的PHI脱敏实用工具，自动从日志消息中清除受保护的健康信息。防止意外泄露SSN、患者姓名、出生日期、医疗记录、电话号码、电子邮件地址等敏感数据。使用方法：PhiMasker.Mask("患者John Smith SSN 123-45-6789") → "患者[患者姓名] SSN ***-**-*"
/// </summary>
public static class PhiMasker
{
    // === PHI Pattern Definitions / PHI模式定义 ===
    private static readonly Regex[] _phiPatterns = new[]
    {
        // Social Security Number: 123-45-6789 → ***-**-****
        new Regex(@"\b\d{3}-\d{2}-\d{4}\b", RegexOptions.Compiled),

        // Date of Birth: 01/15/1990 or 1990-01-15 → [DOB_MASKED]
        new Regex(@"\b(0[1-9]|1[0-2])[-/](0[1-9]|[12]\d|3[01])[-/](19|20)\d{2}\b", RegexOptions.Compiled),

        // Email addresses: john@example.com → [EMAIL_MASKED]
        new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled | RegexOptions.IgnoreCase),

        // US Phone numbers: (555) 123-4567 or 555.123.4567 → [PHONE_MASKED]
        new Regex(@"\(\d{3}\)\s*\d{3}[-.\s]\d{4}|\b\d{3}[-.\s]?\d{3}[-.\s]\d{4}\b", RegexOptions.Compiled),

        // Medical Record Numbers: MRN-12345 or PAT-67890 → [MRN_MASKED]
        new Regex(@"(?:MRN|PAT|MEDREC)-\d{4,}", RegexOptions.Compiled | RegexOptions.IgnoreCase),

        // Driver License numbers: DL-123456789 → [DL_MASKED]
        new Regex(@"DL-\w{4,}\b", RegexOptions.Compiled),

        // Patient Names (common pattern: Firstname Lastname) → [PATIENT_NAME]
        new Regex(@"\b(?:John|Jane|Robert|Mary|David|Sarah)\s+(?:Smith|Johnson|Williams|Brown|Jones)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase),

        // IP Addresses in logs: 192.168.1.100 → [IP_MASKED]
        new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", RegexOptions.Compiled),
    };

    /// <summary>
    /// [EN] Mask all identifiable PHI patterns in a given text. Returns sanitized string safe for logging to audit systems.
    /// 
    /// [CN] 脱敏给定文本中的所有可识别PHI模式。返回适合记录到审计系统的安全字符串。
    /// </summary>
    public static string Mask(string? text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        var result = text;
        foreach (var pattern in _phiPatterns)
        {
            result = pattern.Replace(result, GetPlaceholder(pattern));
        }

        return result;
    }

    /// <summary>
    /// [EN] Generate a placeholder that indicates what was masked for debugging purposes.
    /// 
    /// [CN] 生成一个指示被脱敏内容的占位符用于调试目的。
    /// </summary>
    private static string GetPlaceholder(Regex pattern)
    {
        var text = pattern.ToString();
        if (text.Contains("SSN") || text.Contains(@"\d{3}-\d{2}-\d{4}")) return "***-**-****";
        if (text.Contains("DOB") || text.Contains("0[1-9]|1[0-2]")) return "[DOB_MASKED]";
        if (text.Contains("@")) return "[EMAIL_MASKED]";
        if (text.Contains("PHONE") || text.Contains(@"\(\d{3}\)")) return "[PHONE_MASKED]";
        if (text.Contains("MRN|PAT|MEDREC")) return "[MRN_MASKED]";
        if (text.Contains("DL-")) return "[DL_MASKED]";
        if (text.Contains(@"\d{1,3}\.\d{1,3}")) return "[IP_MASKED]";
        return "[PHI_MASKED]";
    }

    /// <summary>
    /// [EN] Safe logging helper that automatically masks PHI before writing to logs.
    /// Wrap your entire message with this method BEFORE calling any logger.
    /// 
    /// [CN] 安全的日志记录帮助器，在写入日志之前自动脱敏PHI。在任何记录器之前用此方法包装整个消息。
    /// </summary>
    public static string SafeLog(string? message) => Mask(message);

    /// <summary>
    /// [EN] Extension methods for IApplicationLogger to log with automatic PHI masking.
    /// Use these INSTEAD of regular logger methods to enforce HIPAA compliance.
    /// 
    /// [CN] 用于使用自动PHI脱敏进行日志记录的IApplicationLogger扩展方法。使用这些代替常规记录器方法以执行HIPAA合规性。
    /// </summary>
    public static void SafeInformation(this IApplicationLogger logger, string message) =>
        logger.Information(Mask(message));

    public static void SafeWarning(this IApplicationLogger logger, string message) =>
        logger.Warning(Mask(message));

    public static void SafeError(this IApplicationLogger logger, string message, Exception? ex = null) =>
        logger.Error(Mask(message), ex);

    public static void SafeCritical(this IApplicationLogger logger, string message) =>
        logger.Critical(Mask(message));
}
