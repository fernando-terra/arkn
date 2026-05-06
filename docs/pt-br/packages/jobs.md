# Arkn.Jobs

Agendador cron zero-dependência com retry, timeout, lock distribuído, persistência e DLQ — integrado ao `Arkn.Notifications`.

```bash
dotnet add package Arkn.Jobs
```

## Configuração

```csharp
builder.Services.AddArknJobs(jobs =>
{
    jobs.Add<InvoiceProcessorJob>("0 2 * * *")
        .WithName("invoice-processor")
        .WithDescription("Processes pending invoices at 02:00")
        .WithTimeout(TimeSpan.FromMinutes(10))
        .WithRetry(maxAttempts: 3)
        .NotifyOn(JobEvent.Failed | JobEvent.TimedOut);

    // Extensões opcionais (v0.3.0+)
    jobs.WithInMemoryHistory(capacity: 200)
        .WithInMemoryDlq()
        .WithDistributedLock<MyRedisLock>()
        .OnFailure<SlackNotifier>();
});
```

## Implementando um job

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

## Persistência — `IJobHistoryStore` <Badge type="tip" text="v0.3.0" />

Armazena registros de execução de jobs para auditoria, dashboards ou replay. O padrão é em memória:

```csharp
jobs.WithInMemoryHistory(capacity: 100);        // buffer circular, 100 registros/job
jobs.WithHistoryStore<MyEfCoreHistoryStore>();   // plugue um IJobHistoryStore customizado
```

Consulte via endpoint do scheduler:

```csharp
app.MapGet("/jobs/history", (IArknJobScheduler scheduler) =>
    Results.Ok(scheduler.GetAllHistory()));

// Ou injete IJobHistoryStore diretamente
app.MapGet("/jobs/{name}/history", async (string name, IJobHistoryStore store) =>
    Results.Ok(await store.GetRecentAsync(name, limit: 20)));
```

## Lock Distribuído — `IDistributedJobLock` <Badge type="tip" text="v0.3.0" />

Previne execução concorrente entre múltiplas réplicas. O padrão (`NoOpDistributedJobLock`) sempre adquire — seguro para instância única out of the box:

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

Jobs que esgotam todas as tentativas de retry vão para a DLQ. O padrão é em memória:

```csharp
jobs.WithInMemoryDlq();
```

```csharp
// Inspecione a DLQ
app.MapGet("/jobs/dlq", (InMemoryJobDlq dlq) => dlq.GetEntriesAsync());

// Limpe por nome de job
app.MapDelete("/jobs/dlq/{name}", (string name, InMemoryJobDlq dlq) =>
    dlq.ClearAsync(name));
```

## Suporte a expressões cron

| Expressão | Significado |
|---|---|
| `* * * * *` | Todo minuto |
| `0 2 * * *` | Diariamente às 02:00 |
| `0 8 * * 1` | Toda segunda-feira às 08:00 |
| `*/5 * * * *` | A cada 5 minutos |
| `0 9,17 * * 1-5` | Dias úteis às 09:00 e 17:00 |

## Histórico de execuções

```csharp
app.MapGet("/jobs/history", (IArknJobScheduler scheduler) =>
    Results.Ok(scheduler.GetAllHistory()));
```
