using HealthDataInteropSharedLibrary.Shared;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HealthData.Interop.Tests.ConsoleLoggerTests;

/// <summary>
/// Minimal tests for ConsoleLogger Serilog adapter contract.
/// </summary>
[TestClass]
public sealed class ConsoleLoggerTests
{
    [TestMethod]
    public void Information_ShouldNotThrow()
    {
        var logger = new ConsoleLogger();
        Action act = () => logger.Information("test info");
        act.Should().NotThrow();
    }

    [TestMethod]
    public void Warning_ShouldNotThrow()
    {
        var logger = new ConsoleLogger();
        Action act = () => logger.Warning("test warning");
        act.Should().NotThrow();
    }

    [TestMethod]
    public void Error_ShouldNotThrow()
    {
        var logger = new ConsoleLogger();
        Action act = () => logger.Error("test error");
        act.Should().NotThrow();
    }

    [TestMethod]
    public void Critical_ShouldNotThrow()
    {
        var logger = new ConsoleLogger();
        Action act = () => logger.Critical("test critical");
        act.Should().NotThrow();
    }
}