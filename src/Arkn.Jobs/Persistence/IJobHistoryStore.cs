namespace Arkn.Jobs.Persistence;

/// <summary>
/// Pluggable persistence store for job execution history.
/// Default implementation: <see cref="InMemoryJobHistoryStore"/>.
/// </summary>
public interface IJobHistoryStore
{
    Task SaveAsync(JobExecutionRecord record, CancellationToken ct = default);
    Task<IReadOnlyList<JobExecutionRecord>> GetRecentAsync(string jobName, int limit = 50, CancellationToken ct = default);
}
