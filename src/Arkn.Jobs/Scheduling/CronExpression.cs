namespace Arkn.Jobs.Scheduling;

/// <summary>
/// Represents a parsed 5-field cron expression.
/// Fields: minute hour day-of-month month day-of-week
/// Operators: * , - /
/// </summary>
public sealed class CronExpression
{
    public IReadOnlyList<int> Minutes     { get; }  // 0-59
    public IReadOnlyList<int> Hours       { get; }  // 0-23
    public IReadOnlyList<int> DaysOfMonth { get; }  // 1-31
    public IReadOnlyList<int> Months      { get; }  // 1-12
    public IReadOnlyList<int> DaysOfWeek  { get; }  // 0-6 (0=Sunday)

    public string Raw { get; }

    internal CronExpression(
        string raw,
        IReadOnlyList<int> minutes,
        IReadOnlyList<int> hours,
        IReadOnlyList<int> daysOfMonth,
        IReadOnlyList<int> months,
        IReadOnlyList<int> daysOfWeek)
    {
        Raw = raw;
        Minutes     = minutes;
        Hours       = hours;
        DaysOfMonth = daysOfMonth;
        Months      = months;
        DaysOfWeek  = daysOfWeek;
    }
}
