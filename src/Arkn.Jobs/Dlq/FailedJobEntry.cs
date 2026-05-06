namespace Arkn.Jobs.Dlq;

/// <summary>An entry in the dead-letter queue representing a permanently failed job run.</summary>
public sealed class FailedJobEntry
{
    public required string JobName { get; init; }
    public DateTimeOffset FailedAt { get; init; }
    public int AttemptsMade { get; init; }
    public required string ErrorMessage { get; init; }
}
