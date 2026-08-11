using Shared_Library.Shared;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared_Library;

namespace HealthData.Interop.Tests.AuditLogTests;

/// <summary>
/// [EN] Unit tests for AuditLog.Record() static method.
/// [CN] AuditLog.Record() 静态方法的单元测试。
/// Verifies HIPAA audit log recording: who, when, what action, what resource, and IP address.
/// 验证HIPAA审计日志记录：谁、何时、什么操作、什么资源、IP地址。
/// </summary>
// [EN] Redirects Console.Out to capture output; must not run in parallel with other Console-capturing tests.
// [CN] 重定向Console.Out以捕获输出；不能与其他捕获Console的测试并行运行。
[TestClass]
[DoNotParallelize]
public sealed class AuditLogTests
{
    /// <summary>
    /// [EN] Verify Record() produces JSON output containing all required HIPAA fields.
    /// [CN] 验证 Record() 产生的JSON输出包含所有HIPAA必需字段。
    /// </summary>
    [TestMethod]
    public void Record_WithValidInput_ShouldContainAllRequiredFields()
    {
        // Arrange
        var originalOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            AuditLog.Record(
                userId: "user001",
                role: "Physician",
                ipAddress: "192.168.1.100",
                resourceType: "Patient",
                resourceId: "P1001",
                action: "READ"
            );

            // Assert
            string captured = stringWriter.ToString();
            captured.Should().Contain("user001", "Output should contain the user ID");
            captured.Should().Contain("Physician", "Output should contain the role");
            captured.Should().Contain("192.168.1.100", "Output should contain the IP address");
            captured.Should().Contain("READ", "Output should contain the action");
            captured.Should().Contain("Patient/P1001", "Output should contain the resource path");
            captured.Should().Contain("Timestamp", "Output should contain timestamp field");
            captured.Should().Contain("UserId", "Output should contain UserId field name");
            captured.Should().Contain("Role", "Output should contain Role field name");
            captured.Should().Contain("IpAddress", "Output should contain IpAddress field name");
            captured.Should().Contain("Action", "Output should contain Action field name");
            captured.Should().Contain("Resource", "Output should contain Resource field name");
            captured.Should().Contain("AuditMessage", "Output should contain AuditMessage field name");
            captured.Should().Contain("HIPAA compliance", "Output should contain HIPAA compliance message");
        }
        finally
        {
            // Cleanup
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// [EN] Verify Record() includes the HIPAA audit log header and footer markers.
    /// [CN] 验证 Record() 包含HIPAA审计日志的头部和尾部标记。
    /// </summary>
    [TestMethod]
    public void Record_Output_ShouldContainHeaderAndFooterMarkers()
    {
        // Arrange
        var originalOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            AuditLog.Record("user001", "Nurse", "10.0.0.1", "Observation", "O001", "WRITE");

            // Assert
            string captured = stringWriter.ToString();
            captured.Should().Contain("HIPAA AUDIT LOG", "Output should contain audit log header");
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// [EN] Verify Record() output is properly formatted indented JSON.
    /// [CN] 验证 Record() 输出为正确格式的缩进JSON。
    /// </summary>
    [TestMethod]
    public void Record_Output_ShouldBeIndentedJson()
    {
        // Arrange
        var originalOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            AuditLog.Record("admin1", "SysAdmin", "127.0.0.1", "Practitioner", "PR001", "READ");

            // Assert - Indented JSON will have newlines and spaces for indentation
            string captured = stringWriter.ToString();
            captured.Should().Contain("\n", "Indented JSON output should contain newlines");
            captured.Should().Contain("{", "JSON output should contain opening brace");
            captured.Should().Contain("}", "JSON output should contain closing brace");
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// [EN] Verify Record() timestamp uses ISO 8601 format (UTC).
    /// [CN] 验证 Record() 时间戳使用ISO 8601格式（UTC）。
    /// </summary>
    [TestMethod]
    public void Record_Timestamp_ShouldBeIso8601UtcFormat()
    {
        // Arrange
        var originalOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            AuditLog.Record("user001", "Physician", "192.168.1.1", "Patient", "P1", "READ");

            // Assert - UTC timestamps end with 'Z' in ISO 8601 format
            string captured = stringWriter.ToString();
            captured.Should().MatchRegex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}", "Timestamp should follow ISO 8601 pattern");
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// [EN] Verify Record() handles various resource types without throwing.
    /// [CN] 验证 Record() 处理各种资源类型时不抛异常。
    /// </summary>
    [TestMethod]
    public void Record_VariousResourceTypes_ShouldNotThrow()
    {
        // Arrange
        var originalOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            var resourceTypes = new[]
            {
                ("Patient", "P001"),
                ("Observation", "O001"),
                ("Condition", "C001"),
                ("MedicationRequest", "MR001"),
                ("Encounter", "E001")
            };

            // Act & Assert
            foreach (var (type, id) in resourceTypes)
            {
                Action act = () => AuditLog.Record(
                    userId: "user001",
                    role: "Physician",
                    ipAddress: "192.168.1.1",
                    resourceType: type,
                    resourceId: id,
                    action: "READ"
                );

                act.Should().NotThrow($"Record should handle resource type '{type}' without throwing");
            }
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// [EN] Verify Record() handles various action types (READ, WRITE, DELETE, UPDATE) without throwing.
    /// [CN] 验证 Record() 处理各种操作类型（READ、WRITE、DELETE、UPDATE）时不抛异常。
    /// </summary>
    [TestMethod]
    public void Record_VariousActions_ShouldNotThrow()
    {
        // Arrange
        var originalOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            var actions = new[] { "READ", "WRITE", "DELETE", "UPDATE" };

            // Act & Assert
            foreach (var action in actions)
            {
                AuditLog.Record(
                    userId: "user001",
                    role: "Physician",
                    ipAddress: "192.168.1.1",
                    resourceType: "Patient",
                    resourceId: "P001",
                    action: action
                );

                string captured = stringWriter.ToString();
                captured.Should().Contain(action, $"Output should contain action '{action}'");
            }
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// [EN] Verify Record() is a static method and can be called without instantiation.
    /// [CN] 验证 Record() 是静态方法，无需实例化即可调用。
    /// </summary>
    [TestMethod]
    public void Record_ShouldBeStaticAndCallableWithoutInstance()
    {
        // Arrange & Act - Simply calling the static method directly (no instance needed)
        Action act = () => AuditLog.Record(
            userId: "user001",
            role: "Auditor",
            ipAddress: "10.0.0.1",
            resourceType: "AuditEvent",
            resourceId: "A001",
            action: "READ"
        );

        // Assert - should not throw
        act.Should().NotThrow("Record is a static method and should be callable without creating an instance");
    }

    /// <summary>
    /// [EN] Verify Record() output contains the audit message about PHI access.
    /// [CN] 验证 Record() 输出包含关于PHI访问的审计消息。
    /// </summary>
    [TestMethod]
    public void Record_Output_ShouldContainAuditMessageAboutPhiAccess()
    {
        // Arrange
        var originalOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            AuditLog.Record("user001", "Physician", "192.168.1.1", "Patient", "P1", "READ");

            // Assert
            string captured = stringWriter.ToString();
            captured.Should().Contain("PHI access recorded", "Output should contain PHI access message");
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// [EN] Verify Record() correctly combines resourceType and resourceId into a slash-separated path.
    /// [CN] 验证 Record() 正确将resourceType和resourceId组合成斜杠分隔的路径。
    /// </summary>
    [TestMethod]
    public void Record_ResourcePath_ShouldCombineTypeAndIdWithSlash()
    {
        // Arrange
        var originalOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            AuditLog.Record(
                userId: "user001",
                role: "Physician",
                ipAddress: "192.168.1.1",
                resourceType: "DiagnosticReport",
                resourceId: "DR-2024-001",
                action: "READ"
            );

            // Assert
            string captured = stringWriter.ToString();
            captured.Should().Contain("DiagnosticReport/DR-2024-001", "Resource path should combine type and ID with slash");
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}
