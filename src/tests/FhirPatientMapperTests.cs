using HealthDataInteropSharedLibrary.Shared;
using FluentAssertions;
using Hl7.Fhir.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HealthDataInteropSharedLibrary.SmartOnFHIR;

namespace HealthData.Interop.Tests.MapperTests;

/// <summary>
/// [EN] FhirPatientMapper mapping tests — LegacyPatientRecord and RawPatientData to FHIR Patient.
/// Covers: normal mapping, PostProcess configuration (US Core, test data tag, identifier system, addIdentifier switch),
/// null/empty parameter validation, and static helper methods.
/// [CN] FhirPatientMapper 映射测试——LegacyPatientRecord和RawPatientData到FHIR Patient。
/// </summary>
[TestClass]
public sealed class FhirPatientMapperTests
{
    // ===== Normal Scenarios / 正常场景 =====

    /// <summary>
    /// Map LegacyPatientRecord: verify all fields mapped correctly (name, gender, birthDate, phone).
    /// </summary>
    [TestMethod]
    public void MapLegacy_AllFieldsMapped()
    {
        var mapper = new FhirPatientMapper();
        var source = new LegacyPatientRecord
        {
            Id = "P001", FirstName = "John", LastName = "Doe", Gender = "Male", BirthDate = "1990-01-15", Phone = "+1-555-0123"
        };

        var patient = mapper.MapLegacy(source);
        patient.Name.Should().HaveCount(1);
        patient.Name[0].Given.First().Should().Be("John");
        patient.Name[0].Family.Should().Be("Doe");
        patient.Gender.Should().Be(AdministrativeGender.Male);
        patient.BirthDate.ToString().Should().StartWith("1990-01-15");
        patient.Telecom.Should().HaveCount(1);
        patient.Telecom[0].Value.Should().Be("+1-555-0123");
    }

    /// <summary>
    /// Map LegacyPatientRecord: verify Identifier and Meta tags injected by PostProcess.
    /// </summary>
    [TestMethod]
    public void MapLegacy_IdentifierAndMetaInjected()
    {
        var mapper = new FhirPatientMapper();
        var source = new LegacyPatientRecord { Id = "P001", FirstName = "A", LastName = "B", Gender = "F", BirthDate = "2000-01-01" };

        var patient = mapper.MapLegacy(source);
        patient.Identifier.Should().HaveCount(1);
        patient.Identifier.First().System.Should().Be(FhirPatientMapper.DefaultIdSystem);
        patient.Identifier.First().Value.Should().Be("P001");
        patient.Meta.Should().NotBeNull();
        patient.Meta.Tag.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// Map with US Core profile: verify Meta.Profile contains US Core URL.
    /// </summary>
    [TestMethod]
    public void MapLegacy_WithUsCoreProfile_ContainsProfileUrl()
    {
        var mapper = new FhirPatientMapper(usCoreProfile: true, addTestDataTag: false);
        var source = new LegacyPatientRecord { Id = "P002", FirstName = "C", LastName = "D", Gender = "M", BirthDate = "1980-05-20" };

        var patient = mapper.MapLegacy(source);
        patient.Meta.Profile.Should().Contain(FhirPatientMapper.UsCorePatientProfile);
        patient.Meta.Tag.Should().BeNullOrEmpty("No test data tag when addTestDataTag=false");
    }

    /// <summary>
    /// Map RawPatientData: verify fields mapped with generated ID from name.
    /// </summary>
    [TestMethod]
    public void MapRaw_GeneratesIdFromName()
    {
        var mapper = new FhirPatientMapper();
        var source = new RawPatientData { FirstName = "Jane", LastName = "Smith", Gender = "Female", BirthDate = "1985-06-20" };

        var patient = mapper.MapRaw(source);
        patient.Identifier.First().Value.Should().Be("Jane_Smith", "ID should be generated from name");
        patient.Gender.Should().Be(AdministrativeGender.Female);
    }

    /// <summary>
    /// Map RawPatientData (module 05): verify Active is set to true (pre-refactor behavior).
    /// </summary>
    [TestMethod]
    public void MapRaw_SetsActiveTrue()
    {
        var mapper = new FhirPatientMapper();
        var source = new RawPatientData { FirstName = "Jane", LastName = "Smith", Gender = "Female", BirthDate = "1985-06-20" };

        var patient = mapper.MapRaw(source);
        patient.Active.Should().Be(true, "Module 05 marks imported patients Active");
    }

    /// <summary>
    /// Map RawPatientData with addIdentifier=false (module 05 pre-refactor parity):
    /// no business Identifier is written, female gender stays Female (data-integrity fix), US Core profile applied.
    /// </summary>
    [TestMethod]
    public void MapRaw_NoIdentifier_WhenAddIdentifierDisabled()
    {
        var mapper = new FhirPatientMapper(usCoreProfile: true, addTestDataTag: false, addIdentifier: false);
        var source = new RawPatientData { FirstName = "Jane", LastName = "Smith", Gender = "Female", BirthDate = "1985-06-20" };

        var patient = mapper.MapRaw(source);

        patient.Identifier.Should().BeNullOrEmpty("Module 05 pre-refactor wrote no business Identifier");
        patient.Gender.Should().Be(AdministrativeGender.Female, "Female must not be mapped to Male");
        patient.Active.Should().BeTrue("Module 05 marks imported patients Active");
        patient.Meta.Should().NotBeNull();
        patient.Meta.Profile.Should().Contain(FhirPatientMapper.UsCorePatientProfile);
    }

    /// <summary>
    /// Map LegacyPatientRecord with testNameMarkers=true (module 04): verify "-Test"/"[TEST]" markers appended.
    /// </summary>
    [TestMethod]
    public void MapLegacy_WithTestNameMarkers_AppendsMarkers()
    {
        var mapper = new FhirPatientMapper(testNameMarkers: true);
        var source = new LegacyPatientRecord { Id = "P010", FirstName = "John", LastName = "Doe", Gender = "Male", BirthDate = "1990-01-01" };

        var patient = mapper.MapLegacy(source);
        patient.Name[0].Given.First().Should().Be("John-Test");
        patient.Name[0].Family.Should().Be("Doe [TEST]");
    }

    /// <summary>
    /// Map LegacyPatientRecord default (module 04 off): verify names have NO test markers.
    /// </summary>
    [TestMethod]
    public void MapLegacy_WithoutTestNameMarkers_NoMarkers()
    {
        var mapper = new FhirPatientMapper();
        var source = new LegacyPatientRecord { Id = "P011", FirstName = "John", LastName = "Doe", Gender = "Male", BirthDate = "1990-01-01" };

        var patient = mapper.MapLegacy(source);
        patient.Name[0].Given.First().Should().Be("John");
        patient.Name[0].Family.Should().Be("Doe");
    }

    /// <summary>
    /// Map with null phone: verify Telecom is not set (null-safe).
    /// </summary>
    [TestMethod]
    public void MapLegacy_NullPhone_TelecomIsNull()
    {
        var mapper = new FhirPatientMapper();
        var source = new LegacyPatientRecord { Id = "P003", FirstName = "E", LastName = "F", Gender = "Male", BirthDate = "1975-12-01", Phone = null };

        var patient = mapper.MapLegacy(source);
        patient.Telecom.Should().BeNullOrEmpty("Null phone should result in no telecom entries");
    }

    /// <summary>
    /// Static helper BuildHumanName: verify name parts trimmed correctly.
    /// </summary>
    [TestMethod]
    public void BuildHumanName_TrimsWhitespace()
    {
        var name = FhirPatientMapper.BuildHumanName("  John  ", "  Doe  ");
        name.Given.First().Should().Be("John");
        name.Family.Should().Be("Doe");
    }

    /// <summary>
    /// Static helper BuildHumanName with testMarkers: trims then appends "-Test"/"[TEST]".
    /// </summary>
    [TestMethod]
    public void BuildHumanName_WithTestMarkers_AppendsSuffixes()
    {
        var name = FhirPatientMapper.BuildHumanName("  Jane  ", "  Roe  ", testMarkers: true);
        name.Given.First().Should().Be("Jane-Test");
        name.Family.Should().Be("Roe [TEST]");
    }

    /// <summary>
    /// Static helper BuildTelecom: null/empty/whitespace returns null.
    /// </summary>
    [TestMethod]
    public void BuildTelecom_NullOrEmpty_ReturnsNull()
    {
        FhirPatientMapper.BuildTelecom(null).Should().BeNull();
        FhirPatientMapper.BuildTelecom("").Should().BeNull();
        FhirPatientMapper.BuildTelecom("   ").Should().BeNull();
    }

    /// <summary>
    /// Static helper BuildTelecom: valid phone creates ContactPoint with trimmed value.
    /// </summary>
    [TestMethod]
    public void BuildTelecom_ValidPhone_ReturnsContactPoint()
    {
        var telecom = FhirPatientMapper.BuildTelecom("  +1-555-0123  ");
        telecom.Should().HaveCount(1);
        telecom[0].System.Should().Be(ContactPoint.ContactPointSystem.Phone);
        telecom[0].Value.Should().Be("+1-555-0123");
    }

    // ===== Expected Exception Scenarios / 期待异常场景 =====

    /// <summary>
    /// Map with null LegacyPatientRecord source: throws ArgumentNullException.
    /// </summary>
    [TestMethod]
    public void MapLegacy_NullSource_ThrowsArgumentNullException()
    {
        var mapper = new FhirPatientMapper();
        Action act = () => mapper.MapLegacy(null!);
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("source");
    }

    /// <summary>
    /// Map with null RawPatientData source: throws ArgumentNullException.
    /// </summary>
    [TestMethod]
    public void MapRaw_NullSource_ThrowsArgumentNullException()
    {
        var mapper = new FhirPatientMapper();
        Action act = () => mapper.MapRaw(null!);
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("source");
    }

    // ===== Error Scenarios / 错误场景 =====

    /// <summary>
    /// Map with unrecognized gender value: should map to Unknown (not throw).
    /// </summary>
    [TestMethod]
    public void MapLegacy_UnrecognizedGender_MapsToUnknown()
    {
        var mapper = new FhirPatientMapper();
        var source = new LegacyPatientRecord { Id = "P004", FirstName = "G", LastName = "H", Gender = "Non-binary", BirthDate = "2000-01-01" };

        var patient = mapper.MapLegacy(source);
        patient.Gender.Should().Be(AdministrativeGender.Unknown, "Unrecognized gender should map to Unknown");
    }
}
