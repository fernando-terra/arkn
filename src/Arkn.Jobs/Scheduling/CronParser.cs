namespace Arkn.Jobs.Scheduling;

/// <summary>
/// Native cron parser supporting 5-field expressions.
/// Supported operators: * , - /
/// </summary>
public static class CronParser
{
    /// <summary>Parses a 5-field cron expression string into a <see cref="CronExpression"/>.</summary>
    /// <exception cref="FormatException">When the expression is malformed.</exception>
    public static CronExpression Parse(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new FormatException("Cron expression cannot be empty.");

        var parts = expression.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5)
            throw new FormatException($"Cron expression must have exactly 5 fields; got {parts.Length}: '{expression}'");

        return new CronExpression(
            expression,
            ParseField(parts[0], 0, 59,  "minute"),
            ParseField(parts[1], 0, 23,  "hour"),
            ParseField(parts[2], 1, 31,  "day-of-month"),
            ParseField(parts[3], 1, 12,  "month"),
            ParseField(parts[4], 0, 6,   "day-of-week"));
    }

    /// <summary>
    /// Returns the next occurrence of the cron expression strictly after <paramref name="after"/>.
    /// Returns <c>null</c> if no occurrence is found within the next 4 years.
    /// </summary>
    public static DateTimeOffset? GetNextOccurrence(CronExpression cron, DateTimeOffset after)
    {
        // Start from the next minute
        var candidate = new DateTimeOffset(
            after.Year, after.Month, after.Day,
            after.Hour, after.Minute, 0,
            after.Offset)
            .AddMinutes(1);

        var limit = after.AddYears(4);

        while (candidate <= limit)
        {
            // Month
            if (!cron.Months.Contains(candidate.Month))
            {
                candidate = AdvanceToNextMonth(candidate, cron.Months);
                continue;
            }

            // Day of month AND day of week
            if (!cron.DaysOfMonth.Contains(candidate.Day) ||
                !cron.DaysOfWeek.Contains((int)candidate.DayOfWeek))
            {
                candidate = candidate.AddDays(1);
                candidate = new DateTimeOffset(candidate.Year, candidate.Month, candidate.Day, 0, 0, 0, candidate.Offset);
                continue;
            }

            // Hour
            if (!cron.Hours.Contains(candidate.Hour))
            {
                candidate = candidate.AddHours(1);
                candidate = new DateTimeOffset(candidate.Year, candidate.Month, candidate.Day, candidate.Hour, 0, 0, candidate.Offset);
                continue;
            }

            // Minute
            if (!cron.Minutes.Contains(candidate.Minute))
            {
                candidate = candidate.AddMinutes(1);
                continue;
            }

            return candidate;
        }

        return null;
    }

    /// <summary>Convenience overload accepting a raw cron string.</summary>
    public static DateTimeOffset? GetNextOccurrence(string expression, DateTimeOffset after) =>
        GetNextOccurrence(Parse(expression), after);

    // ── Private helpers ────────────────────────────────────────────────────────

    private static IReadOnlyList<int> ParseField(string field, int min, int max, string name)
    {
        var values = new SortedSet<int>();

        foreach (var part in field.Split(','))
        {
            if (part == "*")
            {
                for (int i = min; i <= max; i++) values.Add(i);
                continue;
            }

            if (part.Contains('/'))
            {
                var slashParts = part.Split('/');
                if (slashParts.Length != 2 || !int.TryParse(slashParts[1], out int step) || step <= 0)
                    throw new FormatException($"Invalid step in cron {name} field: '{part}'");

                int start = slashParts[0] == "*" ? min : ParseSingle(slashParts[0], min, max, name);
                for (int i = start; i <= max; i += step) values.Add(i);
                continue;
            }

            if (part.Contains('-'))
            {
                var rangeParts = part.Split('-');
                if (rangeParts.Length != 2)
                    throw new FormatException($"Invalid range in cron {name} field: '{part}'");

                int from = ParseSingle(rangeParts[0], min, max, name);
                int to   = ParseSingle(rangeParts[1], min, max, name);
                if (from > to) throw new FormatException($"Invalid range (start > end) in cron {name} field: '{part}'");

                for (int i = from; i <= to; i++) values.Add(i);
                continue;
            }

            values.Add(ParseSingle(part, min, max, name));
        }

        if (values.Count == 0)
            throw new FormatException($"Cron {name} field '{field}' produced no values.");

        return values.ToList().AsReadOnly();
    }

    private static int ParseSingle(string s, int min, int max, string name)
    {
        if (!int.TryParse(s, out int v))
            throw new FormatException($"Invalid value '{s}' in cron {name} field.");
        if (v < min || v > max)
            throw new FormatException($"Value {v} out of range [{min},{max}] in cron {name} field.");
        return v;
    }

    private static DateTimeOffset AdvanceToNextMonth(DateTimeOffset dt, IReadOnlyList<int> months)
    {
        var next = new DateTimeOffset(dt.Year, dt.Month, 1, 0, 0, 0, dt.Offset).AddMonths(1);
        while (!months.Contains(next.Month) && next.Year <= dt.Year + 4)
            next = next.AddMonths(1);
        return next;
    }
}
