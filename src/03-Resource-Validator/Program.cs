using HealthDataInteropSharedLibrary.ResourceValidator;
using Hl7.Fhir.Model;

namespace _03_Resource_Validator;

/// <summary>
/// Entry point: Demonstrating FHIR resource validation.
/// 入口点：演示FHIR资源验证。
/// </summary>
internal static class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        Console.WriteLine("=== FHIR Resource Validation (Module 03) ===");

        var testPatient = new Hl7.Fhir.Model.Patient
        {
            Active = true,
            BirthDate = "1990-13-45",
            Gender = Hl7.Fhir.Model.AdministrativeGender.Male
        };
        testPatient.Telecom.Add(new Hl7.Fhir.Model.ContactPoint { System = Hl7.Fhir.Model.ContactPoint.ContactPointSystem.Phone });

        Console.WriteLine("Validating patient against FHIR R4 rules...");

        try
        {
            var validator = new ResourceValidationService();

            if (validator.HasFullSpec)
            {
                Console.WriteLine("[Info] Full FHIR spec validation available.");
            }
            else
            {
                Console.WriteLine("[Info] Running basic structural validation (spec download unavailable).");
            }

            var issues = validator.GetValidationIssues(testPatient);
            var isValid = validator.Validate(testPatient);
            Console.WriteLine(ResourceValidationService.FormatValidationResult(isValid, issues.Count, issues));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical technical error: {ex.Message}");
        }
    }
}
