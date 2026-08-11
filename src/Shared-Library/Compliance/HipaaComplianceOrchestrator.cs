using Shared_Library.Shared;
using Shared_Library;

namespace Shared_Library.Compliance;

/// <summary>
/// [EN] Orchestrator for HIPAA compliance demo workflow.
/// Coordinates RBAC checks, patient consent validation, and audit logging to meet Minimum Necessary Standard.
/// [CN] HIPAA合规演示工作流编排器。协调RBAC检查、患者授权验证和审计日志以符合最小必要标准。
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
    /// [EN] Execute the full HIPAA compliance workflow for a PHI access request.
    /// Returns true if all checks pass (RBAC + consent + audit logging).
    /// [CN] 执行PHI访问请求的完整HIPAA合规工作流。所有检查通过则返回true（RBAC+授权+审计日志）。
    /// </summary>
    public bool ExecutePhiAccessRequest(string userId, FhirUserRole role, string ipAddress,
        string patientId, string accessPurpose, string resourceType = "Patient", string action = "READ")
    {
        // Step 1: RBAC - Role-based access control (Least Privilege)
        if (!_rbacAuth.CanAccessFullPHI(role))
        {
            Console.WriteLine("DENIED: Insufficient role for full PHI access");
            return false;
        }
        Console.WriteLine("PASSED: User has permission to access full PHI\n");

        // Step 2: Patient Consent Validation
        if (!_consentManager.CheckConsent(patientId, accessPurpose))
        {
            Console.WriteLine("DENIED: No patient consent provided");
            return false;
        }
        Console.WriteLine("PASSED: Patient consent is granted\n");

        // Step 3: Record HIPAA-compliant audit log
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
