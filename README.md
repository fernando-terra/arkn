# Arkn

[![CI](https://github.com/fernando-terra/arkn/actions/workflows/ci.yml/badge.svg)](https://github.com/fernando-terra/arkn/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Arkn.Results.svg)](https://www.nuget.org/packages/Arkn.Results)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9%20%7C%2010-blue.svg)](https://dotnet.microsoft.com/)

**Conventions you can read. Patterns you can enforce.**

Arkn is a composable, zero-dependency .NET framework. Each package delivers one pattern, depends only on what it needs, and integrates naturally with the others.

🌐 [Landing page](https://fernando-terra.github.io/arkn) · 📦 [NuGet](https://www.nuget.org/packages/Arkn.Results) · 📝 [Blog series](https://medium.com/)

---

## Packages

| Package | What it does | NuGet |
|---|---|---|
| `Arkn.Core` | Domain primitives — `IEntity`, `IValueObject`, `IAggregateRoot` | [![NuGet](https://img.shields.io/nuget/v/Arkn.Core.svg)](https://www.nuget.org/packages/Arkn.Core) |
| `Arkn.Results` | `Result<T>` and `Error` — failures as first-class types, no exceptions | [![NuGet](https://img.shields.io/nuget/v/Arkn.Results.svg)](https://www.nuget.org/packages/Arkn.Results) |
| `Arkn.Http` | Fluent typed HTTP client — `Result<T>` on every call, auth interceptors, debug logging | [![NuGet](https://img.shields.io/nuget/v/Arkn.Http.svg)](https://www.nuget.org/packages/Arkn.Http) |
| `Arkn.Logging` | Structured logging — ANSI console, rotating file, MEL bridge, pluggable sinks | [![NuGet](https://img.shields.io/nuget/v/Arkn.Logging.svg)](https://www.nuget.org/packages/Arkn.Logging) |
| `Arkn.Jobs` | Cron scheduler — retry, timeout, `Result<T>` contract, notifications on failure | [![NuGet](https://img.shields.io/nuget/v/Arkn.Jobs.svg)](https://www.nuget.org/packages/Arkn.Jobs) |
| `Arkn.Notifications` | Pluggable notifier abstraction — fan-out to N channels, zero deps | [![NuGet](https://img.shields.io/nuget/v/Arkn.Notifications.svg)](https://www.nuget.org/packages/Arkn.Notifications) |
| `Arkn.Extensions.Notifications.Slack` | Slack via Incoming Webhook + Block Kit, zero external SDKs | [![NuGet](https://img.shields.io/nuget/v/Arkn.Extensions.Notifications.Slack.svg)](https://www.nuget.org/packages/Arkn.Extensions.Notifications.Slack) |
| `Arkn.Extensions.Logging.ApplicationInsights` | Application Insights sink for `Arkn.Logging` | [![NuGet](https://img.shields.io/nuget/v/Arkn.Extensions.Logging.ApplicationInsights.svg)](https://www.nuget.org/packages/Arkn.Extensions.Logging.ApplicationInsights) |
| `Arkn.Analyzers` | Roslyn analyzers — ARK001–ARK004 enforcing Arkn patterns at compile time | [![NuGet](https://img.shields.io/nuget/v/Arkn.Analyzers.svg)](https://www.nuget.org/packages/Arkn.Analyzers) |
| `Arkn.Templates` | `dotnet new` templates — `arkn-api`, `arkn-job`, `arkn-lib` | [![NuGet](https://img.shields.io/nuget/v/Arkn.Templates.svg)](https://www.nuget.org/packages/Arkn.Templates) |

---

## Quick Start

```bash
dotnet add package Arkn.Results
```

### Result\<T\> — never throw for business logic

```csharp
// Define errors once per domain
public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User '{id}' not found.");

    public static readonly Error InvalidEmail =
        Error.Validation("User.InvalidEmail"); // message defaults to code
}

// Return results, never throw
public async Task<Result<User>> GetByIdAsync(Guid id)
{
    var user = await _repo.FindAsync(id);
    return user is null ? UserErrors.NotFound(id) : Result.Success(user);
}

// Consume with Match — exhaustive, no nulls
return result.Match(
    onSuccess: user  => Ok(user),
    onFailure: error => error.Type switch
    {
        ErrorType.NotFound   => NotFound(new { error.Code, error.Message }),
        ErrorType.Validation => BadRequest(new { error.Code, error.Message }),
        _                    => Problem(error.Message)
    });
```

### Arkn.Http — typed client with auth and debug logging

```csharp
// Minimal typed client
public sealed class PaymentsClient(IArknHttp http)
    : ArknHttpClient(http, "https://api.payments.example.com")
{
    public Task<Result<Payment>> GetAsync(Guid id)        => GetAs<Payment>("/payments/{id}", id);
    public Task<Result<Payment>> CreateAsync(PaymentRequest r) => PostAs<Payment>("/payments", r);
    public Task<Result>          DeleteAsync(Guid id)     => Delete("/payments/{id}", id);
}

// Registration — OAuth2, retry, and environment-aware debug logging
builder.Services.AddArknHttp<PaymentsClient>("https://api.payments.example.com")
    .WithClientCredentials(opts =>
    {
        opts.TokenUrl     = "https://auth.example.com/token";
        opts.ClientId     = "my-client";
        opts.ClientSecret = config["Auth:Secret"];
    })
    .WithRetry(maxAttempts: 3)
    .WithDebugLogging(env.IsDevelopment()
        ? DebugLoggingOptions.Development   // all at Debug → console
        : DebugLoggingOptions.Production);  // 2xx at Info → AppInsights
```

### Arkn.Logging — structured, sink-pluggable

```csharp
builder.Services.AddArknLogging(logging =>
{
    logging.AddConsoleSink();   // ANSI colors, auto-disabled on redirect

    logging.AddFileSink(new FileSinkOptions
    {
        Directory       = "/var/log/myapp",
        FileNamePattern = "app-{date}.log", // daily rotation
        UseJsonFormat   = true,
    });

    if (!env.IsDevelopment())
        logging.AddApplicationInsights(opts =>
        {
            opts.ConnectionString = config["AI:ConnectionString"];
            opts.MinimumLevel     = ArknLogLevel.Info;
        });
});
```

### Arkn.Jobs — cron, retry, notifications

```csharp
// Job — must return Task<Result> (enforced by ARK004 analyzer)
public class InvoiceProcessorJob(IArknLogger logger) : IArknJob
{
    public async Task<Result> ExecuteAsync(CancellationToken ct)
    {
        logger.Info("Processing invoices...");
        // ...
        return Result.Success();
    }
}

// Registration
builder.Services.AddArknJobs(jobs =>
{
    jobs.Add<InvoiceProcessorJob>("0 2 * * *")
        .WithRetry(maxAttempts: 3)
        .WithTimeout(TimeSpan.FromMinutes(10))
        .NotifyOn(JobEvent.Failed | JobEvent.TimedOut);

    jobs.OnFailure<SlackNotifier>(); // global fallback
});
```

### Arkn.Analyzers — compile-time enforcement

```bash
dotnet add package Arkn.Analyzers
```

| Rule | Description | Severity |
|------|-------------|----------|
| ARK001 | Domain methods must return `Result` or `Result<T>` | Warning |
| ARK002 | Error codes must follow `Namespace.Reason` pattern | Warning |
| ARK003 | `Result` must not be silently discarded | Warning |
| ARK004 | `IArknJob.ExecuteAsync` must return `Task<Result>` | Error |

Also ships with IDE instruction files for **GitHub Copilot**, **Cursor** and **Claude** so AI-generated code follows Arkn conventions on the first attempt.

---

## Arkn.Copilot

AI-ready from the start. The repo ships:

- **`.github/copilot-instructions.md`** — loaded automatically by GitHub Copilot
- **`.cursor/rules/arkn.mdc`** — loaded by Cursor for every `.cs` file
- **`CLAUDE.md`** — context for Claude Code and AI agents

Benchmark infrastructure (`benchmarks/copilot/`) measures how often LLMs generate correct Arkn code with and without instruction files.

---

## Templates

```bash
dotnet new install Arkn.Templates

dotnet new arkn-api -n MyApi       # Minimal API — Results, Http, error→HTTP mapping, CI
dotnet new arkn-job -n MyWorker    # Worker Service — Jobs, Logging, SampleJob
dotnet new arkn-lib -n MyLibrary   # Class Library — Core, Results, Analyzers
```

---

## Philosophy

- **Zero lock-in** — `Arkn.Core` and `Arkn.Results` have no external NuGet dependencies
- **Composability** — each package is independently useful; combine only what you need
- **Explicit over magic** — no hidden behaviors, no ambient context, no conventions that surprise
- **Failures are first-class** — `Result<T>` makes every failure visible at the type level
- **Copilot-ready** — APIs consistent enough for LLMs to generate correct code on the first try

---

## Running Tests

```bash
dotnet test
# 162 tests, 0 failures — ubuntu-latest + windows-latest on every push
```

## Contributing

PRs welcome — open an issue first for non-trivial changes.

## License

MIT © [Fernando Terra](https://github.com/fernando-terra)
