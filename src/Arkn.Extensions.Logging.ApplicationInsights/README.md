# Arkn.Extensions.Logging.ApplicationInsights

[![NuGet](https://img.shields.io/nuget/v/Arkn.Extensions.Logging.ApplicationInsights)](https://www.nuget.org/packages/Arkn.Extensions.Logging.ApplicationInsights)

Zero-config [Application Insights](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview) sink for [Arkn.Logging](../Arkn.Logging/README.md).

Forwards structured log entries to Azure Application Insights as **TraceTelemetry** or **ExceptionTelemetry**, with full context propagation.

---

## Installation

```bash
dotnet add package Arkn.Extensions.Logging.ApplicationInsights
```

Requires .NET 9.0 or .NET 10.0.

---

## Quick Start

```csharp
using Arkn.Logging.Extensions;
using Arkn.Logging.Models;
using Arkn.Extensions.Logging.ApplicationInsights.Extensions;

builder.Services.AddArknLogging(log =>
{
    log.SetMinimumLevel(ArknLogLevel.Info)
       .AddConsoleSink()
       .AddApplicationInsights(ai =>
       {
           ai.ConnectionString = "InstrumentationKey=...;IngestionEndpoint=...";
           ai.MinimumLevel     = ArknLogLevel.Warning;   // only Warning+ goes to AI
           ai.RoleName         = "MyService";
       });
});
```

### Zero-config (connection string from environment)

When `ConnectionString` is left null, the Application Insights SDK automatically reads the `APPLICATIONINSIGHTS_CONNECTION_STRING` environment variable — no code change needed between environments.

```csharp
log.AddApplicationInsights(_ => { });   // reads from env var
```

---

## Telemetry Mapping

| Arkn level | AI SeverityLevel  | Telemetry type        |
|------------|-------------------|-----------------------|
| Trace      | Verbose           | TraceTelemetry        |
| Debug      | Verbose           | TraceTelemetry        |
| Info       | Information       | TraceTelemetry        |
| Warning    | Warning           | TraceTelemetry        |
| Error      | Error             | ExceptionTelemetry*   |
| Fatal      | Critical          | ExceptionTelemetry*   |

\* When `LogEntry.Exception` is not null, `ExceptionTelemetry` is sent; otherwise `TraceTelemetry`.

---

## Context & Scope

All `LogEntry.Context` key-value pairs are attached as **custom dimensions** on every telemetry item. The `LogEntry.Scope` string is forwarded as the `scope` custom dimension.

```csharp
logger.Info("Payment processed", scope: "billing", context: new()
{
    ["orderId"] = orderId,
    ["amount"]  = amount,
});
// → TraceTelemetry with customDimensions: { scope: "billing", orderId: "...", amount: "..." }
```

---

## Options Reference

| Property        | Type           | Default   | Description                                                   |
|-----------------|----------------|-----------|---------------------------------------------------------------|
| `ConnectionString` | `string?`   | `null`    | AI connection string. Falls back to env var when null.       |
| `MinimumLevel`  | `ArknLogLevel` | `Info`    | Entries below this level are dropped before sending.         |
| `RoleName`      | `string`       | `"Arkn"`  | `cloud_RoleName` tag on all telemetry items.                 |

---

## License

MIT — see repository root for details.
