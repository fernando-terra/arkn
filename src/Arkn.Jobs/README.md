# Arkn.Jobs

Zero-dependency cron scheduler with retry, timeout, distributed lock, persistence, and DLQ — wired into `Arkn.Notifications`.

## Install

```bash
dotnet add package Arkn.Jobs
```

## Setup

```csharp
builder.Services.AddArknJobs(jobs =>
{
    jobs.Add<InvoiceProcessorJob>("0 2 * * *")
        .WithName("invoice-processor")
        .WithDescription("Processes pending invoices at 02:00")
        .WithTimeout(TimeSpan.FromMinutes(10))
        .WithRetry(maxAttempts: 3)
        .NotifyOn(JobEvent.Failed | JobEvent.TimedOut);

    // Optional: in-memory history, DLQ, distributed lock
    jobs.WithInMemoryHistory(capacity: 200)
        .WithInMemoryDlq()
        .WithDistributedLock<MyRedisLock>()
        .OnFailure<SlackNotifier>();  // global fallback notifier
});
```

## Implementing a job

```csharp
public sealed class InvoiceProcessorJob(IInvoiceService invoices) : IArknJob
{
    public async Task<Result> ExecuteAsync(ArknJobContext ctx)
    {
        ctx.Log("Starting invoice processing batch");

        var result = await invoices.ProcessPendingAsync(ctx.CancellationToken);

        return result.Match(
            onSuccess: count => { ctx.Log($"Processed {count} invoices"); return Result.Success(); },
            onFailure: error => Result.Failure(error));
    }
}
```

## Persistence — `IJobHistoryStore`

Stores job execution records externally (database, blob, etc.). Default is in-memory:

```csharp
jobs.WithInMemoryHistory(capacity: 100);   // circular buffer, 100 records per job
jobs.WithHistoryStore<MyEfCoreHistoryStore>(); // custom implementation
```

Query via `IJobHistoryStore.GetRecentAsync(jobName, limit)` or the scheduler:

```csharp
app.MapGet("/jobs/history", (IArknJobScheduler scheduler) =>
    Results.Ok(scheduler.GetAllHistory()));
```

## Distributed Lock — `IDistributedJobLock`

Prevents concurrent execution across multiple instances. Default (`NoOpDistributedJobLock`) always acquires — suitable for single-instance deployments:

```csharp
jobs.WithDistributedLock<RedisDistributedJobLock>(); // plug any IDistributedJobLock impl
```

```csharp
public sealed class RedisDistributedJobLock(IConnectionMultiplexer redis) : IDistributedJobLock
{
    public async Task<bool> TryAcquireAsync(string jobName, TimeSpan expiry, CancellationToken ct)
        => await redis.GetDatabase().LockTakeAsync(jobName, Environment.MachineName, expiry);

    public async Task ReleaseAsync(string jobName, CancellationToken ct)
        => await redis.GetDatabase().LockReleaseAsync(jobName, Environment.MachineName);
}
```

## Dead-Letter Queue — `IJobDlq`

Jobs that exhaust all retry attempts are enqueued in the DLQ. Default is in-memory:

```csharp
jobs.WithInMemoryDlq();
```

Inspect and drain the DLQ:

```csharp
app.MapGet("/jobs/dlq", (InMemoryJobDlq dlq) =>
    dlq.GetEntriesAsync());

app.MapDelete("/jobs/dlq/{jobName}", (string jobName, InMemoryJobDlq dlq) =>
    dlq.ClearAsync(jobName));
```

## Cron expression support

| Expression | Meaning |
|---|---|
| `* * * * *` | Every minute |
| `0 2 * * *` | Daily at 02:00 |
| `0 8 * * 1` | Every Monday at 08:00 |
| `*/5 * * * *` | Every 5 minutes |
| `0 9,17 * * 1-5` | Weekdays at 09:00 and 17:00 |

## Part of the Arkn ecosystem

[github.com/fernando-terra/arkn](https://github.com/fernando-terra/arkn) · [nuget.org/packages/Arkn.Jobs](https://www.nuget.org/packages/Arkn.Jobs)
