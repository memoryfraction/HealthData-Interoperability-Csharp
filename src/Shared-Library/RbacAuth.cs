
namespace Shared_Library
{
    /// <summary>
    /// [EN] FHIR User Roles (HIPAA Compliant) - 8 official roles for clinic, insurance, audit, and system access.
    /// [CN] FHIR用户角色（符合HIPAA标准）- 诊所、保险、审计和系统访问的8个官方角色。
    /// </summary>
    public enum FhirUserRole
    {
        /// <summary>[EN] System Administrator / [CN] 系统管理员</summary>
        SysAdmin,
        /// <summary>[EN] Physician - Full PHI access / [CN] 医师 - 完整PHI访问</summary>
        Physician,
        /// <summary>[EN] Nurse - Clinical data access / [CN] 护士 - 临床数据访问</summary>
        Nurse,
        /// <summary>[EN] Front Desk - Appointment/demographic access / [CN] 前台 - 预约/人口统计访问</summary>
        FrontDesk,
        /// <summary>[EN] Biller - Billing and demographics / [CN] 计费员 - 计费和人口统计</summary>
        Biller,
        /// <summary>[EN] Insurance - Coverage verification / [CN] 保险 - 覆盖验证</summary>
        Insurance,
        /// <summary>[EN] Patient - Own data access / [CN] 患者 - 自身数据访问</summary>
        Patient,
        /// <summary>[EN] Auditor - Audit log viewing / [CN] 审计员 - 审计日志查看</summary>
        Auditor
    }

    /// <summary>
    /// [EN] RBAC Permission Validation - Implements HIPAA Minimum Necessary &amp; Least Privilege Principle.
    /// [CN] RBAC权限验证 - 实现HIPAA最小必要和最小权限原则。
    /// </summary>
    public class RbacAuth
    {
        /// <summary>
        /// [EN] Check if role can access FULL PHI (HIPAA Critical). Only Physicians are permitted.
        /// [CN] 检查角色是否可以访问完整PHI（HIPAA关键）。仅医师被允许。
        /// </summary>
        /// <param name="role">[EN] The FHIR user role to check / [CN] 要检查的FHIR用户角色</param>
        /// <returns>[EN] True if the role can access full PHI / [CN] 如果角色可以访问完整PHI则返回true</returns>
        public bool CanAccessFullPHI(FhirUserRole role)
        {
            GuardValidRole(role, nameof(role));

            // Only Physician allowed full PHI
            // SysAdmin/Auditor: system/logs only
            return role == FhirUserRole.Physician;
        }

        /// <summary>
        /// [EN] Check if role can modify clinical data. Physicians and Nurses are permitted.
        /// [CN] 检查角色是否可以修改临床数据。医师和护士被允许。
        /// </summary>
        /// <param name="role">[EN] The FHIR user role to check / [CN] 要检查的FHIR用户角色</param>
        /// <returns>[EN] True if the role can modify clinical data / [CN] 如果角色可以修改临床数据则返回true</returns>
        public bool CanModifyClinicalData(FhirUserRole role)
        {
            GuardValidRole(role, nameof(role));

            return role is FhirUserRole.Physician or FhirUserRole.Nurse;
        }

        /// <summary>
        /// [EN] Check if role can view audit logs. System Administrators and Auditors are permitted.
        /// [CN] 检查角色是否可以查看审计日志。系统管理员和审计员被允许。
        /// </summary>
        /// <param name="role">[EN] The FHIR user role to check / [CN] 要检查的FHIR用户角色</param>
        /// <returns>[EN] True if the role can view audit logs / [CN] 如果角色可以查看审计日志则返回true</returns>
        public bool CanViewAuditLogs(FhirUserRole role)
        {
            GuardValidRole(role, nameof(role));

            return role is FhirUserRole.SysAdmin or FhirUserRole.Auditor;
        }

        /// <summary>
        /// [EN] Check if role can view appointment/demographic data.
        /// Physicians, Nurses, FrontDesk, Billers, Insurance, and Patients are permitted.
        /// [CN] 检查角色是否可以查看预约/人口统计数据。
        /// 医师、护士、前台、计费员、保险和患者被允许。
        /// </summary>
        /// <param name="role">[EN] The FHIR user role to check / [CN] 要检查的FHIR用户角色</param>
        /// <returns>[EN] True if the role can access demographics / [CN] 如果角色可以访问人口统计则返回true</returns>
        public bool CanAccessPatientDemographics(FhirUserRole role)
        {
            GuardValidRole(role, nameof(role));

            return role is
                FhirUserRole.Physician
                or FhirUserRole.Nurse
                or FhirUserRole.FrontDesk
                or FhirUserRole.Biller
                or FhirUserRole.Insurance
                or FhirUserRole.Patient;
        }

        /// <summary>
        /// [EN] Check if role can perform any write/modify operation.
        /// Physicians, Nurses, and Patients are permitted.
        /// [CN] 检查角色是否可以执行任何写入/修改操作。
        /// 医师、护士和患者被允许。
        /// </summary>
        /// <param name="role">[EN] The FHIR user role to check / [CN] 要检查的FHIR用户角色</param>
        /// <returns>[EN] True if the role can write resources / [CN] 如果角色可以写入资源则返回true</returns>
        public bool CanWriteAnyResource(FhirUserRole role)
        {
            GuardValidRole(role, nameof(role));

            return role is
                FhirUserRole.Physician
                or FhirUserRole.Nurse
                or FhirUserRole.Patient;
        }

        /// <summary>
        /// [EN] Validates that the enum parameter is a defined FhirUserRole value.
        /// [CN] 验证枚举参数是已定义的FhirUserRole值。
        /// </summary>
        private static void GuardValidRole(FhirUserRole role, string paramName)
        {
            if (!Enum.IsDefined(typeof(FhirUserRole), role))
                throw new ArgumentException($"Invalid role value: {role}", paramName);
        }
    }
}
