# Arkn.Logging

Structured, sink-pluggable logging. **Zero external dependencies.**

```bash
dotnet add package Arkn.Logging
```

## Setup

```csharp
builder.Services.AddArknLogging(logging =>
{
    logging.SetMinimumLevel(ArknLogLevel.Info);
    logging.AddConsoleSink();
    logging.AddFileSink("logs/app.log");
    logging.AddInMemorySink();    // used by Arkn.Jobs for per-run logs
});
```

## Usage

```csharp
public class MyService(IArknLogger logger)
{
    public void DoWork()
    {
        var ctx = ArknLogContext.ForScope("operation-123")
            .With("UserId", 42)
            .With("Action", "ProcessOrder");

        logger.Info("Starting work", ctx);
        logger.Warning("Slow query detected", ctx);
        logger.Error("Operation failed", ex, ctx);
    }
}
```

## Available sinks

| Package | Sink | Notes |
|---|---|---|
| `Arkn.Logging` | `ConsoleLogSink` | Colorized by level |
| `Arkn.Logging` | `FileSink` | Append, auto-flush |
| `Arkn.Logging` | `InMemoryLogSink` | Scope-isolated, used by Jobs |
| `Arkn.Extensions.Logging.Seq` | `SeqSink` | CLEF via HTTP, zero Serilog |
| `Arkn.Extensions.Logging.Elasticsearch` | `ElasticsearchSink` | Bulk API, zero NEST |
