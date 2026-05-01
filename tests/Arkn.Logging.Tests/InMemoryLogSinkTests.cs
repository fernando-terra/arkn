using Arkn.Logging.Models;
using Arkn.Logging.Sinks;

namespace Arkn.Logging.Tests;

public class InMemoryLogSinkTests
{
    private static LogEntry MakeEntry(ArknLogLevel level, string message, string? scope = null) =>
        new(level, message, DateTimeOffset.UtcNow, scope, null, null);

    [Fact]
    public void Write_ShouldRetainEntry()
    {
        var sink = new InMemoryLogSink();
        sink.Write(MakeEntry(ArknLogLevel.Info, "hello"));

        Assert.Equal(1, sink.Count);
    }

    [Fact]
    public void GetEntries_NoScope_ShouldReturnAll()
    {
        var sink = new InMemoryLogSink();
        sink.Write(MakeEntry(ArknLogLevel.Info, "a", "run-1"));
        sink.Write(MakeEntry(ArknLogLevel.Info, "b", "run-2"));
        sink.Write(MakeEntry(ArknLogLevel.Info, "c"));

        Assert.Equal(3, sink.GetEntries().Count);
    }

    [Fact]
    public void GetEntries_WithScope_ShouldFilterByScope()
    {
        var sink = new InMemoryLogSink();
        sink.Write(MakeEntry(ArknLogLevel.Info, "a", "run-1"));
        sink.Write(MakeEntry(ArknLogLevel.Info, "b", "run-1"));
        sink.Write(MakeEntry(ArknLogLevel.Info, "c", "run-2"));

        var run1 = sink.GetEntries("run-1");
        Assert.Equal(2, run1.Count);
        Assert.All(run1, e => Assert.Equal("run-1", e.Scope));
    }

    [Fact]
    public void Clear_NoScope_ShouldRemoveAll()
    {
        var sink = new InMemoryLogSink();
        sink.Write(MakeEntry(ArknLogLevel.Info, "a", "run-1"));
        sink.Write(MakeEntry(ArknLogLevel.Info, "b", "run-2"));

        sink.Clear();

        Assert.Equal(0, sink.Count);
    }

    [Fact]
    public void Clear_WithScope_ShouldRemoveOnlyThatScope()
    {
        var sink = new InMemoryLogSink();
        sink.Write(MakeEntry(ArknLogLevel.Info, "a", "run-1"));
        sink.Write(MakeEntry(ArknLogLevel.Info, "b", "run-2"));

        sink.Clear("run-1");

        Assert.Equal(1, sink.Count);
        Assert.Empty(sink.GetEntries("run-1"));
        Assert.Single(sink.GetEntries("run-2"));
    }

    [Fact]
    public void Write_OverMaxEntries_ShouldEvictOldest()
    {
        var sink = new InMemoryLogSink(maxEntries: 3);
        for (int i = 0; i < 5; i++)
            sink.Write(MakeEntry(ArknLogLevel.Info, $"msg-{i}"));

        var entries = sink.GetEntries();
        Assert.Equal(3, entries.Count);
        Assert.Equal("msg-2", entries[0].Message);
        Assert.Equal("msg-4", entries[2].Message);
    }

    [Fact]
    public void Write_IsThreadSafe()
    {
        var sink = new InMemoryLogSink(maxEntries: 10_000);
        var tasks = Enumerable.Range(0, 100)
            .Select(i => Task.Run(() => sink.Write(MakeEntry(ArknLogLevel.Info, $"msg-{i}"))))
            .ToArray();

        Task.WaitAll(tasks);

        Assert.Equal(100, sink.Count);
    }
}
