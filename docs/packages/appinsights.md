# Arkn.Extensions.Logging.ApplicationInsights

Azure Application Insights sink for Arkn.Logging. Sends structured `LogEntry` records as Application Insights traces and exceptions.

```bash
dotnet add package Arkn.Extensions.Logging.ApplicationInsights
```

## Setup

```csharp
builder.Services.AddArknLogging(logging =>
{
    logging.AddConsoleSink();

    logging.AddApplicationInsightsSink(ai =>
    {
        ai.ConnectionString = config["ApplicationInsights:ConnectionString"];
        ai.MinimumLevel     = ArknLogLevel.Warning;
    });
});
```

## Level mapping

| `ArknLogLevel` | Application Insights severity |
|---|---|
| Trace | Verbose |
| Debug | Verbose |
| Info | Information |
| Warning | Warning |
| Error | Error |
| Fatal | Critical |

## Context and scope

`LogEntry.Context` key-value pairs are forwarded as **custom properties** on the telemetry item, visible in the Application Insights portal under *Custom Properties*.

`LogEntry.Scope` is forwarded as the `Scope` custom property, making it easy to filter logs by `RunId` from an `Arkn.Jobs` execution.

## Exceptions

When a `LogEntry` carries an `Exception`, it is forwarded as an `ExceptionTelemetry` item in addition to the trace, enabling full stack trace visibility in the Failures blade.
