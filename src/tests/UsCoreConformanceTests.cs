using Shared_Library.ResourceValidator;
using Shared_Library.Shared;
using FluentAssertions;
using Hl7.Fhir.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HealthData.Interop.Tests.USCoreTests;

/// <summary>
/// [EN] Unit tests for UsCoreProfiles static class.
/// Verifies the known US Core profile URIs are correctly defined.
/// [CN] UsCoreProfiles静态类的单元测试。验证已知的US Core配置档URI正确定义。
/// </summary>
[TestClass]
public sealed class UsCoreProfilesTests
{
    [TestMethod]
    public void Patient_ProfileUri_ShouldBeValidUsCoreUri()
    {
        UsCoreProfiles.Patient.Should().StartWith("http://hl7.org/fhir/us/core/StructureDefinition/");
        UsCoreProfiles.Patient.Should().Contain("us-core-patient");
    }

    [TestMethod]
    public void GetAllProfiles_ShouldReturnSevenProfiles()
    {
        var profiles = UsCoreProfiles.GetAllProfiles();
        profiles.Should().HaveCount(7);
    }

    [TestMethod]
    public void GetProfileUriForType_Patient_ShouldReturnPatientProfile()
    {
        var uri = UsCoreProfiles.GetProfileUriForType("Patient");
        uri.Should().Be(UsCoreProfiles.Patient);
    }

    [TestMethod]
    public void GetProfileUriForType_Unknown_ShouldReturnNull()
    {
        var uri = UsCoreProfiles.GetProfileUriForType("UnknownResource");
        uri.Should().BeNull();
    }

    [DataTestMethod]
    [DataRow("Patient")]
    [DataRow("Observation")]
    [DataRow("Encounter")]
    [DataRow("Condition")]
    [DataRow("MedicationRequest")]
    [DataRow("AllergyIntolerance")]
    public void GetProfileUriForType_KnownTypes_ShouldReturnNonEmpty(string type)
    {
        var uri = UsCoreProfiles.GetProfileUriForType(type);
        uri.Should().NotBeNull();
        uri.Should().StartWith("http://hl7.org/fhir/us/core/StructureDefinition/");
    }
}

/// <summary>
/// [EN] Unit tests for UsCoreConformanceChecker.
/// Tests profile conformance checking on Patient resources with and without US Core profiles.
/// [CN] UsCoreConformanceChecker的单元测试。测试带有和不带有US Core配置档的Patient资源的一致性检查。
/// </summary>
[TestClass]
public sealed class UsCoreConformanceCheckerTests
{
    [TestMethod]
    public void CheckPatientConformance_WithUsCoreProfile_ShouldBeConformant()
    {
        var patient = new Patient
        {
            Meta = new Meta
            {
                Profile = new List<string> { UsCoreProfiles.Patient }
            }
        };

        var result = UsCoreConformanceChecker.CheckPatientConformance(patient);

        result.IsUsCoreConformant.Should().BeTrue();
        result.DeclaredProfiles.Should().Contain(UsCoreProfiles.Patient);
    }

    [TestMethod]
    public void CheckPatientConformance_WithoutProfile_ShouldNotBeConformant()
    {
        var patient = new Patient();

        var result = UsCoreConformanceChecker.CheckPatientConformance(patient);

        result.IsUsCoreConformant.Should().BeFalse();
    }

    [TestMethod]
    public void CheckPatientConformance_WithWrongProfile_ShouldNotBeConformant()
    {
        var patient = new Patient
        {
            Meta = new Meta
            {
                Profile = new List<string> { "http://example.org/some-other-profile" }
            }
        };

        var result = UsCoreConformanceChecker.CheckPatientConformance(patient);

        result.IsUsCoreConformant.Should().BeFalse();
    }

    [TestMethod]
    public void GetDeclaredProfiles_WithNoMeta_ShouldReturnEmpty()
    {
        var patient = new Patient();
        var profiles = UsCoreConformanceChecker.GetDeclaredProfiles(patient);

        profiles.Should().BeEmpty();
    }

    [TestMethod]
    public void GetDeclaredProfiles_WithMultipleProfiles_ShouldReturnAll()
    {
        var patient = new Patient
        {
            Meta = new Meta
            {
                Profile = new List<string>
                {
                    UsCoreProfiles.Patient,
                    "http://example.org/extra-profile"
                }
            }
        };

        var profiles = UsCoreConformanceChecker.GetDeclaredProfiles(patient);

        profiles.Should().HaveCount(2);
        profiles.Should().Contain(UsCoreProfiles.Patient);
    }

    [TestMethod]
    public void EnsureUsCoreProfile_OnBlankPatient_ShouldAddProfile()
    {
        var patient = new Patient();

        var added = UsCoreConformanceChecker.EnsureUsCoreProfile(patient);

        added.Should().BeTrue("Profile should be added to blank patient");
        var result = UsCoreConformanceChecker.CheckPatientConformance(patient);
        result.IsUsCoreConformant.Should().BeTrue();
    }

    [TestMethod]
    public void EnsureUsCoreProfile_AlreadyHasProfile_ShouldNotAddAgain()
    {
        var patient = new Patient
        {
            Meta = new Meta
            {
                Profile = new List<string> { UsCoreProfiles.Patient }
            }
        };

        var added = UsCoreConformanceChecker.EnsureUsCoreProfile(patient);

        added.Should().BeFalse("Profile already exists, should not add again");
        patient.Meta.Profile.Should().HaveCount(1, "Should still have exactly one profile");
    }

    [TestMethod]
    public void EnsureUsCoreProfile_Idempotent_ShouldBeSafe()
    {
        var patient = new Patient();

        UsCoreConformanceChecker.EnsureUsCoreProfile(patient);
        UsCoreConformanceChecker.EnsureUsCoreProfile(patient);

        patient.Meta.Profile.Should().HaveCount(1, "Calling twice should not duplicate the profile");
    }

    [TestMethod]
    public void CheckResourceConformance_Patient_ShouldReturnResult()
    {
        var resource = new Patient();
        var result = UsCoreConformanceChecker.CheckResourceConformance(resource);

        result.Should().NotBeNull();
        result.ResourceType.Should().Be("Patient");
    }

    [TestMethod]
    public void CheckResourceConformance_UnknownType_ShouldReturnNull()
    {
        // Binary is not a US Core profile resource type
        var resource = new Binary();
        var result = UsCoreConformanceChecker.CheckResourceConformance(resource);

        result.Should().BeNull("Binary has no defined US Core profile");
    }

    [TestMethod]
    public void CheckPatientConformance_NullPatient_ShouldThrow()
    {
        Action act = () => UsCoreConformanceChecker.CheckPatientConformance(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void EnsureUsCoreProfile_NullPatient_ShouldThrow()
    {
        Action act = () => UsCoreConformanceChecker.EnsureUsCoreProfile(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
