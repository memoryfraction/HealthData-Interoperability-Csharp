namespace HealthData.Interop.Abstractions;

/// <summary>
/// [EN] Application logger abstraction for audit trails oriented around HIPAA Security Rule concepts.
/// Allows redirecting log output to files, databases, or console without modifying service code.
/// Per CodeStandard.md: all public methods have bilingual XML docs and parameter guards.
/// 
/// [CN] 应用程序日志记录器抽象，用于围绕HIPAA安全规则概念的审计追踪。
/// 允许将日志输出重定向到文件、数据库或控制台，而无需修改服务代码。
/// 按照CodeStandard.md要求：所有public方法具有双语XML文档和参数保护子句。
/// </summary>
public interface IApplicationLogger
{
    /// <summary>
    /// [EN] Log an informational message.
    /// [CN] 记录信息级别消息。
    /// </summary>
    void Information(string message);

    /// <summary>
    /// [EN] Log a warning message.
    /// [CN] 记录警告级别消息。
    /// </summary>
    void Warning(string message);

    /// <summary>
    /// [EN] Log an error message with an optional exception context.
    /// [CN] 记录错误级别消息，带有可选的异常上下文。
    /// </summary>
    void Error(string message, Exception? ex = null);

    /// <summary>
    /// [EN] Log a critical security event (HIPAA audit-worthy).
    /// [CN] 记录关键安全事件（HIPAA审计级别）。
    /// </summary>
    void Critical(string message);
}
