# Arkn.Logging

> **Conventions you can read. Patterns you can enforce.**

Structured logging with pluggable sinks and Microsoft.Extensions.Logging bridge.

## Install

```bash
dotnet add package Arkn.Logging
```

## Quick example

```csharp
// Create logger with sinks
var sink   = new InMemoryLogSink();
var logger = new ArknLogger([sink]);

logger.Log("Invoice processed", LogLevel.Information, new
{
    InvoiceId = invoice.Id,
    Amount    = invoice.Total
});

// Bridge to MEL
builder.Logging.AddArknLogging(options =>
{
    options.AddSink<ConsoleSink>();
});
```

## Part of the Arkn ecosystem

[github.com/fernando-terra/arkn](https://github.com/fernando-terra/arkn) · [nuget.org/packages/Arkn.Logging](https://www.nuget.org/packages/Arkn.Logging)
