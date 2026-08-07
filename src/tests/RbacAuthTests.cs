using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Shared_Library;

namespace HealthData.Interop.Tests.RbacAuthTests;

/// <summary>
/// [EN] Unit tests for RbacAuth.CanAccessFullPHI().
/// [CN] RbacAuth.CanAccessFullPHI() 方法的单元测试。
/// Verifies that only the Physician role can access full PHI (HIPAA Minimum Necessary principle).
/// 验证只有医师角色可以访问完整PHI（HIPAA最小必要原则）。
/// </summary>
[TestClass]
public sealed class CanAccessFullPHITests
{
    private readonly RbacAuth _auth = new();

    /// <summary>
    /// [EN] Verify Physician role CAN access full PHI.
    /// [CN] 验证医师角色可以访问完整PHI。
    /// </summary>
    [TestMethod]
    public void CanAccessFullPHI_Physician_ShouldReturnTrue()
    {
        // Arrange
        FhirUserRole role = FhirUserRole.Physician;

        // Act
        bool result = _auth.CanAccessFullPHI(role);

        // Assert
        result.Should().BeTrue("Physician is the only role allowed full PHI access under HIPAA Minimum Necessary rule");
    }

    /// <summary>
    /// [EN] Verify all non-Physician roles CANNOT access full PHI.
    /// [CN] 验证所有非医师角色不能访问完整PHI。
    /// </summary>
    [TestMethod]
    public void CanAccessFullPHI_AllNonPhysicianRoles_ShouldReturnFalse()
    {
        // Arrange
        var nonPhysicianRoles = new[]
        {
            FhirUserRole.SysAdmin,
            FhirUserRole.Nurse,
            FhirUserRole.FrontDesk,
            FhirUserRole.Biller,
            FhirUserRole.Insurance,
            FhirUserRole.Patient,
            FhirUserRole.Auditor
        };

        // Act & Assert
        foreach (var role in nonPhysicianRoles)
        {
            bool result = _auth.CanAccessFullPHI(role);
            result.Should().BeFalse($"Role '{role}' should NOT have full PHI access under HIPAA");
        }
    }

    /// <summary>
    /// [EN] Parameterized test: iterate all 8 roles and verify exact expected results.
    /// [CN] 参数化测试：遍历全部8个角色并验证精确的预期结果。
    /// </summary>
    [TestMethod, DataRow("SysAdmin", false)]
    [DataRow("Physician", true)]
    [DataRow("Nurse", false)]
    [DataRow("FrontDesk", false)]
    [DataRow("Biller", false)]
    [DataRow("Insurance", false)]
    [DataRow("Patient", false)]
    [DataRow("Auditor", false)]
    public void CanAccessFullPHI_AllRolesParameterized_ShouldMatchExpected(string roleName, bool expected)
    {
        // Arrange
        FhirUserRole role = (FhirUserRole)Enum.Parse(typeof(FhirUserRole), roleName);

        // Act
        bool result = _auth.CanAccessFullPHI(role);

        // Assert
        result.Should().Be(expected, $"Role '{roleName}' expected {(expected ? "true" : "false")} for CanAccessFullPHI");
    }
}

/// <summary>
/// [EN] Unit tests for RbacAuth.CanModifyClinicalData().
/// [CN] RbacAuth.CanModifyClinicalData() 方法的单元测试。
/// Verifies that only Physician and Nurse roles can modify clinical data.
/// 验证只有医师和护士角色可以修改临床数据。
/// </summary>
[TestClass]
public sealed class CanModifyClinicalDataTests
{
    private readonly RbacAuth _auth = new();

    /// <summary>
    /// [EN] Verify Physician and Nurse CAN modify clinical data.
    /// [CN] 验证医师和护士可以修改临床数据。
    /// </summary>
    [TestMethod]
    public void CanModifyClinicalData_PhysicianAndNurse_ShouldReturnTrue()
    {
        _auth.CanModifyClinicalData(FhirUserRole.Physician).Should().BeTrue();
        _auth.CanModifyClinicalData(FhirUserRole.Nurse).Should().BeTrue();
    }

    /// <summary>
    /// [EN] Verify non-clinical roles CANNOT modify clinical data.
    /// [CN] 验证非临床角色不能修改临床数据。
    /// </summary>
    [TestMethod]
    public void CanModifyClinicalData_NonClinicalRoles_ShouldReturnFalse()
    {
        var nonClinicalRoles = new[]
        {
            FhirUserRole.SysAdmin, FhirUserRole.FrontDesk,
            FhirUserRole.Biller, FhirUserRole.Insurance,
            FhirUserRole.Patient, FhirUserRole.Auditor
        };

        foreach (var role in nonClinicalRoles)
        {
            _auth.CanModifyClinicalData(role).Should().BeFalse($"Role '{role}' should NOT modify clinical data");
        }
    }

    /// <summary>
    /// [EN] Parameterized test: iterate all 8 roles for CanModifyClinicalData.
    /// [CN] 参数化测试：遍历全部8个角色验证CanModifyClinicalData。
    /// </summary>
    [TestMethod, DataRow("SysAdmin", false)]
    [DataRow("Physician", true)]
    [DataRow("Nurse", true)]
    [DataRow("FrontDesk", false)]
    [DataRow("Biller", false)]
    [DataRow("Insurance", false)]
    [DataRow("Patient", false)]
    [DataRow("Auditor", false)]
    public void CanModifyClinicalData_AllRolesParameterized_ShouldMatchExpected(string roleName, bool expected)
    {
        FhirUserRole role = (FhirUserRole)Enum.Parse(typeof(FhirUserRole), roleName);
        bool result = _auth.CanModifyClinicalData(role);
        result.Should().Be(expected, $"Role '{roleName}' expected {(expected ? "true" : "false")} for CanModifyClinicalData");
    }
}

/// <summary>
/// [EN] Unit tests for RbacAuth.CanViewAuditLogs().
/// [CN] RbacAuth.CanViewAuditLogs() 方法的单元测试。
/// Verifies that only SysAdmin and Auditor roles can view audit logs.
/// 验证只有系统管理员和审计员角色可以查看审计日志。
/// </summary>
[TestClass]
public sealed class CanViewAuditLogsTests
{
    private readonly RbacAuth _auth = new();

    /// <summary>
    /// [EN] Verify SysAdmin and Auditor CAN view audit logs.
    /// [CN] 验证系统管理员和审计员可以查看审计日志。
    /// </summary>
    [TestMethod]
    public void CanViewAuditLogs_SysAdminAndAuditor_ShouldReturnTrue()
    {
        _auth.CanViewAuditLogs(FhirUserRole.SysAdmin).Should().BeTrue();
        _auth.CanViewAuditLogs(FhirUserRole.Auditor).Should().BeTrue();
    }

    /// <summary>
    /// [EN] Verify non-audit roles CANNOT view audit logs.
    /// [CN] 验证非审计角色不能查看审计日志。
    /// </summary>
    [TestMethod]
    public void CanViewAuditLogs_NonAuditRoles_ShouldReturnFalse()
    {
        var nonAuditRoles = new[]
        {
            FhirUserRole.Physician, FhirUserRole.Nurse,
            FhirUserRole.FrontDesk, FhirUserRole.Biller,
            FhirUserRole.Insurance, FhirUserRole.Patient
        };

        foreach (var role in nonAuditRoles)
        {
            _auth.CanViewAuditLogs(role).Should().BeFalse($"Role '{role}' should NOT view audit logs");
        }
    }

    /// <summary>
    /// [EN] Parameterized test: iterate all 8 roles for CanViewAuditLogs.
    /// [CN] 参数化测试：遍历全部8个角色验证CanViewAuditLogs。
    /// </summary>
    [TestMethod, DataRow("SysAdmin", true)]
    [DataRow("Physician", false)]
    [DataRow("Nurse", false)]
    [DataRow("FrontDesk", false)]
    [DataRow("Biller", false)]
    [DataRow("Insurance", false)]
    [DataRow("Patient", false)]
    [DataRow("Auditor", true)]
    public void CanViewAuditLogs_AllRolesParameterized_ShouldMatchExpected(string roleName, bool expected)
    {
        FhirUserRole role = (FhirUserRole)Enum.Parse(typeof(FhirUserRole), roleName);
        bool result = _auth.CanViewAuditLogs(role);
        result.Should().Be(expected, $"Role '{roleName}' expected {(expected ? "true" : "false")} for CanViewAuditLogs");
    }
}

/// <summary>
/// [EN] Unit tests for RbacAuth.CanAccessPatientDemographics().
/// [CN] RbacAuth.CanAccessPatientDemographics() 方法的单元测试。
/// Verifies that Physician, Nurse, FrontDesk, Biller, Insurance, and Patient can access demographics.
/// 验证医师、护士、前台、计费员、保险和患者可以访问人口统计信息。
/// </summary>
[TestClass]
public sealed class CanAccessPatientDemographicsTests
{
    private readonly RbacAuth _auth = new();

    /// <summary>
    /// [EN] Verify all allowed roles CAN access patient demographics.
    /// [CN] 验证所有允许的角色可以访问患者人口统计信息。
    /// </summary>
    [TestMethod]
    public void CanAccessPatientDemographics_AllowedRoles_ShouldReturnTrue()
    {
        var allowedRoles = new[]
        {
            FhirUserRole.Physician, FhirUserRole.Nurse,
            FhirUserRole.FrontDesk, FhirUserRole.Biller,
            FhirUserRole.Insurance, FhirUserRole.Patient
        };

        foreach (var role in allowedRoles)
        {
            _auth.CanAccessPatientDemographics(role).Should().BeTrue($"Role '{role}' should access patient demographics");
        }
    }

    /// <summary>
    /// [EN] Verify SysAdmin and Auditor CANNOT access patient demographics.
    /// [CN] 验证系统管理员和审计员不能访问患者人口统计信息。
    /// </summary>
    [TestMethod]
    public void CanAccessPatientDemographics_DeniedRoles_ShouldReturnFalse()
    {
        _auth.CanAccessPatientDemographics(FhirUserRole.SysAdmin).Should().BeFalse();
        _auth.CanAccessPatientDemographics(FhirUserRole.Auditor).Should().BeFalse();
    }

    /// <summary>
    /// [EN] Parameterized test: iterate all 8 roles for CanAccessPatientDemographics.
    /// [CN] 参数化测试：遍历全部8个角色验证CanAccessPatientDemographics。
    /// </summary>
    [TestMethod, DataRow("SysAdmin", false)]
    [DataRow("Physician", true)]
    [DataRow("Nurse", true)]
    [DataRow("FrontDesk", true)]
    [DataRow("Biller", true)]
    [DataRow("Insurance", true)]
    [DataRow("Patient", true)]
    [DataRow("Auditor", false)]
    public void CanAccessPatientDemographics_AllRolesParameterized_ShouldMatchExpected(string roleName, bool expected)
    {
        FhirUserRole role = (FhirUserRole)Enum.Parse(typeof(FhirUserRole), roleName);
        bool result = _auth.CanAccessPatientDemographics(role);
        result.Should().Be(expected, $"Role '{roleName}' expected {(expected ? "true" : "false")} for CanAccessPatientDemographics");
    }
}

/// <summary>
/// [EN] Unit tests for RbacAuth.CanWriteAnyResource().
/// [CN] RbacAuth.CanWriteAnyResource() 方法的单元测试。
/// Verifies that only Physician, Nurse, and Patient can write/modify any resource.
/// 验证只有医师、护士和患者可以写入/修改任意资源。
/// </summary>
[TestClass]
public sealed class CanWriteAnyResourceTests
{
    private readonly RbacAuth _auth = new();

    /// <summary>
    /// [EN] Verify Physician, Nurse, and Patient CAN write any resource.
    /// [CN] 验证医师、护士和患者可以写入任意资源。
    /// </summary>
    [TestMethod]
    public void CanWriteAnyResource_WriteRoles_ShouldReturnTrue()
    {
        _auth.CanWriteAnyResource(FhirUserRole.Physician).Should().BeTrue();
        _auth.CanWriteAnyResource(FhirUserRole.Nurse).Should().BeTrue();
        _auth.CanWriteAnyResource(FhirUserRole.Patient).Should().BeTrue();
    }

    /// <summary>
    /// [EN] Verify read-only roles CANNOT write any resource.
    /// [CN] 验证只读角色不能写入任意资源。
    /// </summary>
    [TestMethod]
    public void CanWriteAnyResource_ReadOnlyRoles_ShouldReturnFalse()
    {
        var readOnlyRoles = new[]
        {
            FhirUserRole.SysAdmin, FhirUserRole.FrontDesk,
            FhirUserRole.Biller, FhirUserRole.Insurance,
            FhirUserRole.Auditor
        };

        foreach (var role in readOnlyRoles)
        {
            _auth.CanWriteAnyResource(role).Should().BeFalse($"Role '{role}' should NOT write any resource");
        }
    }

    /// <summary>
    /// [EN] Parameterized test: iterate all 8 roles for CanWriteAnyResource.
    /// [CN] 参数化测试：遍历全部8个角色验证CanWriteAnyResource。
    /// </summary>
    [TestMethod, DataRow("SysAdmin", false)]
    [DataRow("Physician", true)]
    [DataRow("Nurse", true)]
    [DataRow("FrontDesk", false)]
    [DataRow("Biller", false)]
    [DataRow("Insurance", false)]
    [DataRow("Patient", true)]
    [DataRow("Auditor", false)]
    public void CanWriteAnyResource_AllRolesParameterized_ShouldMatchExpected(string roleName, bool expected)
    {
        FhirUserRole role = (FhirUserRole)Enum.Parse(typeof(FhirUserRole), roleName);
        bool result = _auth.CanWriteAnyResource(role);
        result.Should().Be(expected, $"Role '{roleName}' expected {(expected ? "true" : "false")} for CanWriteAnyResource");
    }
}

/// <summary>
/// [EN] Integration-style permission matrix tests: verify each role's complete permission profile.
/// [CN] 权限矩阵集成测试：验证每个角色的完整权限概要。
/// This ensures consistency across all 5 permission methods for each of the 8 roles.
/// 确保全部5个权限方法在8个角色上的一致性。
/// </summary>
[TestClass]
public sealed class RbacAuthPermissionMatrixTests
{
    private readonly RbacAuth _auth = new();

    /// <summary>
    /// [EN] Physician: Full PHI + Clinical + Demographics + Write, but NO audit logs.
    /// [CN] 医师：完整PHI + 临床 + 人口统计 + 写入，但无审计日志。
    /// </summary>
    [TestMethod]
    public void PermissionMatrix_Physician_ShouldHaveCorrectProfile()
    {
        var role = FhirUserRole.Physician;
        _auth.CanAccessFullPHI(role).Should().BeTrue();
        _auth.CanModifyClinicalData(role).Should().BeTrue();
        _auth.CanViewAuditLogs(role).Should().BeFalse();
        _auth.CanAccessPatientDemographics(role).Should().BeTrue();
        _auth.CanWriteAnyResource(role).Should().BeTrue();
    }

    /// <summary>
    /// [EN] Nurse: Clinical + Demographics + Write, but NO full PHI and NO audit logs.
    /// [CN] 护士：临床 + 人口统计 + 写入，但无完整PHI和无审计日志。
    /// </summary>
    [TestMethod]
    public void PermissionMatrix_Nurse_ShouldHaveCorrectProfile()
    {
        var role = FhirUserRole.Nurse;
        _auth.CanAccessFullPHI(role).Should().BeFalse("Nurse should NOT have full PHI under Minimum Necessary");
        _auth.CanModifyClinicalData(role).Should().BeTrue();
        _auth.CanViewAuditLogs(role).Should().BeFalse();
        _auth.CanAccessPatientDemographics(role).Should().BeTrue();
        _auth.CanWriteAnyResource(role).Should().BeTrue();
    }

    /// <summary>
    /// [EN] SysAdmin: Only audit logs, no other PHI access.
    /// [CN] 系统管理员：仅审计日志，无其他PHI访问。
    /// </summary>
    [TestMethod]
    public void PermissionMatrix_SysAdmin_ShouldHaveCorrectProfile()
    {
        var role = FhirUserRole.SysAdmin;
        _auth.CanAccessFullPHI(role).Should().BeFalse();
        _auth.CanModifyClinicalData(role).Should().BeFalse();
        _auth.CanViewAuditLogs(role).Should().BeTrue();
        _auth.CanAccessPatientDemographics(role).Should().BeFalse();
        _auth.CanWriteAnyResource(role).Should().BeFalse();
    }

    /// <summary>
    /// [EN] Auditor: Only audit logs, no other PHI access.
    /// [CN] 审计员：仅审计日志，无其他PHI访问。
    /// </summary>
    [TestMethod]
    public void PermissionMatrix_Auditor_ShouldHaveCorrectProfile()
    {
        var role = FhirUserRole.Auditor;
        _auth.CanAccessFullPHI(role).Should().BeFalse();
        _auth.CanModifyClinicalData(role).Should().BeFalse();
        _auth.CanViewAuditLogs(role).Should().BeTrue();
        _auth.CanAccessPatientDemographics(role).Should().BeFalse();
        _auth.CanWriteAnyResource(role).Should().BeFalse();
    }

    /// <summary>
    /// [EN] FrontDesk: Only demographics access, read-only.
    /// [CN] 前台：仅人口统计访问，只读。
    /// </summary>
    [TestMethod]
    public void PermissionMatrix_FrontDesk_ShouldHaveCorrectProfile()
    {
        var role = FhirUserRole.FrontDesk;
        _auth.CanAccessFullPHI(role).Should().BeFalse();
        _auth.CanModifyClinicalData(role).Should().BeFalse();
        _auth.CanViewAuditLogs(role).Should().BeFalse();
        _auth.CanAccessPatientDemographics(role).Should().BeTrue();
        _auth.CanWriteAnyResource(role).Should().BeFalse();
    }

    /// <summary>
    /// [EN] Biller: Only demographics access, read-only.
    /// [CN] 计费员：仅人口统计访问，只读。
    /// </summary>
    [TestMethod]
    public void PermissionMatrix_Biller_ShouldHaveCorrectProfile()
    {
        var role = FhirUserRole.Biller;
        _auth.CanAccessFullPHI(role).Should().BeFalse();
        _auth.CanModifyClinicalData(role).Should().BeFalse();
        _auth.CanViewAuditLogs(role).Should().BeFalse();
        _auth.CanAccessPatientDemographics(role).Should().BeTrue();
        _auth.CanWriteAnyResource(role).Should().BeFalse();
    }

    /// <summary>
    /// [EN] Insurance: Only demographics access, read-only.
    /// [CN] 保险角色：仅人口统计访问，只读。
    /// </summary>
    [TestMethod]
    public void PermissionMatrix_Insurance_ShouldHaveCorrectProfile()
    {
        var role = FhirUserRole.Insurance;
        _auth.CanAccessFullPHI(role).Should().BeFalse();
        _auth.CanModifyClinicalData(role).Should().BeFalse();
        _auth.CanViewAuditLogs(role).Should().BeFalse();
        _auth.CanAccessPatientDemographics(role).Should().BeTrue();
        _auth.CanWriteAnyResource(role).Should().BeFalse();
    }

    /// <summary>
    /// [EN] Patient: Demographics + Write, but NO full PHI and NO clinical/audit access.
    /// [CN] 患者：人口统计 + 写入，但无完整PHI和无临床/审计访问。
    /// </summary>
    [TestMethod]
    public void PermissionMatrix_Patient_ShouldHaveCorrectProfile()
    {
        var role = FhirUserRole.Patient;
        _auth.CanAccessFullPHI(role).Should().BeFalse();
        _auth.CanModifyClinicalData(role).Should().BeFalse();
        _auth.CanViewAuditLogs(role).Should().BeFalse();
        _auth.CanAccessPatientDemographics(role).Should().BeTrue();
        _auth.CanWriteAnyResource(role).Should().BeTrue();
    }
}

/// <summary>
/// [EN] Tests verifying that the 8 FhirUserRole enum values match CodeStandard.md requirement for centralized enums.
/// [CN] 验证FhirUserRole枚举的8个值符合CodeStandard.md关于集中管理枚举的要求。
/// </summary>
[TestClass]
public sealed class FhirUserRoleEnumTests
{
    /// <summary>
    /// [EN] Verify enum has exactly 8 members.
    /// [CN] 验证枚举恰好有8个成员。
    /// </summary>
    [TestMethod]
    public void FhirUserRole_EnumMemberCount_ShouldBeEight()
    {
        var names = Enum.GetNames(typeof(FhirUserRole));
        names.Should().HaveCount(8, "FhirUserRole should have exactly 8 official roles");
    }

    /// <summary>
    /// [EN] Verify all expected role names exist in the enum.
    /// [CN] 验证所有预期角色名称存在于枚举中。
    /// </summary>
    [TestMethod]
    public void FhirUserRole_AllExpectedRoles_ShouldExist()
    {
        var expectedNames = new[]
        {
            "SysAdmin", "Physician", "Nurse", "FrontDesk",
            "Biller", "Insurance", "Patient", "Auditor"
        };

        var actualNames = Enum.GetNames(typeof(FhirUserRole));

        foreach (var name in expectedNames)
        {
            actualNames.Should().Contain(name, $"Role '{name}' should exist in FhirUserRole enum");
        }
    }

    /// <summary>
    /// [EN] Verify enum values start at 0 and are contiguous.
    /// [CN] 验证枚举值从0开始且连续。
    /// </summary>
    [TestMethod]
    public void FhirUserRole_EnumValues_ShouldBeContiguous()
    {
        var values = Enum.GetValues(typeof(FhirUserRole)).Cast<int>().ToArray();

        for (int i = 0; i < values.Length; i++)
        {
            values[i].Should().Be(i, $"Enum value at index {i} should be {i}");
        }
    }
}
