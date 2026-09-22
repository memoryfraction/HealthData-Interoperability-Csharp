using HealthData.Interop.Abstractions;
using Microsoft.Extensions.Logging;

namespace HealthData.Interop.Logging.Extensions;

/// <summary>
/// [EN] Bridge that adapts Microsoft.Extensions.Logging.ILogger to IApplicationLogger.
/// All messages are automatically PHI-masked via PhiMasker before reaching the underlying provider.
/// 
/// [CN] 将 Microsoft.Extensions.Logging.ILogger 适配为 IApplicationLogger 的桥梁。
/// 所有消息在到达底层日志提供者前自动通过 PhiMasker 进行 PHI 脱敏。
/// </summary>
public sealed class IApplicationLoggerBridge : IApplicationLogger
{
    private readonly ILogger _logger;

    /// <summary>
    /// [EN] Initialize the bridge with an ILogger instance.
    /// [CN] 使用 ILogger 实例初始化桥接器。
    /// </summary>
    /// <param name="logger">[EN] The ILogger instance to delegate to / [CN] 要委托的 ILogger 实例</param>
    public IApplicationLoggerBridge(ILogger logger)
    {
        Guard.NotNull(logger, nameof(logger));
        _logger = logger;
    }

    /// <summary>
    /// [EN] Log an informational message via the underlying ILogger.
    /// [CN] 通过底层 ILogger 记录信息级别消息。
    /// </summary>
    public void Information(string message)
    {
        Guard.NotNull(message, nameof(message));
        _logger.Log(LogLevel.Information, PhiMasker.Mask(message));
    }

    /// <summary>
    /// [EN] Log a warning message via the underlying ILogger.
    /// [CN] 通过底层 ILogger 记录警告级别消息。
    /// </summary>
    public void Warning(string message)
    {
        Guard.NotNull(message, nameof(message));
        _logger.Log(LogLevel.Warning, PhiMasker.Mask(message));
    }

    /// <summary>
    /// [EN] Log an error message with optional exception context via the underlying ILogger.
    /// [CN] 通过底层 ILogger 记录错误级别消息，带有可选的异常上下文。
    /// </summary>
    public void Error(string message, Exception? ex = null)
    {
        Guard.NotNull(message, nameof(message));
        if (ex is not null)
            _logger.Log(LogLevel.Error, ex, PhiMasker.Mask(message));
        else
            _logger.Log(LogLevel.Error, PhiMasker.Mask(message));
    }

    /// <summary>
    /// [EN] Log a critical security event via the underlying ILogger.
    /// [CN] 通过底层 ILogger 记录关键安全事件。
    /// </summary>
    public void Critical(string message)
    {
        Guard.NotNull(message, nameof(message));
        _logger.Log(LogLevel.Critical, PhiMasker.Mask(message));
    }
}

/// <summary>
/// [EN] Extension methods for Microsoft.Extensions.Logging.ILogger to easily obtain an IApplicationLogger.
/// [CN] Microsoft.Extensions.Logging.ILogger 的扩展方法，方便获取 IApplicationLogger 实例。
/// </summary>
public static class IApplicationLoggerExtensions
{
    /// <summary>
    /// [EN] Convert an ILogger instance to an IApplicationLogger with automatic PHI masking.
    /// [CN] 将 ILogger 实例转换为带有自动 PHI 脱敏的 IApplicationLogger。
    /// </summary>
    /// <param name="logger">[EN] The ILogger instance / [CN] ILogger 实例</param>
    /// <returns>[EN] An IApplicationLogger that delegates to the given ILogger / [CN] 委托给指定 ILogger 的 IApplicationLogger 实例</returns>
    public static IApplicationLogger ToIApplicationLogger(this ILogger logger)
    {
        Guard.NotNull(logger, nameof(logger));
        return new IApplicationLoggerBridge(logger);
    }
}
