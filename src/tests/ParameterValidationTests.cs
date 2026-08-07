using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared_Library;

namespace HealthData.Interop.Tests.ParameterValidationTests;

/// <summary>
/// [EN] Unit tests for AuditLog.Record() parameter validation.
/// [CN] AuditLog.Record() 参数验证的单元测试。
/// Verifies that all parameters are validated for null/empty before processing (CodeStandard.md requirement).
/// 验证所有参数在处理前经过null/空验证（CodeStandard.md要求）。
/// </summary>
[TestClass]
public sealed class AuditLogParameterValidationTests
{
    /// <summary>
    /// [EN] Expected scenario: Verify Record() succeeds with all valid parameters.
    /// [CN] 期望场景：验证 Record() 在所有参数有效时成功执行。
    /// </summary>
    [TestMethod]
    public void Record_AllValidParameters_ShouldSucceed()
    {
        Action act = () => AuditLog.Record("user001", "Physician", "192.168.1.1", "Patient", "P001", "READ");
        act.Should().NotThrow("Record should succeed with all valid parameters");
    }

    /// <summary>
    /// [EN] Error scenario: Verify Record() throws for null userId.
    /// [CN] 错误场景：验证 Record() 对null的userId抛出异常。
    /// </summary>
    [TestMethod]
    public void Record_NullUserId_ShouldThrow()
    {
        Action act = () => AuditLog.Record(null!, "Physician", "1.2.3.4", "Patient", "P1", "READ");
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("userId");
    }

    /// <summary>
    /// [EN] Error scenario: Verify Record() throws for empty userId.
    /// [CN] 错误场景：验证 Record() 对空的userId抛出异常。
    /// </summary>
    [TestMethod]
    public void Record_EmptyUserId_ShouldThrow()
    {
        Action act = () => AuditLog.Record("", "Physician", "1.2.3.4", "Patient", "P1", "READ");
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("userId");
    }

    /// <summary>
    /// [EN] Error scenario: Verify Record() throws for null role.
    /// [CN] 错误场景：验证 Record() 对null的role抛出异常。
    /// </summary>
    [TestMethod]
    public void Record_NullRole_ShouldThrow()
    {
        Action act = () => AuditLog.Record("user001", null!, "1.2.3.4", "Patient", "P1", "READ");
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("role");
    }

    /// <summary>
    /// [EN] Error scenario: Verify Record() throws for null ipAddress.
    /// [CN] 错误场景：验证 Record() 对null的ipAddress抛出异常。
    /// </summary>
    [TestMethod]
    public void Record_NullIpAddress_ShouldThrow()
    {
        Action act = () => AuditLog.Record("user001", "Physician", null!, "Patient", "P1", "READ");
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("ipAddress");
    }

    /// <summary>
    /// [EN] Error scenario: Verify Record() throws for null resourceType.
    /// [CN] 错误场景：验证 Record() 对null的resourceType抛出异常。
    /// </summary>
    [TestMethod]
    public void Record_NullResourceType_ShouldThrow()
    {
        Action act = () => AuditLog.Record("user001", "Physician", "1.2.3.4", null!, "P1", "READ");
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("resourceType");
    }

    /// <summary>
    /// [EN] Error scenario: Verify Record() throws for null resourceId.
    /// [CN] 错误场景：验证 Record() 对null的resourceId抛出异常。
    /// </summary>
    [TestMethod]
    public void Record_NullResourceId_ShouldThrow()
    {
        Action act = () => AuditLog.Record("user001", "Physician", "1.2.3.4", "Patient", null!, "READ");
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("resourceId");
    }

    /// <summary>
    /// [EN] Error scenario: Verify Record() throws for null action.
    /// [CN] 错误场景：验证 Record() 对null的action抛出异常。
    /// </summary>
    [TestMethod]
    public void Record_NullAction_ShouldThrow()
    {
        Action act = () => AuditLog.Record("user001", "Physician", "1.2.3.4", "Patient", "P1", null!);
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("action");
    }

    /// <summary>
    /// [EN] Boundary condition: Verify Record() accepts valid special-character inputs.
    /// [CN] 边界条件：验证 Record() 接受有效的特殊字符输入。
    /// </summary>
    [TestMethod]
    public void Record_SpecialCharacterInputs_ShouldSucceed()
    {
        Action act = () => AuditLog.Record(
            userId: "user_001@example.com",
            role: "Physician-Specialist",
            ipAddress: "::1",
            resourceType: "DiagnosticReport",
            resourceId: "DR-2024/001",
            action: "UPDATE"
        );

        act.Should().NotThrow("Record should accept valid inputs with special characters");
    }
}

/// <summary>
/// [EN] Unit tests for RbacAuth parameter validation (invalid enum values).
/// [CN] RbacAuth参数验证的单元测试（无效枚举值）。
/// Verifies that GuardValidRole rejects undefined enum values cast from integers.
/// 验证GuardValidRole拒绝从整数转换的未定义枚举值。
/// </summary>
[TestClass]
public sealed class RbacAuthParameterValidationTests
{
    private readonly RbacAuth _auth = new();

    /// <summary>
    /// [EN] Expected scenario: Verify CanAccessFullPHI succeeds for all defined roles.
    /// [CN] 期望场景：验证 CanAccessFullPHI 对所有已定义角色成功执行。
    /// </summary>
    [TestMethod]
    public void CanAccessFullPHI_AllDefinedRoles_ShouldNotThrow()
    {
        foreach (FhirUserRole role in Enum.GetValues(typeof(FhirUserRole)))
        {
            Action act = () => _auth.CanAccessFullPHI(role);
            act.Should().NotThrow("CanAccessFullPHI should not throw for defined role");
        }
    }

    /// <summary>
    /// [EN] Error scenario: Verify CanAccessFullPHI throws ArgumentException for undefined enum value.
    /// [CN] 错误场景：验证 CanAccessFullPHI 对未定义的枚举值抛出ArgumentException。
    /// </summary>
    [TestMethod]
    public void CanAccessFullPHI_UndefinedEnumValue_ShouldThrow()
    {
        FhirUserRole invalidRole = (FhirUserRole)999;
        Action act = () => _auth.CanAccessFullPHI(invalidRole);
        act.Should().Throw<ArgumentException>("Invalid role value should be rejected");
    }

    /// <summary>
    /// [EN] Error scenario: Verify CanModifyClinicalData rejects undefined enum values.
    /// [CN] 错误场景：验证 CanModifyClinicalData 拒绝未定义的枚举值。
    /// </summary>
    [TestMethod]
    public void CanModifyClinicalData_UndefinedEnumValue_ShouldThrow()
    {
        FhirUserRole invalidRole = (FhirUserRole)999;
        Action act = () => _auth.CanModifyClinicalData(invalidRole);
        act.Should().Throw<ArgumentException>("Invalid role should be rejected");
    }

    /// <summary>
    /// [EN] Error scenario: Verify CanViewAuditLogs rejects undefined enum values.
    /// [CN] 错误场景：验证 CanViewAuditLogs 拒绝未定义的枚举值。
    /// </summary>
    [TestMethod]
    public void CanViewAuditLogs_UndefinedEnumValue_ShouldThrow()
    {
        FhirUserRole invalidRole = (FhirUserRole)999;
        Action act = () => _auth.CanViewAuditLogs(invalidRole);
        act.Should().Throw<ArgumentException>("Invalid role should be rejected");
    }

    /// <summary>
    /// [EN] Error scenario: Verify CanAccessPatientDemographics rejects undefined enum values.
    /// [CN] 错误场景：验证 CanAccessPatientDemographics 拒绝未定义的枚举值。
    /// </summary>
    [TestMethod]
    public void CanAccessPatientDemographics_UndefinedEnumValue_ShouldThrow()
    {
        FhirUserRole invalidRole = (FhirUserRole)999;
        Action act = () => _auth.CanAccessPatientDemographics(invalidRole);
        act.Should().Throw<ArgumentException>("Invalid role should be rejected");
    }

    /// <summary>
    /// [EN] Error scenario: Verify CanWriteAnyResource rejects undefined enum values.
    /// [CN] 错误场景：验证 CanWriteAnyResource 拒绝未定义的枚举值。
    /// </summary>
    [TestMethod]
    public void CanWriteAnyResource_UndefinedEnumValue_ShouldThrow()
    {
        FhirUserRole invalidRole = (FhirUserRole)999;
        Action act = () => _auth.CanWriteAnyResource(invalidRole);
        act.Should().Throw<ArgumentException>("Invalid role should be rejected");
    }

    /// <summary>
    /// [EN] Boundary condition: Verify CanAccessFullPHI handles zero-valued default enum.
    /// [CN] 边界条件：验证 CanAccessFullPHI 处理零值默认枚举。
    /// </summary>
    [TestMethod]
    public void CanAccessFullPHI_DefaultEnumValue_ShouldNotThrow()
    {
        FhirUserRole defaultRole = default;
        Action act = () => _auth.CanAccessFullPHI(defaultRole);
        act.Should().NotThrow("Default enum value (SysAdmin/0) is valid and should not throw");
    }

    /// <summary>
    /// [EN] Error scenario: Verify negative enum values are rejected by all methods.
    /// [CN] 错误场景：验证所有方法都拒绝负数枚举值。
    /// </summary>
    [TestMethod]
    public void AllMethods_NegativeEnumValue_ShouldThrow()
    {
        FhirUserRole negativeRole = ((FhirUserRole)(-1));

        _auth.Invoking(a => a.CanAccessFullPHI(negativeRole)).Should().Throw<ArgumentException>();
        _auth.Invoking(a => a.CanModifyClinicalData(negativeRole)).Should().Throw<ArgumentException>();
        _auth.Invoking(a => a.CanViewAuditLogs(negativeRole)).Should().Throw<ArgumentException>();
        _auth.Invoking(a => a.CanAccessPatientDemographics(negativeRole)).Should().Throw<ArgumentException>();
        _auth.Invoking(a => a.CanWriteAnyResource(negativeRole)).Should().Throw<ArgumentException>();
    }
}

/// <summary>
/// [EN] Integration tests for parameter validation across shared library components.
/// [CN] 共享库组件间参数验证的集成测试。
/// Verifies that CodeStandard.md requirement "all public methods must begin with parameter validation" is met.
/// 验证CodeStandard.md要求"所有public方法必须以参数验证开始"已满足。
/// </summary>
[TestClass]
public sealed class SharedLibraryParameterValidationIntegrationTests
{
    /// <summary>
    /// [EN] Integration test: Verify all three shared library components enforce parameter validation.
    /// Tests AuditLog, ConsentManager, and RbacAuth together to ensure consistent behavior.
    /// [CN] 集成测试：验证所有三个共享库组件强制参数验证。
    /// 共同测试AuditLog、ConsentManager和RbacAuth，确保一致的行为。
    /// </summary>
    [TestMethod]
    public void AllComponents_ShouldEnforceParameterValidation()
    {
        // AuditLog: null userId should throw
        Action auditAct = () => AuditLog.Record(null!, "Physician", "1.2.3.4", "Patient", "P1", "READ");
        auditAct.Should().Throw<ArgumentNullException>("AuditLog should validate parameters");

        // ConsentManager: null patientId should throw
        var consent = new ConsentManager();
        Action consentAct = () => consent.CheckConsent(null!, "TREATMENT");
        consentAct.Should().Throw<ArgumentNullException>("ConsentManager should validate patientId parameter");

        // RbacAuth: invalid enum value should throw
        var auth = new RbacAuth();
        FhirUserRole badRole = (FhirUserRole)999;
        Action authAct = () => auth.CanAccessFullPHI(badRole);
        authAct.Should().Throw<ArgumentException>("RbacAuth should validate role parameter");
    }

    /// <summary>
    /// [EN] Integration test: Verify normal usage flow works end-to-end with all components.
    /// Simulates the HIPAA compliance demo workflow from module 07.
    /// [CN] 集成测试：验证所有组件的正常用法端到端流程。
    /// 模拟模块07中的HIPAA合规演示工作流程。
    /// </summary>
    [TestMethod]
    public void HipaaComplianceDemoWorkflow_ShouldCompleteSuccessfully()
    {
        const string userId = "dev_rex1";
        const string patientId = "P1001";

        // Step 1: RBAC check
        var auth = new RbacAuth();
        bool canAccessPHI = auth.CanAccessFullPHI(FhirUserRole.Physician);
        canAccessPHI.Should().BeTrue("Physician should have full PHI access");

        // Step 2: Consent check + Step 3: Audit log
        var consent = new ConsentManager();
        var originalOut = Console.Out;
        using var stringWriter = new System.IO.StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            bool hasConsent = consent.CheckConsent(patientId, "TREATMENT");
            hasConsent.Should().BeTrue("Consent should be granted for valid inputs");

            AuditLog.Record(
                userId: userId,
                role: "Physician",
                ipAddress: "192.168.1.100",
                resourceType: "Patient",
                resourceId: patientId,
                action: "READ"
            );

            string captured = stringWriter.ToString();
            captured.Should().Contain("HIPAA AUDIT LOG", "Audit log should be recorded");
            captured.Should().Contain(userId, "Audit log should contain user ID");
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}
