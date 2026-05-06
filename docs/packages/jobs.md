# Arkn.Jobs

Zero-dependency cron scheduler with retry, timeout, distributed lock, persistence, and DLQ — wired into `Arkn.Notifications`.

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

    // Optional extensions (v0.3.0+)
    jobs.WithInMemoryHistory(capacity: 200)
        .WithInMemoryDlq()
        .WithDistributedLock<MyRedisLock>()
        .OnFailure<SlackNotifier>();
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

## Persistence — `IJobHistoryStore` <Badge type="tip" text="v0.3.0" />

Stores job execution records for audit, dashboards, or replay. Default is in-memory:

```csharp
jobs.WithInMemoryHistory(capacity: 100);        // circular buffer, 100 records/job
jobs.WithHistoryStore<MyEfCoreHistoryStore>();   // plug custom IJobHistoryStore
```

Query via the scheduler endpoint:

```csharp
app.MapGet("/jobs/history", (IArknJobScheduler scheduler) =>
    Results.Ok(scheduler.GetAllHistory()));

// Or inject IJobHistoryStore directly
app.MapGet("/jobs/{name}/history", async (string name, IJobHistoryStore store) =>
    Results.Ok(await store.GetRecentAsync(name, limit: 20)));
```

## Distributed Lock — `IDistributedJobLock` <Badge type="tip" text="v0.3.0" />

Prevents concurrent execution across multiple replicas. Default (`NoOpDistributedJobLock`) always acquires — single-instance safe out of the box:

```csharp
jobs.WithDistributedLock<RedisDistributedJobLock>();
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

## Dead-Letter Queue — `IJobDlq` <Badge type="tip" text="v0.3.0" />

Jobs that exhaust all retry attempts land in the DLQ. Default is in-memory:

```csharp
jobs.WithInMemoryDlq();
```

```csharp
// Inspect the DLQ
app.MapGet("/jobs/dlq", (InMemoryJobDlq dlq) => dlq.GetEntriesAsync());

// Drain by job name
app.MapDelete("/jobs/dlq/{name}", (string name, InMemoryJobDlq dlq) =>
    dlq.ClearAsync(name));
```

## Cron expression support

| Expression | Meaning |
|---|---|
| `* * * * *` | Every minute |
| `0 2 * * *` | Daily at 02:00 |
| `0 8 * * 1` | Every Monday at 08:00 |
| `*/5 * * * *` | Every 5 minutes |
| `0 9,17 * * 1-5` | Weekdays at 09:00 and 17:00 |

## Execution history

```csharp
app.MapGet("/jobs/history", (IArknJobScheduler scheduler) =>
    Results.Ok(scheduler.GetAllHistory()));
```
