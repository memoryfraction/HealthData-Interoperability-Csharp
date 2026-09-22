using HealthDataInteropSharedLibrary.Shared;
using HealthDataInteropSharedLibrary;

using HealthData.Interop.Abstractions;

namespace HealthDataInteropSharedLibrary.Compliance;

/// <summary>
/// [EN] Orchestrator for the HIPAA technical-safeguards demo workflow (illustrative, not a compliance certification).
/// Coordinates RBAC checks, patient consent validation, and audit logging to meet Minimum Necessary Standard.
/// [CN] HIPAA技术保障演示工作流编排器（示例性质，不构成合规认证）。协调RBAC检查、患者授权验证和审计日志，体现最小必要原则。
/// </summary>
public sealed class HipaaComplianceOrchestrator
{
    private readonly RbacAuth _rbacAuth;
    private readonly ConsentManager _consentManager;

    /// <summary>
    /// [EN] Initialize with RBAC auth and consent manager instances.
    /// [CN] 使用RBAC授权和授权管理器实例初始化。
    /// </summary>
    public HipaaComplianceOrchestrator()
    {
        _rbacAuth = new RbacAuth();
        _consentManager = new ConsentManager();
    }

    /// <summary>
    /// [EN] Execute the full safeguards-demo workflow for a simulated PHI access request.
    /// Returns true if all checks pass (RBAC + consent + audit logging).
    /// [CN] 对模拟的PHI访问请求执行完整的安全保障演示流程。所有检查通过则返回true（RBAC+授权+审计日志）。
    /// </summary>
    public bool ExecutePhiAccessRequest(string userId, FhirUserRole role, string ipAddress,
        string patientId, string accessPurpose, string resourceType = "Patient", string action = "READ")
    {
        // Step 1: RBAC - Role-based access control (Least Privilege)
        if (!_rbacAuth.CanAccessFullPHI(role))
        {
            SafeConsole.WriteLine("DENIED: Insufficient role for full PHI access");
            return false;
        }
        SafeConsole.WriteLine("PASSED: User has permission to access full PHI\n");

        // Step 2: Patient Consent Validation
        if (!_consentManager.CheckConsent(patientId, accessPurpose))
        {
            SafeConsole.WriteLine("DENIED: No patient consent provided");
            return false;
        }
        SafeConsole.WriteLine("PASSED: Patient consent is granted\n");

        // Step 3: Record the audit log entry
        AuditLog.Record(userId, role.ToString(), ipAddress, resourceType, patientId, action);
        return true;
    }

    /// <summary>
    /// [EN] Check only the RBAC permission without full workflow.
    /// Returns true if the role can access full PHI.
    /// [CN] 仅检查RBAC权限而不执行完整工作流。如果角色可以访问完整PHI则返回true。
    /// </summary>
    public bool CanAccessFullPHI(FhirUserRole role) => _rbacAuth.CanAccessFullPHI(role);

    /// <summary>
    /// [EN] Get user context display string.
    /// [CN] 获取用户上下文显示字符串。
    /// </summary>
    public static string FormatUserContext(string userId, FhirUserRole role, string ipAddress) =>
        $"[User Context] User: {userId} | Role: {role} | IP: {ipAddress}";
}

