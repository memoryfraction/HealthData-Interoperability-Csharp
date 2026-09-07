using HealthDataInteropSharedLibrary.DriftDetection;
using HealthDataInteropSharedLibrary.Shared;
using FluentAssertions;
using Hl7.Fhir.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HealthData.Interop.Tests.DriftDetectionTests;

/// <summary>
/// [EN] DriftDetectionService tests — legacy source of truth vs synced FHIR Patient comparison.
/// Covers: missing target, exact match, single/multiple field drift, test-name-marker stripping
/// (opt-in), null-argument validation, the female/male gender regression, and report summarization.
/// [CN] DriftDetectionService 测试——遗留源与已同步FHIR Patient的比对。
/// </summary>
[TestClass]
public sealed class DriftDetectionServiceTests
{
    // ===== Normal Scenarios / 正常场景 =====

    /// <summary>
    /// Compare with null target (never synced / deleted): not found, no fields, HasDrift true.
    /// </summary>
    [TestMethod]
    public void Compare_TargetNull_ReturnsNotFoundNoDrift()
    {
        var source = CreateSource();

        var result = DriftDetectionService.Compare(source, target: null);

        result.FoundInTarget.Should().BeFalse();
        result.DriftedFields.Should().BeEmpty();
        result.HasDrift.Should().BeTrue();
    }

    /// <summary>
    /// Compare with every field matching: in sync, no drifted fields.
    /// </summary>
    [TestMethod]
    public void Compare_AllFieldsMatch_NoDrift()
    {
        var source = CreateSource();
        var target = CreateTarget(given: "John", family: "Doe", gender: AdministrativeGender.Male, birthDate: "1990-01-15", phone: "+1-555-0123");

        var result = DriftDetectionService.Compare(source, target);

        result.FoundInTarget.Should().BeTrue();
        result.DriftedFields.Should().BeEmpty();
        result.HasDrift.Should().BeFalse();
    }

    /// <summary>
    /// Compare with only the phone differing: exactly one drift field for Phone, nothing else flagged.
    /// </summary>
    [TestMethod]
    public void Compare_SinglePhoneDrift_DetectsOnlyPhone()
    {
        var source = CreateSource(phone: "+1-555-9999");
        var target = CreateTarget(phone: "+1-555-0123");

        var result = DriftDetectionService.Compare(source, target);

        result.DriftedFields.Should().HaveCount(1);
        result.DriftedFields.Single().FieldName.Should().Be("Phone");
    }

    /// <summary>
    /// Compare with name, gender, and birthdate all differing: all four fields flagged, nothing else.
    /// [EN] Note: phone also differs (source vs target), so 4 fields drift total.

    /// </summary>
    [TestMethod]
    public void Compare_MultipleFieldsDrift_DetectsAll()
    {
        var source = CreateSource();
        var target = CreateTarget(given: "Jane", family: "Roe", gender: AdministrativeGender.Female, birthDate: "1985-06-20");

        var result = DriftDetectionService.Compare(source, target);

        result.DriftedFields.Should().HaveCount(4);
        result.DriftedFields.Select(f => f.FieldName).Should().BeEquivalentTo("FirstName", "LastName", "Gender", "BirthDate");
    }

    /// <summary>
    /// Compare with stripTestNameMarkers=true: "-Test"/" [TEST]" suffixes on the target are ignored, no name drift.
    /// </summary>
    [TestMethod]
    public void Compare_StripTestNameMarkers_True_NoFalsePositive()
    {
        var source = CreateSource();
        var target = CreateTarget(given: "John-Test", family: "Doe [TEST]");

        var result = DriftDetectionService.Compare(source, target, stripTestNameMarkers: true);

        result.DriftedFields.Should().NotContain(f => (f.FieldName == "FirstName" || f.FieldName == "LastName"));
    }

    /// <summary>
    /// Compare with stripTestNameMarkers=false (default): markers are real data differences and ARE flagged as drift.
    /// </summary>
    [TestMethod]
    public void Compare_StripTestNameMarkers_False_FlagsMarkersAsDrift()
    {
        var source = CreateSource();
        var target = CreateTarget(given: "John-Test", family: "Doe [TEST]");

        var result = DriftDetectionService.Compare(source, target);

        result.DriftedFields.Should().Contain(f => f.FieldName == "FirstName");
        result.DriftedFields.Should().Contain(f => f.FieldName == "LastName");
    }

    // ===== Expected Exception Scenarios / 期待异常场景 =====

    /// <summary>
    /// Compare with null source: throws ArgumentNullException with param name "source".
    /// </summary>
    [TestMethod]
    public void Compare_NullSource_ThrowsArgumentNullException()
    {
        Action act = () => DriftDetectionService.Compare(null!, CreateTarget());
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("source");
    }

    // ===== Error Scenarios / 错误场景 =====

    /// <summary>
    /// Gender regression guard: "Female" source vs Male target must flag gender drift
    /// (previously "female" contained the substring "male" and falsely drifted to Male).
    /// </summary>
    [TestMethod]
    public void Compare_FemaleSourceVsMaleTarget_FlagsGenderDrift()
    {
        var source = CreateSource(gender: "Female");
        var target = CreateTarget(gender: AdministrativeGender.Male);

        var result = DriftDetectionService.Compare(source, target);

        result.DriftedFields.Should().Contain(f => f.FieldName == "Gender");
    }

    /// <summary>
    /// Gender regression guard: "Female" source vs Female target must NOT flag gender drift.
    /// </summary>
    [TestMethod]
    public void Compare_FemaleSourceVsFemaleTarget_NoGenderDrift()
    {
        var source = CreateSource(gender: "Female");
        var target = CreateTarget(gender: AdministrativeGender.Female);

        var result = DriftDetectionService.Compare(source, target);

        result.DriftedFields.Should().NotContain(f => f.FieldName == "Gender");
    }

    /// <summary>
    /// Summarize mixed results: one missing, one field-drift, one in sync — counts match.
    /// </summary>
    [TestMethod]
    public void Summarize_MixedResults_CountsCorrectly()
    {
        var results = new List<PatientDriftResult>
        {
            new("M1", FoundInTarget: false, DriftedFields: Array.Empty<DriftField>()),
            new("D1", FoundInTarget: true, DriftedFields: new[] { new DriftField("Phone", "a", "b") }),
            new("S1", FoundInTarget: true, DriftedFields: Array.Empty<DriftField>())
        };

        var report = DriftReport.Summarize(results);

        report.RecordsChecked.Should().Be(3);
        report.RecordsInSync.Should().Be(1);
        report.RecordsWithFieldDrift.Should().Be(1);
        report.RecordsMissingInTarget.Should().Be(1);
    }

    /// <summary>
    /// Summarize with null results: throws ArgumentNullException with param name "results".
    /// </summary>
    [TestMethod]
    public void Summarize_NullResults_ThrowsArgumentNullException()
    {
        Action act = () => DriftReport.Summarize(null!);
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("results");
    }

    // ===== Test Helpers / 测试辅助 =====

    /// <summary>
    /// Builds a baseline legacy source record for comparison tests.
    /// </summary>
    private static LegacyPatientRecord CreateSource(
        string gender = "Male",
        string birthDate = "1990-01-15",
        string? phone = "+1-555-0123")
    {
        return new LegacyPatientRecord
        {
            Id = "T-001",
            FirstName = "John",
            LastName = "Doe",
            Gender = gender,
            BirthDate = birthDate,
            Phone = phone
        };
    }

    /// <summary>
    /// Builds a FHIR Patient by hand (independent of FhirPatientMapper) with the given field values.
    /// [EN] Note: patient.BirthDate is cast to a raw Fhir.String so its ToString() renders exactly the
    ///     input string (FhirDateTime would append a time component), matching what module 04's
    ///     generated mapper assigns at runtime.
    /// [CN] 注意：将BirthDate直接赋为FHIR原始字符串，使其ToString()精确等于输入字符串
    ///     （FhirDateTime会附加时间部分），与模块04生成的映射器在运行时的赋值方式一致。
    /// </summary>
    private static Patient CreateTarget(
        string given = "John",
        string family = "Doe",
        AdministrativeGender gender = AdministrativeGender.Male,
        string birthDate = "1990-01-15",
        string? phone = "+1-555-0123")
    {
        var patient = new Patient
        {
            Name = new List<HumanName>
            {
                new HumanName { Given = new[] { given }, Family = family }
            },
            Gender = gender,
            BirthDate = birthDate
        };

        if (!string.IsNullOrWhiteSpace(phone))
        {
            patient.Telecom = new List<ContactPoint>
            {
                new ContactPoint
                {
                    System = ContactPoint.ContactPointSystem.Phone,
                    Value = phone
                }
            };
        }

        return patient;
    }
}
