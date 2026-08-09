using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace HealthData.Interop.BasicClient;

/// <summary>
/// [EN] Service for basic FHIR Patient CRUD operations. Encapsulates patient creation and search logic.
/// [CN] 基础FHIR Patient CRUD操作服务。封装患者创建和查询逻辑。
/// </summary>
public sealed class FhirBasicService
{
    private readonly FhirClient _client;

    /// <summary>
    /// [EN] Initialize with a FHIR server URL.
    /// [CN] 使用FHIR服务器URL初始化。
    /// </summary>
    public FhirBasicService(string fhirServerUrl)
    {
        Guard.NotNullOrEmpty(fhirServerUrl, nameof(fhirServerUrl));
        _client = new FhirClient(fhirServerUrl);
    }

    /// <summary>
    /// [EN] Create a Patient resource on the FHIR server.
    /// [CN] 在FHIR服务器上创建Patient资源。
    /// </summary>
    public async Task<Patient> CreatePatientAsync(string firstName, string familyName, string gender, string birthDate, string identifierValue)
    {
        Guard.NotNullOrEmpty(firstName, nameof(firstName));
        Guard.NotNullOrEmpty(familyName, nameof(familyName));
        Guard.NotNullOrEmpty(gender, nameof(gender));
        Guard.NotNullOrEmpty(birthDate, nameof(birthDate));
        Guard.NotNullOrEmpty(identifierValue, nameof(identifierValue));

        var patient = new Patient
        {
            Name = new List<HumanName>
            {
                new HumanName
                {
                    Given = new[] { firstName },
                    Family = familyName
                }
            },
            Gender = Enum.TryParse<AdministrativeGender>(gender, true, out var g) ? g : AdministrativeGender.Unknown,
            BirthDate = birthDate,
            Identifier = new List<Identifier>
            {
                new Identifier { Value = identifierValue }
            }
        };

        Console.WriteLine($"Sending patient {patient.Name[0].Given.FirstOrDefault()} {patient.Name[0].Family}...");
        var created = await _client.CreateAsync(patient);
        return created;
    }

    /// <summary>
    /// [EN] Search for patients by name parameter.
    /// [CN] 按名字参数搜索患者。
    /// </summary>
    public async Task<List<Patient>> SearchPatientsByNameAsync(string name)
    {
        Guard.NotNullOrEmpty(name, nameof(name));

        var results = await _client.SearchAsync("Patient", new[] { $"name={name}" });
        var patients = new List<Patient>();

        foreach (var entry in results.Entry)
        {
            if (entry.Resource is Patient pat)
                patients.Add(pat);
        }

        return patients;
    }

    /// <summary>
    /// [EN] Format patient display string.
    /// [CN] 格式化患者显示字符串。
    /// </summary>
    public static string FormatPatientName(Patient patient)
    {
        Guard.NotNull(patient, nameof(patient));
        var given = patient.Name?[0]?.Given?.FirstOrDefault() ?? "";
        var family = patient.Name?[0]?.Family ?? "";
        return $"{given} {family}".Trim();
    }

    /// <summary>
    /// [EN] Parameter validation helpers.
    /// [CN] 参数验证辅助方法。
    /// </summary>
    private static class Guard
    {
        public static void NotNull(object? value, string name)
        {
            if (value is null)
                throw new ArgumentNullException(name, $"Parameter '{name}' must not be null.");
        }

        public static void NotNullOrEmpty(string? value, string name)
        {
            if (value is null)
                throw new ArgumentNullException(name, $"Parameter '{name}' must not be null.");
            if (value.Length == 0)
                throw new ArgumentException($"Parameter '{name}' must not be empty.", name);
        }
    }
}
