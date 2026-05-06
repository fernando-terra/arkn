using Arkn.Jobs.Dlq;
using Xunit;

namespace Arkn.Jobs.Tests;

public class InMemoryJobDlqTests
{
    [Fact]
    public async Task EnqueueAsync_ShouldAddEntry()
    {
        var dlq   = new InMemoryJobDlq();
        var entry = new FailedJobEntry { JobName = "job-a", FailedAt = DateTimeOffset.UtcNow, AttemptsMade = 3, ErrorMessage = "boom" };
        await dlq.EnqueueAsync(entry);

        var entries = await dlq.GetEntriesAsync();
        Assert.Single(entries);
        Assert.Equal("job-a", entries[0].JobName);
    }

    [Fact]
    public async Task ClearAsync_NoJobName_ShouldClearAll()
    {
        var dlq = new InMemoryJobDlq();
        await dlq.EnqueueAsync(new FailedJobEntry { JobName = "a", FailedAt = DateTimeOffset.UtcNow, AttemptsMade = 1, ErrorMessage = "e" });
        await dlq.EnqueueAsync(new FailedJobEntry { JobName = "b", FailedAt = DateTimeOffset.UtcNow, AttemptsMade = 1, ErrorMessage = "e" });

        await dlq.ClearAsync();

        var entries = await dlq.GetEntriesAsync();
        Assert.Empty(entries);
    }

    [Fact]
    public async Task ClearAsync_WithJobName_ShouldClearOnlyThatJob()
    {
        var dlq = new InMemoryJobDlq();
        await dlq.EnqueueAsync(new FailedJobEntry { JobName = "a", FailedAt = DateTimeOffset.UtcNow, AttemptsMade = 1, ErrorMessage = "e" });
        await dlq.EnqueueAsync(new FailedJobEntry { JobName = "b", FailedAt = DateTimeOffset.UtcNow, AttemptsMade = 1, ErrorMessage = "e" });

        await dlq.ClearAsync("a");

        var entries = await dlq.GetEntriesAsync();
        Assert.Single(entries);
        Assert.Equal("b", entries[0].JobName);
    }

    [Fact]
    public async Task NoOpLock_ShouldAlwaysAcquire()
    {
        var lock_ = new Arkn.Jobs.Locking.NoOpDistributedJobLock();
        var acquired = await lock_.TryAcquireAsync("any-job", TimeSpan.FromSeconds(30));
        Assert.True(acquired);
    }
}
