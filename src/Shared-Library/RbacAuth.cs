namespace Shared_Library
{
    /// <summary>
    /// FHIR User Roles (HIPAA Compliant)
    /// 8 official roles for clinic, insurance, audit, and system access
    /// </summary>
    public enum FhirUserRole
    {
        SysAdmin,
        Physician,
        Nurse,
        FrontDesk,
        Biller,
        Insurance,
        Patient,
        Auditor
    }

    /// <summary>
    /// RBAC Permission Validation
    /// Implements HIPAA Minimum Necessary & Least Privilege Principle
    /// </summary>
    public class RbacAuth
    {
        /// <summary>
        /// Check if role can access FULL PHI (HIPAA Critical)
        /// </summary>
        public bool CanAccessFullPHI(FhirUserRole role)
        {
            // Only Physician allowed full PHI
            // SysAdmin/Auditor: system/logs only
            return role == FhirUserRole.Physician;
        }

        /// <summary>
        /// Check if role can modify clinical data
        /// </summary>
        public bool CanModifyClinicalData(FhirUserRole role)
        {
            return role is FhirUserRole.Physician or FhirUserRole.Nurse;
        }

        /// <summary>
        /// Check if role can view audit logs
        /// </summary>
        public bool CanViewAuditLogs(FhirUserRole role)
        {
            return role is FhirUserRole.SysAdmin or FhirUserRole.Auditor;
        }

        /// <summary>
        /// Check if role can view appointment/demographic data
        /// </summary>
        public bool CanAccessPatientDemographics(FhirUserRole role)
        {
            return role is
                FhirUserRole.Physician
                or FhirUserRole.Nurse
                or FhirUserRole.FrontDesk
                or FhirUserRole.Biller
                or FhirUserRole.Insurance
                or FhirUserRole.Patient;
        }

        /// <summary>
        /// Check if role can perform any write/modify operation
        /// </summary>
        public bool CanWriteAnyResource(FhirUserRole role)
        {
            return role is
                FhirUserRole.Physician
                or FhirUserRole.Nurse
                or FhirUserRole.Patient;
        }
    }
}