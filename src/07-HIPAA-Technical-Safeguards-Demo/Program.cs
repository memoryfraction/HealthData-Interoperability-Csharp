using HealthDataInteropSharedLibrary.Shared;
using HealthDataInteropSharedLibrary.Compliance;

namespace _07_HIPAA_Technical_Safeguards_Demo;

/// <summary>
/// Entry point: demo of HIPAA-oriented technical safeguards for PHI access (RBAC, consent, audit, PHI masking).
/// Uses SafeConsole to ensure all PHI is masked before output.
/// </summary>
internal static class Program
{
    static void Main(string[] args)
    {
        HealthData.Interop.Abstractions.SafeConsole.WriteLine("=============================================");
        HealthData.Interop.Abstractions.SafeConsole.WriteLine("      HIPAA Technical Safeguards Demo (FHIR)");
        HealthData.Interop.Abstractions.SafeConsole.WriteLine("=============================================\n");

        var orchestrator = new HipaaComplianceOrchestrator();

        const string userId = "dev_rex1";
        const FhirUserRole role = FhirUserRole.Physician;
        const string ipAddress = "192.168.1.100";
        const string patientId = "P1001";
        const string accessPurpose = "TREATMENT";

        HealthData.Interop.Abstractions.SafeConsole.WriteLine(HipaaComplianceOrchestrator.FormatUserContext(userId, role, ipAddress));
        HealthData.Interop.Abstractions.SafeConsole.WriteLine();

        // Run the safeguards workflow: RBAC -> consent -> audit log
        HealthData.Interop.Abstractions.SafeConsole.WriteLine("Step 1: RBAC Permission Check");
        var result = orchestrator.ExecutePhiAccessRequest(
            userId: userId,
            role: role,
            ipAddress: ipAddress,
            patientId: patientId,
            accessPurpose: accessPurpose);

        HealthData.Interop.Abstractions.SafeConsole.WriteLine();
        if (result)
        {
            HealthData.Interop.Abstractions.SafeConsole.WriteLine("=============================================");
            HealthData.Interop.Abstractions.SafeConsole.WriteLine(" Demo completed - simulated PHI access request was allowed (see audit log above).");
            HealthData.Interop.Abstractions.SafeConsole.WriteLine("=============================================");
        }
    }
}
