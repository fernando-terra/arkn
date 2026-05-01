using Arkn.Jobs.Core;
using Arkn.Jobs.Models;

namespace Arkn.Jobs.Tests;

public class ArknJobHistoryTests
{
    private static ArknJobExecution MakeExecution(string jobName, ArknJobStatus status = ArknJobStatus.Succeeded) =>
        new(Guid.NewGuid(), jobName, status,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1),
            null, []);

    [Fact]
    public void Record_ShouldStoreExecution()
    {
        var history = new ArknJobHistory();
        history.Record(MakeExecution("job-a"));

        Assert.Single(history.GetHistory("job-a"));
    }

    [Fact]
    public void GetHistory_UnknownJob_ShouldReturnEmpty()
    {
        var history = new ArknJobHistory();
        Assert.Empty(history.GetHistory("ghost"));
    }

    [Fact]
    public void Record_OverMax_ShouldEvictOldest()
    {
        var history = new ArknJobHistory(maxRunsPerJob: 3);
        for (int i = 0; i < 5; i++)
            history.Record(MakeExecution("job-a"));

        Assert.Equal(3, history.GetHistory("job-a").Count);
    }

    [Fact]
    public void GetHistory_IsOrderedMostRecentFirst()
    {
        var history = new ArknJobHistory();
        history.Record(MakeExecution("job-a", ArknJobStatus.Succeeded));
        history.Record(MakeExecution("job-a", ArknJobStatus.Failed));

        var entries = history.GetHistory("job-a");
        Assert.Equal(ArknJobStatus.Failed, entries[0].Status);
        Assert.Equal(ArknJobStatus.Succeeded, entries[1].Status);
    }

    [Fact]
    public void GetAllHistory_ShouldAggregateAcrossJobs()
    {
        var history = new ArknJobHistory();
        history.Record(MakeExecution("job-a"));
        history.Record(MakeExecution("job-b"));
        history.Record(MakeExecution("job-a"));

        Assert.Equal(3, history.GetAllHistory().Count);
    }
}
