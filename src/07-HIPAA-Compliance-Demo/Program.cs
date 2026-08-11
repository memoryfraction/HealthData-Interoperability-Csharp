using HealthDataInteropSharedLibrary.Shared;
using HealthDataInteropSharedLibrary.Compliance;

namespace _07_HIPAA_Compliance_Demo;

/// <summary>
/// Entry point: HIPAA compliance demo demonstrating PHI access control.
/// 入口点：演示PHI访问控制的HIPAA合规演示。
/// </summary>
internal static class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=============================================");
        Console.WriteLine("      HIPAA Compliance Demo (FHIR)");
        Console.WriteLine("=============================================\n");

        var orchestrator = new HipaaComplianceOrchestrator();

        const string userId = "dev_rex1";
        const FhirUserRole role = FhirUserRole.Physician;
        const string ipAddress = "192.168.1.100";
        const string patientId = "P1001";
        const string accessPurpose = "TREATMENT";

        Console.WriteLine(HipaaComplianceOrchestrator.FormatUserContext(userId, role, ipAddress));
        Console.WriteLine();

        // Execute full HIPAA compliance workflow
        Console.WriteLine("Step 1: RBAC Permission Check");
        var result = orchestrator.ExecutePhiAccessRequest(
            userId: userId,
            role: role,
            ipAddress: ipAddress,
            patientId: patientId,
            accessPurpose: accessPurpose);

        Console.WriteLine();
        if (result)
        {
            Console.WriteLine("=============================================");
            Console.WriteLine(" Demo completed successfully - All HIPAA checks passed.");
            Console.WriteLine("=============================================");
        }
    }
}