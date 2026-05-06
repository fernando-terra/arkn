namespace Arkn.Jobs.Dlq;

/// <summary>
/// In-memory DLQ implementation. Thread-safe. Unbounded (callers should call ClearAsync periodically).
/// </summary>
public sealed class InMemoryJobDlq : IJobDlq
{
    private readonly List<FailedJobEntry> _entries = [];
    private readonly object _lock = new();

    public Task EnqueueAsync(FailedJobEntry entry, CancellationToken ct = default)
    {
        lock (_lock) _entries.Add(entry);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<FailedJobEntry>> GetEntriesAsync(CancellationToken ct = default)
    {
        lock (_lock)
            return Task.FromResult<IReadOnlyList<FailedJobEntry>>(_entries.ToList().AsReadOnly());
    }

    public Task ClearAsync(string? jobName = null, CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (jobName is null) _entries.Clear();
            else _entries.RemoveAll(e => e.JobName == jobName);
        }
        return Task.CompletedTask;
    }

    public int Count { get { lock (_lock) return _entries.Count; } }
}
