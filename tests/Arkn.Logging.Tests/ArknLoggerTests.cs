using Arkn.Logging.Abstractions;
using Arkn.Logging.Core;
using Arkn.Logging.Models;
using Arkn.Logging.Sinks;

namespace Arkn.Logging.Tests;

public class ArknLoggerTests
{
    private static (ArknLogger logger, InMemoryLogSink sink) Build(
        ArknLogLevel minimumLevel = ArknLogLevel.Trace)
    {
        var sink = new InMemoryLogSink();
        var logger = new ArknLogger([sink], minimumLevel);
        return (logger, sink);
    }

    [Fact]
    public void Info_ShouldWriteToSink()
    {
        var (logger, sink) = Build();
        logger.Info("hello world");

        var entries = sink.GetEntries();
        Assert.Single(entries);
        Assert.Equal(ArknLogLevel.Info, entries[0].Level);
        Assert.Equal("hello world", entries[0].Message);
    }

    [Theory]
    [InlineData(ArknLogLevel.Trace)]
    [InlineData(ArknLogLevel.Debug)]
    [InlineData(ArknLogLevel.Info)]
    [InlineData(ArknLogLevel.Warning)]
    [InlineData(ArknLogLevel.Error)]
    [InlineData(ArknLogLevel.Fatal)]
    public void AllLevels_ShouldWriteWhenAboveMinimum(ArknLogLevel level)
    {
        var (logger, sink) = Build(ArknLogLevel.Trace);

        logger.Log(level, "test");

        Assert.Single(sink.GetEntries());
        Assert.Equal(level, sink.GetEntries()[0].Level);
    }

    [Fact]
    public void BelowMinimumLevel_ShouldNotWrite()
    {
        var (logger, sink) = Build(ArknLogLevel.Warning);

        logger.Info("should be filtered");

        Assert.Empty(sink.GetEntries());
    }

    [Fact]
    public void Error_WithException_ShouldAttachException()
    {
        var (logger, sink) = Build();
        var ex = new InvalidOperationException("boom");

        logger.Error("something failed", ex);

        var entry = sink.GetEntries()[0];
        Assert.Equal(ex, entry.Exception);
    }

    [Fact]
    public void Log_WithContext_ShouldAttachScopeAndProperties()
    {
        var (logger, sink) = Build();
        var context = ArknLogContext.ForScope("job-run-42").With("UserId", 99);

        logger.Info("job started", context);

        var entry = sink.GetEntries()[0];
        Assert.Equal("job-run-42", entry.Scope);
        Assert.Equal(99, entry.Context!["UserId"]);
    }

    [Fact]
    public void MultipleSinks_AllShouldReceiveEntry()
    {
        var sink1 = new InMemoryLogSink();
        var sink2 = new InMemoryLogSink();
        var logger = new ArknLogger([sink1, sink2]);

        logger.Info("broadcast");

        Assert.Single(sink1.GetEntries());
        Assert.Single(sink2.GetEntries());
    }

    [Fact]
    public void FailingSink_ShouldNotCrashLogger()
    {
        var failing = new FailingLogSink();
        var good    = new InMemoryLogSink();
        var logger  = new ArknLogger([failing, good]);

        // Should not throw
        logger.Info("test");

        Assert.Single(good.GetEntries());
    }

    private sealed class FailingLogSink : IArknLogSink
    {
        public void Write(LogEntry entry) => throw new Exception("sink error");
    }
}
