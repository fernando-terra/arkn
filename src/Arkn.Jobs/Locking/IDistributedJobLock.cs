namespace Arkn.Jobs.Locking;

/// <summary>
/// Distributed lock abstraction for job scheduling.
/// Prevents concurrent execution across multiple instances.
/// Default: <see cref="NoOpDistributedJobLock"/> (always acquires — single-instance behavior).
/// </summary>
public interface IDistributedJobLock
{
    /// <summary>
    /// Attempts to acquire the lock for <paramref name="jobName"/>.
    /// Returns <c>false</c> if already held — runner should skip this execution.
    /// </summary>
    Task<bool> TryAcquireAsync(string jobName, TimeSpan expiry, CancellationToken ct = default);

    /// <summary>Releases the lock after execution completes.</summary>
    Task ReleaseAsync(string jobName, CancellationToken ct = default);
}
