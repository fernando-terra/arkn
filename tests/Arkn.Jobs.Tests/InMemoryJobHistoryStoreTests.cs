using Arkn.Jobs.Persistence;
using Xunit;

namespace Arkn.Jobs.Tests;

public class InMemoryJobHistoryStoreTests
{
    [Fact]
    public async Task SaveAsync_ShouldPersistRecord()
    {
        var store  = new InMemoryJobHistoryStore();
        var record = new JobExecutionRecord { JobName = "job-a", StartedAt = DateTimeOffset.UtcNow, Success = true };
        await store.SaveAsync(record);

        var results = await store.GetRecentAsync("job-a");
        Assert.Single(results);
        Assert.Equal("job-a", results[0].JobName);
        Assert.True(results[0].Success);
    }

    [Fact]
    public async Task SaveAsync_OverCapacity_ShouldEvictOldest()
    {
        var store = new InMemoryJobHistoryStore(capacity: 3);
        for (int i = 0; i < 5; i++)
            await store.SaveAsync(new JobExecutionRecord { JobName = "j", StartedAt = DateTimeOffset.UtcNow, Success = true });

        var results = await store.GetRecentAsync("j");
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task GetRecentAsync_UnknownJob_ShouldReturnEmpty()
    {
        var store   = new InMemoryJobHistoryStore();
        var results = await store.GetRecentAsync("unknown");
        Assert.Empty(results);
    }

    [Fact]
    public async Task GetRecentAsync_ShouldRespectLimit()
    {
        var store = new InMemoryJobHistoryStore();
        for (int i = 0; i < 20; i++)
            await store.SaveAsync(new JobExecutionRecord { JobName = "j", StartedAt = DateTimeOffset.UtcNow, Success = true });

        var results = await store.GetRecentAsync("j", limit: 5);
        Assert.Equal(5, results.Count);
    }
}
