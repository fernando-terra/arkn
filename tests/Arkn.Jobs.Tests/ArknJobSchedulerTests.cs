using Arkn.Jobs.Scheduling;

namespace Arkn.Jobs.Tests;

// Scheduler integration test focuses on cron matching logic (firing at the right minute)
public class ArknJobSchedulerTests
{
    [Fact]
    public void CronParser_EveryMinute_ShouldMatchNow()
    {
        var now = DateTimeOffset.UtcNow;
        var cron = CronParser.Parse("* * * * *");

        Assert.Contains(now.Minute, cron.Minutes);
        Assert.Contains(now.Hour,   cron.Hours);
    }

    [Fact]
    public void CronParser_SpecificMinute_ShouldOnlyMatchThatMinute()
    {
        var cron = CronParser.Parse("30 * * * *");
        Assert.Equal([30], cron.Minutes.ToArray());
    }

    [Fact]
    public void CronParser_WeekdayOnly_ShouldExcludeWeekend()
    {
        var cron = CronParser.Parse("0 9 * * 1-5");
        Assert.DoesNotContain(0, cron.DaysOfWeek); // Sunday
        Assert.DoesNotContain(6, cron.DaysOfWeek); // Saturday
        Assert.Contains(1, cron.DaysOfWeek);       // Monday
        Assert.Contains(5, cron.DaysOfWeek);       // Friday
    }
}
