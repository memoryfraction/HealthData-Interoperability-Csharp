using HealthData.Interop.Abstractions;
using Serilog;

namespace HealthData.Interop.Logging.Serilog;

/// <summary>
/// [EN] Serilog-backed logger implementation for HIPAA audit trails.
/// All messages are logged as structured events with UTC timestamps via Serilog.
/// 
/// [CN] 基于Serilog的日志记录器实现，用于HIPAA审计追踪。
/// 所有消息均通过Serilog以结构化事件形式记录，附带UTC时间戳。
/// </summary>
public sealed class ConsoleLogger : IApplicationLogger
{
    private static readonly global::Serilog.ILogger _log = global::Serilog.Log.ForContext(typeof(ConsoleLogger));

    /// <summary>
    /// [EN] Log an informational message via Serilog.
    /// [CN] 通过Serilog记录信息级别消息。
    /// </summary>
    public void Information(string message)
    {
        Guard.NotNull(message, nameof(message));
        _log.Information(PhiMasker.Mask(message));
    }

    /// <summary>
    /// [EN] Log a warning message via Serilog.
    /// [CN] 通过Serilog记录警告级别消息。
    /// </summary>
    public void Warning(string message)
    {
        Guard.NotNull(message, nameof(message));
        _log.Warning(PhiMasker.Mask(message));
    }

    /// <summary>
    /// [EN] Log an error message via Serilog with optional exception context.
    /// [CN] 通过Serilog记录错误级别消息，带有可选的异常上下文。
    /// </summary>
    public void Error(string message, Exception? ex = null)
    {
        Guard.NotNull(message, nameof(message));
        if (ex is not null)
            _log.Error(ex, PhiMasker.Mask(message));
        else
            _log.Error(PhiMasker.Mask(message));
    }

    /// <summary>
    /// [EN] Log a critical security event via Serilog.
    /// [CN] 通过Serilog记录关键安全事件。
    /// </summary>
    public void Critical(string message)
    {
        Guard.NotNull(message, nameof(message));
        _log.Fatal(PhiMasker.Mask(message));
    }
}
