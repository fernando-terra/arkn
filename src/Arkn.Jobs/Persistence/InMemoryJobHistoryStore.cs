namespace Arkn.Jobs.Persistence;

/// <summary>
/// In-memory circular buffer implementation of <see cref="IJobHistoryStore"/>.
/// Thread-safe. Retains the last <see cref="Capacity"/> records per job.
/// </summary>
public sealed class InMemoryJobHistoryStore : IJobHistoryStore
{
    private readonly int _capacity;
    private readonly Dictionary<string, LinkedList<JobExecutionRecord>> _store = new();
    private readonly object _lock = new();

    public int Capacity => _capacity;

    public InMemoryJobHistoryStore(int capacity = 100)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        _capacity = capacity;
    }

    public Task SaveAsync(JobExecutionRecord record, CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (!_store.TryGetValue(record.JobName, out var list))
            {
                list = new LinkedList<JobExecutionRecord>();
                _store[record.JobName] = list;
            }
            list.AddFirst(record);
            while (list.Count > _capacity) list.RemoveLast();
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<JobExecutionRecord>> GetRecentAsync(
        string jobName, int limit = 50, CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (!_store.TryGetValue(jobName, out var list))
                return Task.FromResult<IReadOnlyList<JobExecutionRecord>>([]);

            var result = list.Take(limit).ToList().AsReadOnly();
            return Task.FromResult<IReadOnlyList<JobExecutionRecord>>(result);
        }
    }
}
