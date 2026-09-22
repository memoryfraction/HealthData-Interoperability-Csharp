using HealthData.Interop.Abstractions;
using HealthData.Interop.Logging.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HealthData.Interop.Tests.IApplicationLoggerBridgeTests;

/// <summary>
/// Tests for IApplicationLoggerBridge: verifies that Information/Warning/Error/Critical
/// are correctly mapped to ILogger methods, and that PHI masking is applied.
/// </summary>
[TestClass]
public sealed class IApplicationLoggerBridgeTests
{
    private static (IApplicationLogger bridge, List<(LogLevel Level, string Message)> entries) CreateBridge()
    {
        var entries = new List<(LogLevel Level, string Message)>();
        var provider = new CapturingProvider(entries);
        var logger = new LoggerFactory(new[] { provider }).CreateLogger("test");
        var bridge = logger.ToIApplicationLogger();
        return (bridge, entries);
    }

    private sealed class CapturingProvider : ILoggerProvider
    {
        private readonly List<(LogLevel Level, string Message)> _entries;
        public CapturingProvider(List<(LogLevel Level, string Message)> entries) => _entries = entries;

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(_entries);
        public void Dispose() { }

        private sealed class CapturingLogger : ILogger
        {
            private readonly List<(LogLevel Level, string Message)> _entries;
            public CapturingLogger(List<(LogLevel Level, string Message)> entries) => _entries = entries;

            public IDisposable? BeginScope<TState>(TState state) { throw new NotSupportedException(); }
            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                lock (_entries)
                    _entries.Add((logLevel, formatter(state, exception)));
            }
        }
    }

    [TestMethod]
    public void Information_ShouldMapToInformation_WithMasking()
    {
        var (bridge, entries) = CreateBridge();
        bridge.Information("Patient SSN 123-45-6789 called");
        entries.Should().HaveCount(1);
        entries[0].Level.Should().Be(LogLevel.Information);
        entries[0].Message.Should().Contain("***-**-****").And.NotContain("123-45-6789");
    }

    [TestMethod]
    public void Warning_ShouldMapToWarning_WithMasking()
    {
        var (bridge, entries) = CreateBridge();
        bridge.Warning("Contact: john.doe@example.com");
        entries.Should().HaveCount(1);
        entries[0].Level.Should().Be(LogLevel.Warning);
        entries[0].Message.Should().Contain("[EMAIL_MASKED]").And.NotContain("john.doe@example.com");
    }

    [TestMethod]
    public void Error_WithException_ShouldMapToError_WithMasking()
    {
        var (bridge, entries) = CreateBridge();
        var ex = new InvalidOperationException("test");
        bridge.Error("Patient 555.123.4567 not found", ex);
        entries.Should().HaveCount(1);
        entries[0].Level.Should().Be(LogLevel.Error);
        entries[0].Message.Should().Contain("[PHONE_MASKED]").And.NotContain("555.123.4567");
    }

    [TestMethod]
    public void Critical_ShouldMapToCritical_WithMasking()
    {
        var (bridge, entries) = CreateBridge();
        bridge.Critical("Security breach at 192.168.1.100");
        entries.Should().HaveCount(1);
        entries[0].Level.Should().Be(LogLevel.Critical);
        entries[0].Message.Should().Contain("[IP_MASKED]").And.NotContain("192.168.1.100");
    }

    [TestMethod]
    public void NullMessage_ShouldThrow_ArgumentNullException()
    {
        var (bridge, _) = CreateBridge();
        Action act = () => bridge.Information(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
