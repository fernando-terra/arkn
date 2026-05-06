# Arkn.Extensions.Logging.Seq

Seq sink for Arkn.Logging — sends structured logs via HTTP using **CLEF** (Compact Log Event Format / JSON Lines). **Zero Serilog dependency.**

```bash
dotnet add package Arkn.Extensions.Logging.Seq
```

## Setup

```csharp
builder.Services.AddArknLogging(logging =>
{
    logging.AddConsoleSink();

    logging.AddSeqSink(seq =>
    {
        seq.ServerUrl            = "http://localhost:5341";
        seq.ApiKey               = config["Seq:ApiKey"];     // optional
        seq.MinimumLevel         = ArknLogLevel.Info;
        seq.BatchSize            = 50;                       // flush after N entries
        seq.FlushIntervalSeconds = 5;                        // or every 5 seconds
        seq.TimeoutSeconds       = 10;
    });
});
```

## How it works

Each `LogEntry` is serialized as a **CLEF** line (`application/vnd.serilog.clef`) and POSTed to `/api/events/raw?clef`.

CLEF field mapping:

| Arkn field | CLEF field |
|---|---|
| `Timestamp` | `@t` (ISO 8601) |
| `Message` | `@mt` |
| `Level` | `@l` (Verbose/Debug/Information/Warning/Error/Fatal) |
| `Exception` | `@x` |
| `Scope` | `Scope` (custom property) |
| `Context.*` | flat properties |

## Batching

Entries are buffered in memory and flushed:
- When the buffer reaches `BatchSize` (default: 50)
- Every `FlushIntervalSeconds` seconds (default: 5)
- On `Dispose()` (remaining entries are flushed synchronously)

Sink failures are swallowed — a Seq outage will not crash your application.

## Seq level mapping

| `ArknLogLevel` | Seq level |
|---|---|
| Trace | Verbose |
| Debug | Debug |
| Info | Information |
| Warning | Warning |
| Error | Error |
| Fatal | Fatal |
