using HealthDataInteropSharedLibrary.Shared;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace HealthDataInteropSharedLibrary.AdvancedQuery;

/// <summary>
/// [EN] Service for advanced FHIR search queries including chained parameters, _include, and _revinclude.
/// [CN] 高级FHIR搜索查询服务，包括链式参数、_include和_revinclude。
/// </summary>
public sealed class AdvancedQueryService
{
    private readonly FhirClient _client;

    /// <summary>
    /// [EN] Initialize with a FHIR server URL and HTTPS configuration.
    /// 
    /// IMPORTANT / 重要说明:
    /// [EN] enableHttps controls TLS certificate validation behavior.
    ///      Default is true (strict HTTPS/TLS validation).
    ///      Set to false ONLY for local development when network issues prevent connecting to remote FHIR servers.
    /// PRODUCTION REQUIREMENT: Always use enableHttps = true in production. Disabling TLS violates HIPAA 164.312(e)(1).
    /// [CN] enableHttps 控制 TLS 证书验证。默认值为 true。仅本地开发时可设为 false。生产环境必须为 true。
    /// </summary>
    public AdvancedQueryService(string fhirServerUrl, bool enableHttps = true)
    {
        Guard.NotNullOrEmpty(fhirServerUrl, nameof(fhirServerUrl));

        if (enableHttps)
        {
            _client = new FhirClient(fhirServerUrl);
        }
        else
        {
            var handler = new System.Net.Http.SocketsHttpHandler();
            handler.SslOptions = new System.Net.Security.SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback = 
                    (sender, certificate, chain, sslPolicyErrors) => sslPolicyErrors == System.Net.Security.SslPolicyErrors.None
            };
            _client = new FhirClient(fhirServerUrl, new System.Net.Http.HttpClient(handler));
        }
    }

    /// <summary>
    /// [EN] Search Encounters by practitioner name with forward and reverse includes.
    /// Fetches Encounter records where participant's practitioner name matches the given search term,
    /// includes related Patient resources, and reverse-includes Observation resources.
    /// [CN] 按医生姓名搜索就诊记录，包含相关的患者资源和反向包含观察指标资源。
    /// </summary>
    public async System.Threading.Tasks.Task<Bundle> SearchEncountersByPractitionerNameAsync(string practitionerName)
    {
        Guard.NotNullOrEmpty(practitionerName, nameof(practitionerName));

        var q = new SearchParams();

        // Chained Parameters: participant.individual.name
        q.Where($"participant.individual.name:contains={practitionerName}");

        // Forward Include: Encounter -> Patient
        q.Include("Encounter:patient");

        // Reverse Include: Observation -> Patient
        q.Include("Observation:patient");

        var results = await _client.SearchAsync<Encounter>(q);
        return results;
    }

    /// <summary>
    /// [EN] Format search result entries for display.
    /// [CN] 格式化搜索结果条目以供显示。
    /// </summary>
    public static string FormatSearchResult(Bundle bundle)
    {
        Guard.NotNull(bundle, nameof(bundle));

        if (bundle.Entry.Count == 0)
            return "No matching resources found.";

        var lines = new List<string>();
        foreach (var entry in bundle.Entry)
        {
            lines.Add($"Resource found: {entry.Resource.TypeName}/{entry.Resource.Id}");
        }
        return string.Join(System.Environment.NewLine, lines);
    }
}

/// <summary>
/// [EN] Parameter validation helpers.
/// [CN] 参数验证辅助方法。
/// </summary>
internal static class Guard
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
