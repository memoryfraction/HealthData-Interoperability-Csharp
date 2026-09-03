using HealthDataInteropSharedLibrary.Shared;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace HealthDataInteropSharedLibrary.BasicClient;

/// <summary>
/// [EN] Service for basic FHIR Patient CRUD operations. Encapsulates patient creation and search logic.
/// [CN] 基础FHIR Patient CRUD操作服务。封装患者创建和查询逻辑。
/// </summary>
public sealed class FhirBasicService
{
    private readonly FhirClient _client;

    /// <summary>
    /// [EN] Initialize with a FHIR server URL. TLS certificate validation is STRICT by default.
    /// [CN] 使用 FHIR 服务器 URL 初始化。TLS 证书验证默认严格开启。
    /// </summary>
    /// <remarks>
    /// [EN] A certificate-validation bypass is available ONLY for local development and is OFF by default.
    ///     To opt in (dev only), set the environment variable HEALTHDATA_INSECURE_SKIP_TLS=1 before
    ///     starting the process. NEVER set this in production/staging. Disabling TLS validation violates
    ///     HIPAA 164.312(e)(1) transmission security rule.
    /// [CN] 证书校验绕过仅用于本地开发，且默认关闭。如需开启（仅限开发），请在启动进程前设置环境变量
    ///     HEALTHDATA_INSECURE_SKIP_TLS=1。切勿在生产/测试环境设置。禁用 TLS 验证违反 HIPAA 164.312(e)(1) 传输安全规定。
    /// </remarks>
    /// <param name="fhirServerUrl">[EN] FHIR server base URL / [CN] FHIR 服务器基础URL</param>
    public FhirBasicService(string fhirServerUrl)
    {
        Guard.NotNullOrEmpty(fhirServerUrl, nameof(fhirServerUrl));

        var handler = new System.Net.Http.SocketsHttpHandler
        {
            ConnectTimeout = System.TimeSpan.FromSeconds(30)
        };

        // SECURITY NOTICE / 安全说明:
        // [EN] TLS certificate validation is STRICT by default. A certificate-validation bypass is available ONLY for local
        //     development (e.g. behind a proxy/firewall with a self-signed MITM cert) and is OFF by default.
        // [CN] TLS 证书验证默认严格开启。证书校验绕过仅用于本地开发（例如企业代理/防火墙的自签名中间人证书），且默认关闭。
        //     TO OPT IN (dev only) / 如需开启（仅限开发）: set the environment variable HEALTHDATA_INSECURE_SKIP_TLS=1 before starting the process.
        //     NEVER set this in production/staging. Disabling TLS validation violates HIPAA 164.312(e)(1).
        //     切勿在生产/测试环境设置。禁用 TLS 验证违反 HIPAA 164.312(e)(1) 传输安全规定。
        if (DevTlsBypass.IsEnabled)
        {
            handler.SslOptions = new System.Net.Security.SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true
            };
        }

        // Note: We intentionally don't dispose HttpClient here because FhirClient holds a reference to it.
        // In production, use IHttpClientFactory or DI container for proper lifecycle management.
        var httpClient = new System.Net.Http.HttpClient(handler)
        {
            Timeout = System.TimeSpan.FromMinutes(2)
        };

        var settings = new FhirClientSettings { PreferredFormat = ResourceFormat.Json };
        _client = new FhirClient(fhirServerUrl, httpClient, settings);
    }

    /// <summary>
    /// [EN] Create a Patient resource on the FHIR server.
    /// [CN] 在FHIR服务器上创建Patient资源。
    /// </summary>
    public async System.Threading.Tasks.Task<Patient> CreatePatientAsync(IReadOnlyList<string> givenNames, string familyName, string gender, string birthDate, string identifierValue)
    {
        Guard.NotNull(givenNames, nameof(givenNames));
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
                    Given = givenNames,
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

        SafeConsole.WriteLine($"Sending patient {patient.Name[0].Given.FirstOrDefault()} {patient.Name[0].Family}...");
        var created = await _client.CreateAsync(patient);
        return created;
    }

    /// <summary>
    /// [EN] Search for patients by name parameter.
    /// [CN] 按名字参数搜索患者。
    /// </summary>
    public async System.Threading.Tasks.Task<List<Patient>> SearchPatientsByNameAsync(string name)
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
                throw new System.ArgumentNullException(name, $"Parameter '{name}' must not be null.");
        }

        public static void NotNullOrEmpty(string? value, string name)
        {
            if (value is null)
                throw new System.ArgumentNullException(name, $"Parameter '{name}' must not be null.");
            if (value.Length == 0)
                throw new System.ArgumentException($"Parameter '{name}' must not be empty.", name);
        }
    }
}
