using Arkn.Logging.Models;

namespace Arkn.Logging.Tests;

public class LogEntryTests
{
    [Fact]
    public void Create_ShouldBuildMinimalEntry()
    {
        var entry = LogEntry.Create(ArknLogLevel.Info, "hello");

        Assert.Equal(ArknLogLevel.Info, entry.Level);
        Assert.Equal("hello", entry.Message);
        Assert.Null(entry.Scope);
        Assert.Null(entry.Context);
        Assert.Null(entry.Exception);
        Assert.True(entry.Timestamp <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Record_WithAllFields_ShouldBeEqual()
    {
        var ts = DateTimeOffset.UtcNow;
        var ctx = new Dictionary<string, object?> { ["k"] = "v" };
        var ex = new Exception("boom");

        var a = new LogEntry(ArknLogLevel.Error, "msg", ts, "run-1", ctx, ex);
        var b = new LogEntry(ArknLogLevel.Error, "msg", ts, "run-1", ctx, ex);

        Assert.Equal(a, b);
    }

    [Fact]
    public void Record_WithDifferentLevel_ShouldNotBeEqual()
    {
        var ts = DateTimeOffset.UtcNow;
        var a = new LogEntry(ArknLogLevel.Info,  "msg", ts, null, null, null);
        var b = new LogEntry(ArknLogLevel.Error, "msg", ts, null, null, null);

        Assert.NotEqual(a, b);
    }
}
