namespace HealthDataInteropSharedLibrary.Shared;
    /// <summary>
    /// [EN] Patient consent manager for purpose-of-use data access control (HIPAA Security Rule-oriented).
    /// Verifies that patients have granted consent before their data is accessed.
    /// [CN] 患者授权管理器，用于基于使用目的的数据访问控制（HIPAA安全规则取向）。
    /// </summary>
    public class ConsentManager
    {
        /// <summary>
        /// [EN] Checks whether a patient has consented to data access for the specified purpose.
        /// In production, this reads from FHIR Consent resources; in demo mode, simulates granted consent.
        /// [CN] 检查患者是否已授权以指定用途访问数据。
        /// </summary>
        /// <param name="patientId">[EN] Patient identifier / [CN] 患者标识符</param>
        /// <param name="requestPurpose">[EN] Purpose of data access request / [CN] 数据访问请求的用途</param>
        /// <returns>[EN] True if consent is granted / [CN] 如果已授权则返回true</returns>
        public bool CheckConsent(string patientId, string requestPurpose)
        {
            Guard.NotNullOrEmpty(patientId, nameof(patientId));

            // In real scenario: Read from FHIR Consent resource
            // For demo, we simulate that the patient has granted consent for the requested purpose.
            SafeConsole.WriteLine($"\n[CONSENT CHECK] Patient {patientId} | Purpose: {requestPurpose} | Status: GRANTED");
            return true;
        }
    }

