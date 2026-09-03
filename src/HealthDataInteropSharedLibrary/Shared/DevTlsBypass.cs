namespace HealthDataInteropSharedLibrary.Shared;

/// <summary>
/// [EN] Gates the local-development-only TLS certificate validation bypass used by the FHIR client
/// constructors and demo modules. STRICT TLS validation is the default everywhere; this bypass is
/// OFF unless explicitly opted into via environment variable, and must NEVER be enabled in
/// production/staging (violates HIPAA 164.312(e)(1) transmission security).
/// [CN] 控制 FHIR 客户端构造函数和示例模块中"仅限本地开发"的 TLS 证书校验绕过开关。所有场景默认严格
/// TLS 验证；仅当显式设置环境变量时才会开启此绕过，且绝不能在生产/测试环境启用（违反 HIPAA
/// 164.312(e)(1) 传输安全规定）。
/// </summary>
public static class DevTlsBypass
{
    private const string EnvVarName = "HEALTHDATA_INSECURE_SKIP_TLS";

    /// <summary>
    /// [EN] True only when HEALTHDATA_INSECURE_SKIP_TLS=1 is set in the process environment.
    /// [CN] 仅当进程环境变量 HEALTHDATA_INSECURE_SKIP_TLS 设为 1 时为 true。
    /// </summary>
    public static bool IsEnabled =>
        System.Environment.GetEnvironmentVariable(EnvVarName) == "1";
}
