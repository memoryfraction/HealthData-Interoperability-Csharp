using HealthDataInteropSharedLibrary.Shared;
using HealthDataInteropSharedLibrary.Compliance;

namespace _07_HIPAA_Compliance_Demo;

/// <summary>
/// Entry point: HIPAA compliance demo demonstrating PHI access control.
/// Uses SafeConsole to ensure all PHI is masked before output.
/// </summary>
internal static class Program
{
    static void Main(string[] args)
    {
        SafeConsole.WriteLine("=============================================");
        SafeConsole.WriteLine("      HIPAA Compliance Demo (FHIR)");
        SafeConsole.WriteLine("=============================================\n");

        var orchestrator = new HipaaComplianceOrchestrator();

        const string userId = "dev_rex1";
        const FhirUserRole role = FhirUserRole.Physician;
        const string ipAddress = "192.168.1.100";
        const string patientId = "P1001";
        const string accessPurpose = "TREATMENT";

        SafeConsole.WriteLine(HipaaComplianceOrchestrator.FormatUserContext(userId, role, ipAddress));
        SafeConsole.WriteLine();

        // Execute full HIPAA compliance workflow
        SafeConsole.WriteLine("Step 1: RBAC Permission Check");
        var result = orchestrator.ExecutePhiAccessRequest(
            userId: userId,
            role: role,
            ipAddress: ipAddress,
            patientId: patientId,
            accessPurpose: accessPurpose);

        SafeConsole.WriteLine();
        if (result)
        {
            SafeConsole.WriteLine("=============================================");
            SafeConsole.WriteLine(" Demo completed successfully - All HIPAA checks passed.");
            SafeConsole.WriteLine("=============================================");
        }
    }
}
