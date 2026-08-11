using HealthDataInteropSharedLibrary.Shared;
using Firely.Fhir.Validation;
using Hl7.Fhir.Model;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Specification.Terminology;

namespace HealthDataInteropSharedLibrary.ResourceValidator;

/// <summary>
/// [EN] Service for FHIR resource validation using Firely SDK.
/// Validates resources against HL7 FHIR R4 specification rules.
/// 
/// SPEC LOADING / 规范加载:
/// [EN] The FHIR R4 specification (specification.zip, ~6MB) is embedded in this NuGet package as an
///     EmbeddedResource and copied to the output directory on build. ZipSource.CreateValidationSource()
///     looks for specification.zip in AppDomain.CurrentDomain.BaseDirectory. If found, full spec-based
///     validation is used. If not found (e.g., removed manually), the service gracefully falls back to
///     basic structural validation without errors or retries.
///     
///     Prior versions used Polly retries with exponential backoff, but this was incorrect because
///     ZipSource does NOT download spec.zip from the internet - it reads a local file. Retrying a missing
///     file 3 times with 14s cumulative delay was wasteful and confusing to users. The embedded approach
///     ensures reliable validation regardless of network availability.
/// 
/// IMPORTANT FOR NUGET CONSUMERS: Full FHIR R4 validation is included out-of-the-box.
/// No additional package references or manual downloads are required.
/// 
/// [CN] 使用Firely SDK进行FHIR资源验证的服务。specification.zip(~6MB)已嵌入NuGet包中，
///     构建时自动复制到输出目录。如果文件被手动删除，自动回退到基本结构验证。
/// </summary>
public sealed class ResourceValidationService
{
    private readonly object _lock = new object();
    private Validator? _validator;
    private bool _hasFullSpec;

    /// <summary>
    /// [EN] Initialize the validation service with FHIR R4 core specification.
    /// Attempts to load full spec from embedded specification.zip. Falls back gracefully if unavailable.
    /// </summary>
    public ResourceValidationService()
    {
        LoadSpecIfNeeded();
    }

    private void LoadSpecIfNeeded()
    {
        lock (_lock)
        {
            if (_hasFullSpec)
                return;

            try
            {
                var coreSource = ZipSource.CreateValidationSource();
                _validator = new Validator(
                    new CachedResolver(coreSource),
                    new LocalTerminologyService(new CachedResolver(coreSource)),
                    null,
                    new ValidationSettings()
                );
                _hasFullSpec = true;
                SafeConsole.WriteLine("[Info] Full FHIR R4 spec loaded successfully (specification.zip embedded).");
            }
            catch (Exception ex)
            {
                // spec.zip not found in output directory - fall back to basic validation
                _validator = null;
                _hasFullSpec = false;
                SafeConsole.WriteLine("[Info] FHIR specification unavailable. Using basic structural validation.");
                SafeConsole.WriteLine($"[Info] Reason: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// [EN] Check if full FHIR spec validation is available (spec.zip loaded).
    /// </summary>
    public bool HasFullSpec => _hasFullSpec;

    /// <summary>
    /// [EN] Attempt to reload the FHIR specification.
    /// Call this method if you added specification.zip to the output directory after initial startup.
    /// Returns true if the spec was successfully loaded, false otherwise.
    /// 
    /// [CN] 尝试重新加载FHIR规范。如果初始启动后添加了specification.zip，调用此方法。
    /// </summary>
    public bool TryEnsureSpecLoaded()
    {
        lock (_lock)
        {
            if (_hasFullSpec)
                return true;

            try
            {
                var coreSource = ZipSource.CreateValidationSource();
                _validator = new Validator(
                    new CachedResolver(coreSource),
                    new LocalTerminologyService(new CachedResolver(coreSource)),
                    null,
                    new ValidationSettings()
                );
                _hasFullSpec = true;
                SafeConsole.WriteLine("[Info] FHIR spec.zip loaded successfully. Full validation is now available.");
            }
            catch
            {
                SafeConsole.WriteLine("[Info] Basic structural validation will be used as fallback.");
                return false;
            }

            return _hasFullSpec;
        }
    }

    /// <summary>
    /// [EN] Validate a FHIR Patient resource against R4 rules.
    /// Falls back to basic structural validation if spec unavailable or validation fails.
    /// 
    /// [CN] 按R4规则验证FHIR Patient资源。如果规范不可用，执行基本结构验证。
    /// </summary>
    public bool Validate(Patient patient)
    {
        Guard.NotNull(patient, nameof(patient));

        if (_validator != null)
        {
            try
            {
                var result = _validator.Validate(patient);
                return result.Success;
            }
            catch
            {
                // Validation failed unexpectedly - fall back to basic validation
                return PerformBasicValidation(patient).Count == 0;
            }
        }

        return PerformBasicValidation(patient).Count == 0;
    }

    /// <summary>
    /// [EN] Get detailed validation issues for a FHIR Patient resource.
    /// Falls back to basic structural issues if spec unavailable or validation fails.
    /// 
    /// [CN] 获取FHIR Patient资源的详细验证问题。如果规范不可用，返回基本结构问题。
    /// </summary>
    public List<(string Severity, string Diagnostics, string Location)> GetValidationIssues(Patient patient)
    {
        Guard.NotNull(patient, nameof(patient));

        if (_validator != null)
        {
            try
            {
                var result = _validator.Validate(patient);
                return result.Issue.Select(i => (
                    i.Severity.ToString().ToUpper(),
                    i.Diagnostics ?? "Unknown issue",
                    string.Join(", ", i.Location)
                )).ToList();
            }
            catch
            {
                // Validation failed - fall back to basic issues
            }
        }

        // Fallback: basic structural validation results
        var basicIssues = PerformBasicValidation(patient);
        return basicIssues.Select(msg => ("INFO", msg, "FHIR R4")).ToList();
    }

    /// <summary>
    /// [EN] Basic structural validation without the full FHIR specification.
    /// Used as fallback when spec.zip is unavailable.
    /// 
    /// [CN] 不使用完整FHIR规范的基本结构验证。用于spec文件不可用的回退方案。
    /// </summary>
    private static List<string> PerformBasicValidation(Patient patient)
    {
        var issues = new List<string>();

        // Validate Gender enum
        var validGenders = new[] { "male", "female", "other", "unknown" };
        var genderStr = patient.Gender?.ToString()?.ToLower();
        if (genderStr != null && !validGenders.Contains(genderStr))
            issues.Add($"Invalid AdministrativeGender: {patient.Gender}");

        // Validate Telecom has required elements
        foreach (var telecom in patient.Telecom)
        {
            if (telecom.System.HasValue && string.IsNullOrEmpty(telecom.Value))
                issues.Add("ContactPoint has System but no Value - phone/email requires a value when system is specified");
        }

        return issues;
    }

    /// <summary>
    /// [EN] Format validation result for display.
    /// [CN] 格式化验证结果以供显示。
    /// </summary>
    public static string FormatValidationResult(bool isValid, int issueCount, List<(string Severity, string Diagnostics, string Location)> issues)
    {
        if (isValid)
            return "Resource is valid!";

        var lines = new List<string> { $"Found {issueCount} issues:" };
        foreach (var issue in issues)
        {
            lines.Add($"[{issue.Severity}] {issue.Diagnostics} (At: {issue.Location})");
        }
        return string.Join(System.Environment.NewLine, lines);
    }
}
