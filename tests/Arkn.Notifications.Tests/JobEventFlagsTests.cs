using Arkn.Jobs.Models;

namespace Arkn.Notifications.Tests;

public class JobEventFlagsTests
{
    [Fact]
    public void JobEvent_All_ShouldContainAllFlags()
    {
        Assert.True(JobEvent.All.HasFlag(JobEvent.Started));
        Assert.True(JobEvent.All.HasFlag(JobEvent.Succeeded));
        Assert.True(JobEvent.All.HasFlag(JobEvent.Failed));
        Assert.True(JobEvent.All.HasFlag(JobEvent.TimedOut));
    }

    [Fact]
    public void JobEvent_None_ShouldNotContainAnyFlag()
    {
        Assert.False(JobEvent.None.HasFlag(JobEvent.Started));
        Assert.False(JobEvent.None.HasFlag(JobEvent.Failed));
    }

    [Fact]
    public void JobEvent_FailedAndTimedOut_ShouldCombineCorrectly()
    {
        var combined = JobEvent.Failed | JobEvent.TimedOut;
        Assert.True(combined.HasFlag(JobEvent.Failed));
        Assert.True(combined.HasFlag(JobEvent.TimedOut));
        Assert.False(combined.HasFlag(JobEvent.Started));
        Assert.False(combined.HasFlag(JobEvent.Succeeded));
    }

    [Fact]
    public void ArknJobOptions_NotifyOn_DefaultsToNone()
    {
        var opts = new ArknJobOptions();
        Assert.Equal(JobEvent.None, opts.NotifyOn);
    }
}
