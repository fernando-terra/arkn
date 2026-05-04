using Arkn.Notifications.Abstractions;
using Arkn.Notifications.Core;
using Arkn.Notifications.Models;

namespace Arkn.Notifications.Tests;

public class ArknNotifierRegistryTests
{
    private sealed class RecordingNotifier : IArknNotifier
    {
        public List<ArknNotification> Received { get; } = [];
        public Task NotifyAsync(ArknNotification n, CancellationToken ct = default)
        {
            Received.Add(n);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingNotifier : IArknNotifier
    {
        public Task NotifyAsync(ArknNotification n, CancellationToken ct = default) =>
            throw new InvalidOperationException("boom");
    }

    [Fact]
    public async Task DispatchAsync_ShouldCallAllNotifiers()
    {
        var a = new RecordingNotifier();
        var b = new RecordingNotifier();
        var registry = new ArknNotifierRegistry([a, b]);
        var notification = ArknNotification.Error("Title", "Body", "source");

        await registry.DispatchAsync(notification);

        Assert.Single(a.Received);
        Assert.Single(b.Received);
    }

    [Fact]
    public async Task DispatchAsync_FailingNotifier_ShouldNotCrashOthers()
    {
        var good    = new RecordingNotifier();
        var failing = new ThrowingNotifier();
        var registry = new ArknNotifierRegistry([failing, good]);

        await registry.DispatchAsync(ArknNotification.Info("T", "B", "s")); // should not throw

        Assert.Single(good.Received);
    }

    [Fact]
    public async Task DispatchAsync_ShouldPassCorrectNotification()
    {
        var notifier = new RecordingNotifier();
        var registry = new ArknNotifierRegistry([notifier]);
        var n = ArknNotification.Critical("Crit", "Something bad", "jobs/invoice");

        await registry.DispatchAsync(n);

        var received = Assert.Single(notifier.Received);
        Assert.Equal("Crit", received.Title);
        Assert.Equal(NotificationLevel.Critical, received.Level);
        Assert.Equal("jobs/invoice", received.Source);
    }
}
