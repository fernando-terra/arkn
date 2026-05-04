namespace Arkn.Jobs.Scheduling;

/// <summary>
/// Represents a parsed 5-field cron expression.
/// Fields: minute hour day-of-month month day-of-week
/// Operators: * , - /
/// </summary>
public sealed class CronExpression
{
    /// <summary>Valid minute values (0–59) for this expression.</summary>
    public IReadOnlyList<int> Minutes     { get; }  // 0-59
    /// <summary>Valid hour values (0–23) for this expression.</summary>
    public IReadOnlyList<int> Hours       { get; }  // 0-23
    /// <summary>Valid day-of-month values (1–31) for this expression.</summary>
    public IReadOnlyList<int> DaysOfMonth { get; }  // 1-31
    /// <summary>Valid month values (1–12) for this expression.</summary>
    public IReadOnlyList<int> Months      { get; }  // 1-12
    /// <summary>Valid day-of-week values (0–6, where 0 = Sunday) for this expression.</summary>
    public IReadOnlyList<int> DaysOfWeek  { get; }  // 0-6 (0=Sunday)

    /// <summary>The original raw cron string as provided by the caller.</summary>
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
