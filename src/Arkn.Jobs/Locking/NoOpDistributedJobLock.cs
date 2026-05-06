namespace Arkn.Jobs.Locking;

/// <summary>
/// No-op lock that always acquires successfully.
/// Default behavior — suitable for single-instance deployments.
/// Replace with a real implementation (e.g., Redis) for multi-instance scenarios.
/// </summary>
public sealed class NoOpDistributedJobLock : IDistributedJobLock
{
    public Task<bool> TryAcquireAsync(string jobName, TimeSpan expiry, CancellationToken ct = default) =>
        Task.FromResult(true);

    public Task ReleaseAsync(string jobName, CancellationToken ct = default) =>
        Task.CompletedTask;
}
