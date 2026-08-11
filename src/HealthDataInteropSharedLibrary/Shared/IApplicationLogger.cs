namespace HealthDataInteropSharedLibrary.Shared;

/// <summary>
/// [EN] Application logger abstraction for HIPAA-compliant audit trails.
/// Allows redirecting log output to files, databases, or console without modifying service code.
/// Per CodeStandard.md: all public methods have bilingual XML docs and parameter guards.
/// 
/// [CN] 应用程序日志记录器抽象，用于HIPAA合规审计追踪。
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
    /// [EN] Log a critical security/compliance event (HIPAA audit-worthy).
    /// [CN] 记录关键安全/合规事件（HIPAA审计级别）。
    /// </summary>
    void Critical(string message);
}

/// <summary>
/// [EN] Serilog-backed logger implementation for HIPAA audit trails.
/// All messages are logged as structured events with UTC timestamps via Serilog.
/// 
/// [CN] 基于Serilog的日志记录器实现，用于HIPAA审计追踪。
/// 所有消息均通过Serilog以结构化事件形式记录，附带UTC时间戳。
/// </summary>
public sealed class ConsoleLogger : IApplicationLogger
{
    private static readonly Serilog.ILogger _log = Serilog.Log.ForContext<ConsoleLogger>();

    /// <summary>
    /// [EN] Log an informational message via Serilog.
    /// [CN] 通过Serilog记录信息级别消息。
    /// </summary>
    public void Information(string message)
    {
        Guard.NotNull(message, nameof(message));
        _log.Information(message);
    }

    /// <summary>
    /// [EN] Log a warning message via Serilog.
    /// [CN] 通过Serilog记录警告级别消息。
    /// </summary>
    public void Warning(string message)
    {
        Guard.NotNull(message, nameof(message));
        _log.Warning(message);
    }

    /// <summary>
    /// [EN] Log an error message via Serilog with optional exception context.
    /// [CN] 通过Serilog记录错误级别消息，带有可选的异常上下文。
    /// </summary>
    public void Error(string message, Exception? ex = null)
    {
        Guard.NotNull(message, nameof(message));
        if (ex is not null)
            _log.Error(ex, message);
        else
            _log.Error(message);
    }

    /// <summary>
    /// [EN] Log a critical security/compliance event via Serilog.
    /// [CN] 通过Serilog记录关键安全/合规事件。
    /// </summary>
    public void Critical(string message)
    {
        Guard.NotNull(message, nameof(message));
        _log.Fatal(message);
    }
}

