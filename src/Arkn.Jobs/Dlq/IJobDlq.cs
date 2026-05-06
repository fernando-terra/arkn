namespace Arkn.Jobs.Dlq;

/// <summary>
/// Dead-letter queue for permanently failed job runs.
/// Populated after all retry attempts are exhausted.
/// </summary>
public interface IJobDlq
{
    Task EnqueueAsync(FailedJobEntry entry, CancellationToken ct = default);
    Task<IReadOnlyList<FailedJobEntry>> GetEntriesAsync(CancellationToken ct = default);
    Task ClearAsync(string? jobName = null, CancellationToken ct = default);
}
