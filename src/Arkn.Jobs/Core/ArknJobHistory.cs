using Arkn.Jobs.Models;

namespace Arkn.Jobs.Core;

/// <summary>
/// Thread-safe in-memory store for job execution history.
/// Retains the last <see cref="MaxRunsPerJob"/> executions per job.
/// </summary>
public sealed class ArknJobHistory
{
    private readonly Dictionary<string, LinkedList<ArknJobExecution>> _history = new();
    private readonly Lock _lock = new();

    /// <summary>Maximum executions retained per job name.</summary>
    public int MaxRunsPerJob { get; }

    public ArknJobHistory(int maxRunsPerJob = 100) => MaxRunsPerJob = maxRunsPerJob;

    /// <summary>Records a completed execution.</summary>
    public void Record(ArknJobExecution execution)
    {
        lock (_lock)
        {
            if (!_history.TryGetValue(execution.JobName, out var list))
            {
                list = new LinkedList<ArknJobExecution>();
                _history[execution.JobName] = list;
            }

            list.AddFirst(execution);

            while (list.Count > MaxRunsPerJob)
                list.RemoveLast();
        }
    }

    /// <summary>Returns the last N executions for a job, most recent first.</summary>
    public IReadOnlyList<ArknJobExecution> GetHistory(string jobName)
    {
        lock (_lock)
        {
            return _history.TryGetValue(jobName, out var list)
                ? list.ToList().AsReadOnly()
                : [];
        }
    }

    /// <summary>Returns all executions across all jobs, most recent first.</summary>
    public IReadOnlyList<ArknJobExecution> GetAllHistory()
    {
        lock (_lock)
        {
            return _history.Values
                .SelectMany(l => l)
                .OrderByDescending(e => e.StartedAt)
                .ToList()
                .AsReadOnly();
        }
    }
}
