using HealthDataInteropSharedLibrary.Shared;
using Hl7.Fhir.Model;

namespace HealthDataInteropSharedLibrary.ResourceValidator;

/// <summary>
/// [EN] Known US Core Implementation Guide v7.1.0 StructureDefinition profile URIs.
/// Used to validate that FHIR resources claim conformance to US Core profiles.
/// Per ONC (g)(10) and 21st Century Cures Act, certified EHRs must support these profiles.
/// 
/// [CN] 已知的US Core实施指南v7.1.0 StructureDefinition配置档URI。
/// 用于验证FHIR资源声明符合US Core配置档。
/// 按照ONC (g)(10)和21世纪治愈法案，认证的EHR必须支持这些配置档。
/// </summary>
public static class UsCoreProfiles
{
    /// <summary>[EN] US Core Patient Profile / [CN] US Core患者配置档</summary>
    public const string Patient = "http://hl7.org/fhir/us/core/StructureDefinition/us-core-patient";

    /// <summary>[EN] US Core Observation Lab Profile / [CN] US Core实验室观察配置档</summary>
    public const string ObservationLab = "http://hl7.org/fhir/us/core/StructureDefinition/us-core-observation-lab";

    /// <summary>[EN] US Core Observation Vital Signs Profile / [CN] US Core生命体征观察配置档</summary>
    public const string ObservationVitalSigns = "http://hl7.org/fhir/us/core/StructureDefinition/us-core-vital-signs";

    /// <summary>[EN] US Core Encounter Profile / [CN] US Core就诊配置档</summary>
    public const string Encounter = "http://hl7.org/fhir/us/core/StructureDefinition/us-core-encounter";

    /// <summary>[EN] US Core Condition Profile / [CN] US Core病症配置档</summary>
    public const string Condition = "http://hl7.org/fhir/us/core/StructureDefinition/us-core-condition";

    /// <summary>[EN] US Core MedicationRequest Profile / [CN] US Core用药请求配置档</summary>
    public const string MedicationRequest = "http://hl7.org/fhir/us/core/StructureDefinition/us-core-medication-request";

    /// <summary>[EN] US Core AllergyIntolerance Profile / [CN] US Core过敏不耐受配置档</summary>
    public const string AllergyIntolerance = "http://hl7.org/fhir/us/core/StructureDefinition/us-core-allergyintolerance";

    /// <summary>
    /// [EN] Get the expected US Core profile URI for a given FHIR resource type.
    /// Returns null if no US Core profile is defined for that resource type.
    /// [CN] 获取给定FHIR资源类型的预期US Core配置档URI。如果该资源类型未定义US Core配置档，则返回null。
    /// </summary>
    public static string? GetProfileUriForType(string resourceTypeName)
    {
        return resourceTypeName switch
        {
            "Patient" => Patient,
            "Observation" => ObservationLab,
            "Encounter" => Encounter,
            "Condition" => Condition,
            "MedicationRequest" => MedicationRequest,
            "AllergyIntolerance" => AllergyIntolerance,
            _ => null
        };
    }

    /// <summary>
    /// [EN] Check all known US Core profile URIs. Returns a dictionary of profile name to URI.
    /// Useful for documentation and compliance reporting.
    /// [CN] 检查所有已知的US Core配置档URI。返回配置档名称到URI的字典。适用于文档和合规报告。
    /// </summary>
    public static Dictionary<string, string> GetAllProfiles()
    {
        return new Dictionary<string, string>
        {
            { "Patient", Patient },
            { "ObservationLab", ObservationLab },
            { "ObservationVitalSigns", ObservationVitalSigns },
            { "Encounter", Encounter },
            { "Condition", Condition },
            { "MedicationRequest", MedicationRequest },
            { "AllergyIntolerance", AllergyIntolerance }
        };
    }
}

/// <summary>
/// [EN] US Core profile conformance checker.
/// Validates that FHIR resources declare the expected US Core StructureDefinition in their Meta.Profile.
/// This checks that resources explicitly claim US Core conformance, which is required by ONC (g)(10).
/// For full structural validation, use the Firely SDK ResourceValidationService instead.
/// 
/// [CN] US Core配置档一致性检查器。
/// 验证FHIR资源在其Meta.Profile中声明预期的US Core StructureDefinition。
/// 检查资源是否明确声明US Core一致性，这是ONC (g)(10)所要求的。
/// 对于完整的结构验证，请改用Firely SDK ResourceValidationService。
/// </summary>
public static class UsCoreConformanceChecker
{
    /// <summary>
    /// [EN] Result of a US Core conformance check.
    /// [CN] US Core一致性检查结果。
    /// </summary>
    public sealed class ConformanceResult
    {
        /// <summary>[EN] Whether the resource conforms to a US Core profile / [CN] 资源是否符合US Core配置档</summary>
        public bool IsUsCoreConformant { get; set; }

        /// <summary>[EN] The resource type being checked / [CN] 被检查的资源类型</summary>
        public string ResourceType { get; set; } = string.Empty;

        /// <summary>[EN] Expected US Core profile URI / [CN] 预期的US Core配置档URI</summary>
        public string? ExpectedProfileUri { get; set; }

        /// <summary>[EN] Profile URIs actually declared on the resource / [CN] 资源上实际声明的配置档URI</summary>
        public List<string> DeclaredProfiles { get; set; } = new();

        /// <summary>[EN] Human-readable message about the result / [CN] 关于结果的可读消息</summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// [EN] Check if a FHIR Patient resource declares the US Core Patient profile in its Meta.
    /// [CN] 检查FHIR Patient资源是否在其Meta中声明US Core患者配置档。
    /// </summary>
    public static ConformanceResult CheckPatientConformance(Patient patient)
    {
        Guard.NotNull(patient, nameof(patient));

        var expected = UsCoreProfiles.Patient;
        var declared = GetDeclaredProfiles(patient);

        return new ConformanceResult
        {
            ResourceType = "Patient",
            ExpectedProfileUri = expected,
            DeclaredProfiles = declared,
            IsUsCoreConformant = declared.Contains(expected),
            Message = declared.Contains(expected)
                ? "Patient resource conforms to US Core Patient profile."
                : $"Patient resource does NOT declare US Core profile: {expected}"
        };
    }

    /// <summary>
    /// [EN] Generic conformance check for any FHIR resource type that has a known US Core profile.
    /// Uses the resource's CLR type name to look up the expected US Core profile URI.
    /// Returns null if the resource type has no defined US Core profile.
    /// [CN] 对任何具有已知US Core配置档的FHIR资源类型的通用一致性检查。
    /// 使用资源的CLR类型名称查找预期的US Core配置档URI。如果资源类型没有定义的US Core配置档，则返回null。
    /// </summary>
    public static ConformanceResult? CheckResourceConformance(Resource resource)
    {
        Guard.NotNull(resource, nameof(resource));

        var typeName = GetResourceTypeName(resource);
        var expectedUri = UsCoreProfiles.GetProfileUriForType(typeName);

        if (expectedUri is null)
            return null;

        var declared = GetDeclaredProfiles(resource);

        return new ConformanceResult
        {
            ResourceType = typeName,
            ExpectedProfileUri = expectedUri,
            DeclaredProfiles = declared,
            IsUsCoreConformant = declared.Contains(expectedUri),
            Message = declared.Contains(expectedUri)
                ? $"{typeName} conforms to US Core profile: {expectedUri}"
                : $"{typeName} does NOT declare US Core profile: {expectedUri}"
        };
    }

    /// <summary>
    /// [EN] Extract declared profile URIs from a FHIR resource's Meta element.
    /// Returns an empty list if the resource has no Meta or no profiles declared.
    /// [CN] 从FHIR资源的Meta元素中提取声明的配置档URI。如果资源没有Meta或没有声明配置档，则返回空列表。
    /// </summary>
    public static List<string> GetDeclaredProfiles(Resource resource)
    {
        Guard.NotNull(resource, nameof(resource));

        if (resource.Meta?.Profile is null)
            return new List<string>();

        // Meta.Profile is a nullable list; filter out nulls and empties
        return resource.Meta.Profile
            .Where(p => !string.IsNullOrEmpty(p))
            .Select(p => p!)
            .ToList();
    }

    /// <summary>
    /// [EN] Ensure a Patient resource has the US Core profile declared in its Meta.
    /// If not already present, adds the profile URI to the resource's Meta.
    /// Returns true if the profile was added, false if it was already present.
    /// [CN] 确保Patient资源的Meta中声明了US Core配置档。如果尚未存在，则将配置档URI添加到资源的Meta中。
    /// 如果配置档已添加则返回true，如果已经存在则返回false。
    /// </summary>
    public static bool EnsureUsCoreProfile(Patient patient)
    {
        Guard.NotNull(patient, nameof(patient));

        var profileUri = UsCoreProfiles.Patient;

        if (patient.Meta is null)
            patient.Meta = new Meta();

        // Initialize Profile list if null
        var profiles = (patient.Meta.Profile?.ToList()) ?? new List<string?>();

        if (!profiles.Contains(profileUri))
        {
            profiles.Add(profileUri);
            patient.Meta.Profile = profiles;
            return true;
        }

        return false;
    }

    /// <summary>
    /// [EN] Get the resource type name from a FHIR Resource by examining its CLR type.
    /// For example, returns "Patient" for Hl7.Fhir.Model.Patient instances.
    /// [CN] 通过检查CLR类型从FHIR资源获取资源类型名称。例如，对Hl7.Fhir.Model.Patient实例返回"Patient"。
    /// </summary>
    private static string GetResourceTypeName(Resource resource)
    {
        // Firely SDK resource types have simple names like "Patient", "Observation", etc.
        var typeName = resource.GetType().Name;
        return typeName;
    }
}
