using HealthDataInteropSharedLibrary.Shared;
using Hl7.Fhir.Model;

namespace HealthDataInteropSharedLibrary.DriftDetection;

/// <summary>
/// [EN] A single field where the legacy source of truth and the synced FHIR resource disagree.
/// [CN] 一个字段级差异：遗留源系统与已同步的FHIR资源不一致。
/// </summary>
/// <param name="FieldName">[EN] Name of the drifted field (e.g. "FirstName", "Phone"). / [CN] 发生漂移的字段名称（如"FirstName"、"Phone"）</param>
/// <param name="SourceValue">[EN] Value from the legacy source of truth. / [CN] 遗留源系统中的字段值</param>
/// <param name="TargetValue">[EN] Value currently stored on the FHIR server. / [CN] FHIR服务器上当前存储的字段值</param>
public sealed record DriftField(string FieldName, string SourceValue, string TargetValue);

/// <summary>
/// [EN] Drift result for one patient: whether it exists on the FHIR server and which fields, if any, disagree.
/// [CN] 单个患者的漂移结果：是否存在于FHIR服务器上，以及哪些字段（如有）不一致。
/// </summary>
/// <param name="LegacyId">[EN] Unique identifier of the patient in the legacy source system. / [CN] 患者在遗留源系统中的唯一标识符</param>
/// <param name="FoundInTarget">[EN] True when a matching Patient resource exists on the FHIR server; false when missing. / [CN] FHIR服务器上存在匹配Patient资源时为true；缺失时为false</param>
/// <param name="DriftedFields">[EN] List of fields whose values disagree between source and target (empty when in sync). / [CN] 源系统与目标之间值不一致的字段列表（一致时为空）</param>
public sealed record PatientDriftResult(string LegacyId, bool FoundInTarget, IReadOnlyList<DriftField> DriftedFields)
{
    /// <summary>[EN] True when the record is missing on the target or has at least one drifted field. / [CN] 目标缺失或存在字段漂移时为true</summary>
    public bool HasDrift => !FoundInTarget || DriftedFields.Count > 0;
}

/// <summary>
/// [EN] Aggregate drift report across a batch of patients.
/// [CN] 一批患者的聚合漂移报告。
/// </summary>
/// <param name="RecordsChecked">[EN] Total number of legacy records compared. / [CN] 比对的遗留记录总数</param>
/// <param name="RecordsInSync">[EN] Records that exist on the server with no field-level differences. / [CN] 存在于服务器且无字段级差异的记录数</param>
/// <param name="RecordsWithFieldDrift">[EN] Records that exist on the server but have at least one drifted field. / [CN] 存在于服务器但至少有一个字段漂移的记录数</param>
/// <param name="RecordsMissingInTarget">[EN] Records not found on the server (never synced or deleted). / [CN] 服务器上未找到的记录数（从未同步或已被删除）</param>
/// <param name="Results">[EN] Per-patient drift results in original order. / [CN] 按原始顺序排列的单个患者漂移结果</param>
public sealed record DriftReport(
    int RecordsChecked,
    int RecordsInSync,
    int RecordsWithFieldDrift,
    int RecordsMissingInTarget,
    IReadOnlyList<PatientDriftResult> Results)
{
    /// <summary>
    /// [EN] Build a summary report from individual per-patient results.
    /// [CN] 从单个患者结果构建汇总报告。
    /// </summary>
    public static DriftReport Summarize(IReadOnlyList<PatientDriftResult> results)
    {
        Guard.NotNull(results, nameof(results));

        var missing = results.Count(r => !r.FoundInTarget);
        var fieldDrift = results.Count(r => r.FoundInTarget && r.DriftedFields.Count > 0);
        var inSync = results.Count - missing - fieldDrift;

        return new DriftReport(results.Count, inSync, fieldDrift, missing, results);
    }
}

/// <summary>
/// [EN] Compares the legacy source of truth against the FHIR resource a hybrid sync pipeline (module 04) produced,
/// surfacing the data drift that Chapter 5/6 of "FHIR Architecture Decisions" calls out as the most underestimated
/// risk of the hybrid model: the FHIR copy silently falling out of sync with the system that still owns the data.
/// This service never writes anything — detection only. Reconciliation (re-running the ETL, or a manual fix) is a
/// separate, deliberate step.
/// [CN] 将遗留源系统（真实数据所有者）与hybrid同步流水线（模块04）生成的FHIR资源做比对，
/// 揭示混合架构中最容易被低估的风险——FHIR副本与源系统悄然失步。本服务只做检测，不做任何写入；
/// 修复（重跑ETL或人工纠正）是单独、有意识的一步。
/// </summary>
public static class DriftDetectionService
{
    /// <summary>
    /// [EN] Compare one legacy record against its corresponding FHIR Patient (null when never synced or deleted).
    /// When <paramref name="stripTestNameMarkers"/> is true, strips the "-Test"/" [TEST]" suffixes that
    /// <c>FhirPatientMapper(testNameMarkers: true)</c> appends in the module 04 sandbox demo, so comparisons
    /// against that seeded data are not flagged as false-positive drift.
    /// [CN] 将一条遗留记录与其对应的FHIR Patient比对（从未同步或已被删除时target为null）。
    /// stripTestNameMarkers为true时，会去除模块04沙盒示例中FhirPatientMapper(testNameMarkers: true)
    /// 追加的"-Test"/" [TEST]"后缀，避免对该示例数据产生误报。
    /// </summary>
    /// <param name="source">[EN] Legacy record from the source of truth. / [CN] 来自源系统的遗留记录</param>
    /// <param name="target">[EN] Corresponding FHIR Patient, or null when not found on the server. / [CN] 对应的FHIR Patient，未找到时为null</param>
    /// <param name="stripTestNameMarkers">[EN] When true, strips sandbox test-name suffixes before comparing names. / [CN] 为true时，比对姓名前去除沙盒测试名称后缀</param>
    public static PatientDriftResult Compare(LegacyPatientRecord source, Patient? target, bool stripTestNameMarkers = false)
    {
        Guard.NotNull(source, nameof(source));

        if (target is null)
            return new PatientDriftResult(source.Id, FoundInTarget: false, DriftedFields: Array.Empty<DriftField>());

        var targetGiven = target.Name?.FirstOrDefault()?.Given?.FirstOrDefault()?.Trim() ?? string.Empty;
        var targetFamily = target.Name?.FirstOrDefault()?.Family?.Trim() ?? string.Empty;

        if (stripTestNameMarkers)
        {
            targetGiven = StripSuffix(targetGiven, "-Test");
            targetFamily = StripSuffix(targetFamily, " [TEST]");
        }

        var drifted = new List<DriftField>();

        var firstNameDrift = CompareFirstName(source, targetGiven);
        if (firstNameDrift is not null) drifted.Add(firstNameDrift);

        var lastNameDrift = CompareLastName(source, targetFamily);
        if (lastNameDrift is not null) drifted.Add(lastNameDrift);

        var genderDrift = CompareGender(source, target);
        if (genderDrift is not null) drifted.Add(genderDrift);

        var birthDateDrift = CompareBirthDate(source, target);
        if (birthDateDrift is not null) drifted.Add(birthDateDrift);

        var phoneDrift = ComparePhone(source, target);
        if (phoneDrift is not null) drifted.Add(phoneDrift);

        return new PatientDriftResult(source.Id, FoundInTarget: true, DriftedFields: drifted);
    }

    /// <summary>[EN] Compares the FirstName field; returns a DriftField when values differ, null when they match. / [CN] 比对FirstName字段；值不同时返回DriftField，一致时返回null</summary>
    private static DriftField? CompareFirstName(LegacyPatientRecord source, string targetGiven)
    {
        var sourceValue = source.FirstName.Trim();
        if (!string.Equals(sourceValue, targetGiven, StringComparison.Ordinal))
            return new DriftField("FirstName", sourceValue, targetGiven);
        return null;
    }

    /// <summary>[EN] Compares the LastName field; returns a DriftField when values differ, null when they match. / [CN] 比对LastName字段；值不同时返回DriftField，一致时返回null</summary>
    private static DriftField? CompareLastName(LegacyPatientRecord source, string targetFamily)
    {
        var sourceValue = source.LastName.Trim();
        if (!string.Equals(sourceValue, targetFamily, StringComparison.Ordinal))
            return new DriftField("LastName", sourceValue, targetFamily);
        return null;
    }

    /// <summary>[EN] Compares the Gender field; returns a DriftField when values differ, null when they match. / [CN] 比对Gender字段；值不同时返回DriftField，一致时返回null</summary>
    private static DriftField? CompareGender(LegacyPatientRecord source, Patient target)
    {
        var sourceGender = GenderNormalizer.Normalize(source.Gender);
        var targetGender = target.Gender ?? AdministrativeGender.Unknown;
        if (sourceGender != targetGender)
            return new DriftField("Gender", sourceGender.ToString(), targetGender.ToString());
        return null;
    }

    /// <summary>[EN] Compares the BirthDate field; returns a DriftField when values differ, null when they match. / [CN] 比对BirthDate字段；值不同时返回DriftField，一致时返回null</summary>
    private static DriftField? CompareBirthDate(LegacyPatientRecord source, Patient target)
    {
        var sourceValue = source.BirthDate.Trim();
        var targetValue = target.BirthDate ?? string.Empty;
        if (!string.Equals(sourceValue, targetValue, StringComparison.Ordinal))
            return new DriftField("BirthDate", sourceValue, targetValue);
        return null;
    }

    /// <summary>[EN] Compares the Phone field; returns a DriftField when values differ, null when they match. / [CN] 比对Phone字段；值不同时返回DriftField，一致时返回null</summary>
    private static DriftField? ComparePhone(LegacyPatientRecord source, Patient target)
    {
        var sourceValue = string.IsNullOrWhiteSpace(source.Phone) ? null : source.Phone.Trim();
        var targetValue = target.Telecom?
            .FirstOrDefault(t => t.System == ContactPoint.ContactPointSystem.Phone)?
            .Value?.Trim();

        if (!string.Equals(sourceValue, targetValue, StringComparison.Ordinal))
            return new DriftField("Phone", sourceValue ?? "(none)", targetValue ?? "(none)");
        return null;
    }

    private static string StripSuffix(string value, string suffix)
    {
        return value.EndsWith(suffix, StringComparison.Ordinal)
            ? value[..^suffix.Length]
            : value;
    }
}
