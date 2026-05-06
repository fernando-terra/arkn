namespace Arkn.Jobs.Persistence;

/// <summary>Immutable record of a completed job execution, suitable for persistence.</summary>
public sealed class JobExecutionRecord
{
    public required string JobName { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? FinishedAt { get; init; }
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeSpan Duration => FinishedAt.HasValue ? FinishedAt.Value - StartedAt : TimeSpan.Zero;
}
