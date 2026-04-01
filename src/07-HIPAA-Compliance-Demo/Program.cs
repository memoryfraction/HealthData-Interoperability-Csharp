using Shared_Library; 

namespace _07_HIPAA_Compliance_Demo
{
    /// <summary>
    /// =============================================
    /// HIPAA Compliance Demo (FHIR)
    /// Purpose: Demonstrate 3 core requirements for handling Protected Health Information (PHI)
    /// 1. Audit logging for all PHI access
    /// 2. Patient consent validation before accessing data
    /// 3. Role-based access control (RBAC) with least privilege
    /// =============================================
    /// 中文说明:
    /// 本项目演示医疗IT系统在处理受保护健康信息(PHI)时，必须满足的3项核心HIPAA合规要求：
    /// 1. 所有PHI访问操作的审计日志记录
    /// 2. 访问数据前的患者授权验证
    /// 3. 基于角色的最小权限访问控制
    /// =============================================
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=============================================");
            Console.WriteLine("      HIPAA Compliance Demo (FHIR)");
            Console.WriteLine("=============================================\n");

            // 模拟用户身份（对应JWT Token中的身份信息）
            var currentUser = new
            {
                UserId = "dev_rex1",
                Role = FhirUserRole.Physician,
                IpAddress = "192.168.1.100"
            };
            const string patientId = "P1001";
            const string accessPurpose = "TREATMENT";

            Console.WriteLine($"[User Context] User: {currentUser.UserId} | Role: {currentUser.Role} | IP: {currentUser.IpAddress}\n");

            // 1. Step 1: RBAC - 角色权限检查（最小权限原则）
            // 1. Step 1: RBAC Permission Check (Least Privilege)
            Console.WriteLine("Step 1: RBAC Permission Check");
            var auth = new RbacAuth();
            if (!auth.CanAccessFullPHI(currentUser.Role))
            {
                Console.WriteLine("DENIED: Insufficient role for full PHI access\n");
                return;
            }
            Console.WriteLine("PASSED: User has permission to access full PHI\n");

            // 2. Step 2: 患者授权验证
            // 2. Step 2: Patient Consent Validation
            Console.WriteLine("Step 2: Patient Consent Validation");
            var consent = new ConsentManager();
            if (!consent.CheckConsent(patientId, accessPurpose))
            {
                Console.WriteLine("DENIED: No patient consent provided\n");
                return;
            }
            Console.WriteLine("PASSED: Patient consent is granted\n");

            // 3. Step 3: 访问FHIR数据并记录审计日志
            // 3. Step 3: Accessing FHIR Data & Logging Audit
            Console.WriteLine("Step 3: Accessing FHIR Patient Data & Logging Audit");
            Console.WriteLine($"Fetching data for Patient/{patientId}...\n");

            // 记录审计日志（HIPAA要求的不可篡改记录）
            // Log audit (immutable record required by HIPAA)
            AuditLog.Record(
                userId: currentUser.UserId,
                role: currentUser.Role.ToString(),
                ipAddress: currentUser.IpAddress,
                resourceType: "Patient",
                resourceId: patientId,
                action: "READ");

            Console.WriteLine("=============================================");
            Console.WriteLine(" Demo completed successfully - All HIPAA checks passed.");
            Console.WriteLine("=============================================");
        }
    }
}