namespace Arkn.Jobs.Models;

/// <summary>Configuration options for a registered job.</summary>
public sealed class ArknJobOptions
{
    /// <summary>Machine-readable job identifier. Defaults to the type name.</summary>
    public string JobName { get; set; } = string.Empty;

    /// <summary>Human-readable description shown in dashboards.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Cron expression (5 fields: min hour dom month dow).</summary>
    public string CronExpression { get; set; } = string.Empty;

    /// <summary>Maximum wall-clock time allowed per execution. Null = no timeout.</summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>Maximum number of attempts (initial + retries). Default: 1 (no retry).</summary>
    public int MaxAttempts { get; set; } = 1;

    /// <summary>Base delay between retry attempts. Multiplied linearly by attempt index.</summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>The CLR type of the job implementation.</summary>
    public Type JobType { get; set; } = typeof(object);
}
