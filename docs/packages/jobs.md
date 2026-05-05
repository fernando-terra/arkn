# Arkn.Jobs

Zero-dependency cron scheduler with retry, timeout and scoped logs per run.

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

    jobs.OnFailure<SlackNotifier>();  // global fallback notifier
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
