namespace Shared_Library
{
    public class ConsentManager
    {
        // HIPAA：必须检查患者是否允许数据被访问
        // HIPAA: Must check if patient has consented to data access
        public bool CheckConsent(string patientId, string requestPurpose)
        {
            // In real scenario: Read from FHIR Consent resource
            // 真实场景：从FHIR Consent资源读取     
            // For demo, we simulate that the patient has granted consent for the requested purpose.
            // Demo：模拟患者已授权
            Console.WriteLine($"\n[CONSENT CHECK] Patient {patientId} | Purpose: {requestPurpose} | Status: GRANTED");
            return true;
        }
    }
}
