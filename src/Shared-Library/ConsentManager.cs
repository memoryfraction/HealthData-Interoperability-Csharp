
namespace Shared_Library
{
    /// <summary>
    /// [EN] Patient consent manager for HIPAA-compliant data access control.
    /// Verifies that patients have granted consent before their data is accessed.
    /// [CN] 患者授权管理器，用于符合HIPAA标准的数据访问控制。
    /// 在访问患者数据之前验证患者是否已授权。
    /// </summary>
    public class ConsentManager
    {
        /// <summary>
        /// [EN] Checks whether a patient has consented to data access for the specified purpose.
        /// In production, this reads from FHIR Consent resources; in demo mode, simulates granted consent.
        /// [CN] 检查患者是否已授权以指定用途访问数据。
        /// 在生产环境中，从FHIR Consent资源读取；在演示模式下，模拟已授权。
        /// </summary>
        /// <param name="patientId">[EN] Patient identifier / [CN] 患者标识符</param>
        /// <param name="requestPurpose">[EN] Purpose of data access request / [CN] 数据访问请求的用途</param>
        /// <returns>[EN] True if consent is granted / [CN] 如果已授权则返回true</returns>
        public bool CheckConsent(string patientId, string requestPurpose)
        {
            GuardAgainstNullOrEmpty(patientId, nameof(patientId));

            // In real scenario: Read from FHIR Consent resource
            // 真实场景：从FHIR Consent资源读取     
            // For demo, we simulate that the patient has granted consent for the requested purpose.
            // Demo：模拟患者已授权
            Console.WriteLine($"\n[CONSENT CHECK] Patient {patientId} | Purpose: {requestPurpose} | Status: GRANTED");
            return true;
        }

        /// <summary>
        /// [EN] Validates that a string argument is neither null nor empty.
        /// Throws ArgumentNullException for null and ArgumentException for empty, matching BCL conventions.
        /// [CN] 验证字符串参数既不为null也不为空。
        /// null抛出ArgumentNullException，空字符串抛出ArgumentException，符合BCL惯例。
        /// </summary>
        private static void GuardAgainstNullOrEmpty(string? value, string name)
        {
            if (value is null)
                throw new ArgumentNullException(name, $"Parameter '{name}' must not be null.");
            if (value.Length == 0)
                throw new ArgumentException($"Parameter '{name}' must not be empty.", name);
        }
    }
}
