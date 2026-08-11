using Shared_Library.Shared;
using Firely.Fhir.Validation;
using Hl7.Fhir.Model;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Specification.Terminology;

namespace Shared_Library.ResourceValidator;

/// <summary>
/// [EN] Service for FHIR resource validation using Firely SDK.
/// Validates resources against HL7 FHIR R4 specification rules.
/// [CN] 使用Firely SDK进行FHIR资源验证的服务。按HL7 FHIR R4规范规则验证资源。
/// </summary>
public sealed class ResourceValidationService
{
    private readonly Validator _validator;

    /// <summary>
    /// [EN] Initialize the validation service with FHIR R4 core specification.
    /// [CN] 使用FHIR R4核心规范初始化验证服务。
    /// </summary>
    public ResourceValidationService()
    {
        var coreSource = ZipSource.CreateValidationSource();
        _validator = new Validator(
            new CachedResolver(coreSource),
            new LocalTerminologyService(new CachedResolver(coreSource)),
            null,
            new ValidationSettings()
        );
    }

    /// <summary>
    /// [EN] Validate a FHIR Patient resource against R4 rules.
    /// Returns true if the resource is valid, false otherwise.
    /// [CN] 按R4规则验证FHIR Patient资源。资源有效返回true，否则返回false。
    /// </summary>
    public bool Validate(Patient patient)
    {
        Guard.NotNull(patient, nameof(patient));

        var result = _validator.Validate(patient);
        return result.Success;
    }

    /// <summary>
    /// [EN] Get detailed validation issues for a FHIR Patient resource.
    /// Each issue includes severity, diagnostics message, and location path.
    /// [CN] 获取FHIR Patient资源的详细验证问题。每个问题包含严重级别、诊断消息和位置路径。
    /// </summary>
    public List<(string Severity, string Diagnostics, string Location)> GetValidationIssues(Patient patient)
    {
        Guard.NotNull(patient, nameof(patient));

        var result = _validator.Validate(patient);
        return result.Issue.Select(i => (
            i.Severity.ToString().ToUpper(),
            i.Diagnostics ?? "Unknown issue",
            string.Join(", ", i.Location)
        )).ToList();
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
        return string.Join(Environment.NewLine, lines);
    }
}
