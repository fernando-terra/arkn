using Arkn.Jobs.Scheduling;

namespace Arkn.Jobs.Tests;

public class CronParserTests
{
    [Theory]
    [InlineData("* * * * *")]
    [InlineData("0 2 * * *")]
    [InlineData("0 8 * * 1")]
    [InlineData("*/5 * * * *")]
    [InlineData("0,30 9-17 * * 1-5")]
    [InlineData("0 0 1 1 *")]
    public void Parse_ValidExpressions_ShouldNotThrow(string expr)
    {
        var cron = CronParser.Parse(expr);
        Assert.NotNull(cron);
        Assert.Equal(expr, cron.Raw);
    }

    [Theory]
    [InlineData("* * * *")]           // only 4 fields
    [InlineData("60 * * * *")]        // minute out of range
    [InlineData("* 24 * * *")]        // hour out of range
    [InlineData("* * 32 * *")]        // day out of range
    [InlineData("* * * 13 *")]        // month out of range
    [InlineData("* * * * 7")]         // dow out of range
    [InlineData("abc * * * *")]       // non-numeric
    [InlineData("5-2 * * * *")]       // inverted range
    public void Parse_InvalidExpressions_ShouldThrow(string expr)
    {
        Assert.Throws<FormatException>(() => CronParser.Parse(expr));
    }

    [Fact]
    public void GetNextOccurrence_EveryMinute_ShouldReturnNextMinute()
    {
        var cron = CronParser.Parse("* * * * *");
        var after = new DateTimeOffset(2026, 5, 1, 12, 30, 0, TimeSpan.Zero);
        var next = CronParser.GetNextOccurrence(cron, after);

        Assert.NotNull(next);
        Assert.Equal(new DateTimeOffset(2026, 5, 1, 12, 31, 0, TimeSpan.Zero), next);
    }

    [Fact]
    public void GetNextOccurrence_DailyAt2AM_ShouldReturnCorrectTime()
    {
        var cron = CronParser.Parse("0 2 * * *");
        var after = new DateTimeOffset(2026, 5, 1, 3, 0, 0, TimeSpan.Zero); // after 2am
        var next = CronParser.GetNextOccurrence(cron, after);

        Assert.NotNull(next);
        Assert.Equal(new DateTimeOffset(2026, 5, 2, 2, 0, 0, TimeSpan.Zero), next);
    }

    [Fact]
    public void GetNextOccurrence_EveryFiveMinutes_ShouldRespectStep()
    {
        var cron = CronParser.Parse("*/5 * * * *");
        var after = new DateTimeOffset(2026, 5, 1, 10, 7, 0, TimeSpan.Zero);
        var next = CronParser.GetNextOccurrence(cron, after);

        Assert.NotNull(next);
        Assert.Equal(10, next!.Value.Minute % 5); // should be 10:10
        Assert.Equal(10, next.Value.Minute);
    }

    [Fact]
    public void GetNextOccurrence_MondayAt8AM_ShouldReturnNextMonday()
    {
        var cron = CronParser.Parse("0 8 * * 1"); // Monday
        // 2026-05-01 is a Friday
        var after = new DateTimeOffset(2026, 5, 1, 9, 0, 0, TimeSpan.Zero);
        var next = CronParser.GetNextOccurrence(cron, after);

        Assert.NotNull(next);
        Assert.Equal(DayOfWeek.Monday, next!.Value.DayOfWeek);
        Assert.Equal(8, next.Value.Hour);
        Assert.Equal(0, next.Value.Minute);
    }

    [Fact]
    public void GetNextOccurrence_CommaList_ShouldMatchAnyInList()
    {
        var cron = CronParser.Parse("0 9,17 * * *");
        var after = new DateTimeOffset(2026, 5, 1, 10, 0, 0, TimeSpan.Zero);
        var next = CronParser.GetNextOccurrence(cron, after);

        Assert.NotNull(next);
        Assert.Equal(17, next!.Value.Hour);
        Assert.Equal(0, next.Value.Minute);
    }

    [Fact]
    public void GetNextOccurrence_StringOverload_ShouldWork()
    {
        var after = new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero);
        var next = CronParser.GetNextOccurrence("* * * * *", after);
        Assert.NotNull(next);
    }
}
