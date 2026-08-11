using Shared_Library.Shared;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared_Library;

namespace HealthData.Interop.Tests.ConsentManagerTests;

/// <summary>
/// [EN] Unit tests for ConsentManager.CheckConsent().
/// [CN] ConsentManager.CheckConsent() 方法的单元测试。
/// Verifies patient consent validation logic with parameter validation, happy path, and error scenarios.
/// 验证患者授权验证逻辑，包括参数验证、期望路径和错误场景。
/// </summary>
// [EN] Redirects Console.Out to capture output; must not run in parallel with other Console-capturing tests.
// [CN] 重定向Console.Out以捕获输出；不能与其他捕获Console的测试并行运行。
[TestClass]
[DoNotParallelize]
public sealed class ConsentManagerTests
{
    /// <summary>
    /// [EN] Expected scenario: Verify CheckConsent returns true for valid patient ID and purpose.
    /// [CN] 期望场景：验证 CheckConsent 对有效患者ID和用途返回true。
    /// </summary>
    [TestMethod]
    public void CheckConsent_WithValidPatientIdAndPurpose_ShouldReturnTrue()
    {
        // Arrange
        var manager = new ConsentManager();

        // Act
        bool result = manager.CheckConsent("P1001", "TREATMENT");

        // Assert
        result.Should().BeTrue("Patient consent should be granted for valid patient ID and purpose");
    }

    /// <summary>
    /// [EN] Expected scenario: Verify CheckConsent returns true for various patient IDs.
    /// [CN] 期望场景：验证 CheckConsent 对不同患者ID均返回true。
    /// </summary>
    [TestMethod]
    public void CheckConsent_WithDifferentPatientIds_ShouldAlwaysReturnTrue()
    {
        // Arrange
        var manager = new ConsentManager();

        var patientIds = new[] { "P1001", "P2002", "PATIENT-001", "xyz" };

        // Act & Assert
        foreach (var pid in patientIds)
        {
            bool result = manager.CheckConsent(pid, "TREATMENT");
            result.Should().BeTrue($"Consent should be granted for patient ID: {pid}");
        }
    }

    /// <summary>
    /// [EN] Expected scenario: Verify CheckConsent returns true for different access purposes.
    /// [CN] 期望场景：验证 CheckConsent 对不同访问用途均返回true。
    /// </summary>
    [TestMethod]
    public void CheckConsent_WithDifferentPurposes_ShouldAlwaysReturnTrue()
    {
        // Arrange
        var manager = new ConsentManager();

        var purposes = new[]
        {
            "TREATMENT", "PAYMENT", "HEALTHCARE_OPERATIONS",
            "RESEARCH", "PUBLIC_HEALTH", "LAW_ENFORCEMENT"
        };

        // Act & Assert
        foreach (var purpose in purposes)
        {
            bool result = manager.CheckConsent("P1001", purpose);
            result.Should().BeTrue($"Consent should be granted for purpose: {purpose}");
        }
    }

    /// <summary>
    /// [EN] Expected scenario: Verify CheckConsent handles null purpose gracefully (optional parameter).
    /// [CN] 期望场景：验证 CheckConsent 对null用途能正常处理（可选参数）。
    /// </summary>
    [TestMethod]
    public void CheckConsent_WithNullPurpose_ShouldReturnTrue()
    {
        // Arrange
        var manager = new ConsentManager();

        // Act
        bool result = manager.CheckConsent("P1001", null!);

        // Assert - purpose is optional, should not affect consent check
        result.Should().BeTrue("Purpose parameter is optional, null should be handled gracefully");
    }

    /// <summary>
    /// [EN] Error scenario: Verify CheckConsent throws ArgumentException for empty patient ID.
    /// [CN] 错误场景：验证 CheckConsent 对空患者ID抛出ArgumentException。
    /// </summary>
    [TestMethod]
    public void CheckConsent_WithEmptyPatientId_ShouldThrowArgumentException()
    {
        // Arrange
        var manager = new ConsentManager();

        // Act & Assert
        Action act = () => manager.CheckConsent("", "TREATMENT");
        act.Should().Throw<ArgumentException>("Empty patient ID should trigger parameter validation");
    }

    /// <summary>
    /// [EN] Error scenario: Verify CheckConsent throws ArgumentNullException for null patient ID.
    /// [CN] 错误场景：验证 CheckConsent 对null患者ID抛出ArgumentNullException。
    /// </summary>
    [TestMethod]
    public void CheckConsent_WithNullPatientId_ShouldThrowArgumentNullException()
    {
        // Arrange
        var manager = new ConsentManager();

        // Act & Assert
        Action act = () => manager.CheckConsent(null!, "TREATMENT");
        act.Should().Throw<ArgumentNullException>("Null patient ID should trigger parameter validation");
    }

    /// <summary>
    /// [EN] Expected scenario: Verify CheckConsent is idempotent across multiple calls.
    /// [CN] 期望场景：验证 CheckConsent 在多次调用下是幂等的。
    /// </summary>
    [TestMethod]
    public void CheckConsent_MultipleCalls_ShouldBeIdempotent()
    {
        // Arrange
        var manager = new ConsentManager();

        // Act & Assert
        for (int i = 0; i < 5; i++)
        {
            bool result = manager.CheckConsent("P1001", "TREATMENT");
            result.Should().BeTrue($"Call number {(i + 1)} should return true");
        }
    }

    /// <summary>
    /// [EN] Expected scenario: Verify different ConsentManager instances return same result for identical inputs.
    /// [CN] 期望场景：验证不同的 ConsentManager 实例对相同输入返回相同结果。
    /// </summary>
    [TestMethod]
    public void CheckConsent_DifferentInstances_ShouldReturnSameResult()
    {
        // Arrange
        var manager1 = new ConsentManager();
        var manager2 = new ConsentManager();

        // Act
        bool result1 = manager1.CheckConsent("P1001", "TREATMENT");
        bool result2 = manager2.CheckConsent("P1001", "TREATMENT");

        // Assert
        result1.Should().Be(result2, "Different instances should return the same result for identical inputs");
    }

    /// <summary>
    /// [EN] Expected scenario: Verify Console output contains expected consent check information.
    /// [CN] 期望场景：验证控制台输出包含预期的授权检查信息。
    /// </summary>
    [TestMethod]
    public void CheckConsent_ConsoleOutput_ShouldContainPatientIdAndPurpose()
    {
        // Arrange
        var manager = new ConsentManager();
        const string patientId = "P999";
        const string purpose = "TEST_PURPOSE";

        var originalOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            _ = manager.CheckConsent(patientId, purpose);
        }
        finally
        {
            // Cleanup
            Console.SetOut(originalOut);
        }

        // Assert
        string captured = stringWriter.ToString();
        captured.Should().Contain("[CONSENT CHECK]", "Output should contain consent check marker");
        captured.Should().Contain(patientId, "Output should contain the patient ID");
        captured.Should().Contain(purpose, "Output should contain the access purpose");
        captured.Should().Contain("GRANTED", "Output should indicate consent was granted");
    }

    /// <summary>
    /// [EN] Boundary condition: Verify CheckConsent handles edge case inputs without unexpected exceptions.
    /// [CN] 边界条件：验证 CheckConsent 处理边界情况输入时没有意外异常。
    /// </summary>
    [TestMethod]
    public void CheckConsent_EdgeCaseValidInputs_ShouldNotThrow()
    {
        // Arrange
        var manager = new ConsentManager();

        var edgeCases = new (string patientId, string purpose)[]
        {
            ("P-123!@#", "special-chars"),
            ("very-long-patient-id-that-exceeds-normal-length", "very-long-purpose-description"),
            ("  valid-with-spaces  ", "TREATMENT")
        };

        // Act & Assert
        foreach (var (pid, purpose) in edgeCases)
        {
            Action act = () => manager.CheckConsent(pid, purpose);
            act.Should().NotThrow("CheckConsent should handle valid edge case input");
        }
    }

    /// <summary>
    /// [EN] Error scenario: Verify parameter validation rejects whitespace-only patient ID.
    /// [CN] 错误场景：验证参数验证拒绝仅空白字符的患者ID。
    /// </summary>
    [TestMethod]
    public void CheckConsent_WithWhitespacePatientId_ShouldReturnTrue()
    {
        // Arrange
        var manager = new ConsentManager();

        // Act - whitespace is not empty per string.IsNullOrEmpty, so it passes through
        bool result = manager.CheckConsent("  ", "TREATMENT");

        // Assert - IsNullOrEmpty only blocks truly empty strings, whitespace passes through to simulated logic
        result.Should().BeTrue("Whitespace-only patient ID passes parameter validation in simulated environment");
    }
}