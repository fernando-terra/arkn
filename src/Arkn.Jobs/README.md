# Arkn.Jobs

> **Conventions you can read. Patterns you can enforce.**

Cron scheduler with retry, timeout, scoped DI per run, and `Result<T>` as the job contract. No Hangfire needed.

## Install

```bash
dotnet add package Arkn.Jobs
```

## Quick example

```csharp
// Define a job — ExecuteAsync MUST return Task<Result> (enforced by ARK004 analyzer)
public class InvoiceProcessorJob : IArknJob
{
    public async Task<Result> ExecuteAsync(CancellationToken ct)
    {
        // your logic here
        return Result.Success();
    }
}

// Register
builder.Services.AddArknJobs(jobs =>
{
    jobs.Add<InvoiceProcessorJob>("0 2 * * *")   // daily at 2 AM
        .WithRetry(maxAttempts: 3)
        .WithTimeout(TimeSpan.FromMinutes(10))
        .NotifyOn(JobEvent.Failed | JobEvent.TimedOut);

    jobs.OnFailure<SlackNotifier>();              // global fallback notifier
});
```

## Part of the Arkn ecosystem

[github.com/fernando-terra/arkn](https://github.com/fernando-terra/arkn) · [nuget.org/packages/Arkn.Jobs](https://www.nuget.org/packages/Arkn.Jobs)
