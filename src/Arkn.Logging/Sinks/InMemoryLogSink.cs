using Arkn.Logging.Abstractions;
using Arkn.Logging.Models;

namespace Arkn.Logging.Sinks;

/// <summary>
/// Thread-safe in-memory log sink with optional scope isolation.
/// Critical for <c>Arkn.Jobs</c>: each job run creates its own scope (RunId),
/// and the dashboard reads only that scope's entries.
/// </summary>
public sealed class InMemoryLogSink : IArknLogSink
{
    private readonly List<LogEntry> _entries = [];
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly int _maxEntries;

    /// <param name="maxEntries">Maximum entries retained across all scopes. Oldest are evicted first.</param>
    public InMemoryLogSink(int maxEntries = 10_000)
    {
        _maxEntries = maxEntries;
    }

    /// <inheritdoc />
    public void Write(LogEntry entry)
    {
        _lock.EnterWriteLock();
        try
        {
            _entries.Add(entry);
            // Evict oldest when over capacity
            if (_entries.Count > _maxEntries)
                _entries.RemoveRange(0, _entries.Count - _maxEntries);
        }
        finally { _lock.ExitWriteLock(); }
    }

    /// <summary>
    /// Returns all entries, optionally filtered by scope.
    /// </summary>
    /// <param name="scope">
    /// When provided, returns only entries whose <see cref="LogEntry.Scope"/> matches.
    /// When <c>null</c>, returns all entries regardless of scope.
    /// </param>
    public IReadOnlyList<LogEntry> GetEntries(string? scope = null)
    {
        _lock.EnterReadLock();
        try
        {
            return scope is null
                ? _entries.ToList().AsReadOnly()
                : _entries.Where(e => e.Scope == scope).ToList().AsReadOnly();
        }
        finally { _lock.ExitReadLock(); }
    }

    /// <summary>
    /// Clears entries, optionally limited to a specific scope.
    /// </summary>
    /// <param name="scope">When <c>null</c>, clears all entries.</param>
    public void Clear(string? scope = null)
    {
        _lock.EnterWriteLock();
        try
        {
            if (scope is null)
                _entries.Clear();
            else
                _entries.RemoveAll(e => e.Scope == scope);
        }
        finally { _lock.ExitWriteLock(); }
    }

    /// <summary>Total number of retained entries across all scopes.</summary>
    public int Count
    {
        get
        {
            _lock.EnterReadLock();
            try { return _entries.Count; }
            finally { _lock.ExitReadLock(); }
        }
    }
}
