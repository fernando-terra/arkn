# Arkn.Analyzers

> **Conventions you can read. Patterns you can enforce.**

Roslyn analyzers that enforce Arkn patterns at compile time. Part of Arkn.Copilot — making your codebase ready for AI-assisted development.

## Install

```bash
dotnet add package Arkn.Analyzers
```

> This package is a development dependency — it doesn't ship with your application.

## Rules

| ID | Description | Severity |
|----|-------------|----------|
| **ARK001** | Domain methods must return `Result` or `Result<T>` — not throw | Warning |
| **ARK002** | Error codes must follow `Namespace.Reason` pattern (e.g. `User.NotFound`) | Warning |
| **ARK003** | `Result` / `Result<T>` must not be silently discarded | Warning |
| **ARK004** | `IArknJob.ExecuteAsync` must return `Task<Result>` or `Task<Result<T>>` | Error |
| **ARK005** | Avoid using raw `HttpClient` — extend `ArknHttpClient` instead | Warning |
| **ARK006** | Prefer `IArknLogger` over MEL `ILogger` or `Console` in Arkn components | Warning |
| **ARK007** | Domain methods must not use `throw new` — return `Result.Failure` instead | Warning |
| **ARK008** | `catch` blocks must not swallow exceptions silently | Warning |

ARK001 includes a **code fix** that wraps the return type in `Result<T>` automatically.

## ARK005 — Avoid raw HttpClient

```csharp
// ❌ ARK005
var client = new HttpClient();
client.GetAsync("https://api.example.com");

// ✅ Use typed client via AddArknHttp
builder.Services.AddArknHttp<PaymentClient>("https://api.pay.com");
```

## ARK006 — Prefer IArknLogger

```csharp
// ❌ ARK006
public class MyService(ILogger<MyService> logger) { ... }
// ❌ Also triggers
Console.WriteLine("something happened");

// ✅ Use IArknLogger for structured, sink-routed logging
public class MyService(IArknLogger logger) { ... }
```

## Copilot-ready

Also ships with IDE instruction files in the Arkn repository:
- `.github/copilot-instructions.md` — GitHub Copilot
- `.cursor/rules/arkn.mdc` — Cursor
- `CLAUDE.md` — Claude Code and AI agents

## Suppressing a rule

```csharp
#pragma warning disable ARK005
var client = new HttpClient(); // legacy code, tracked in #1234
#pragma warning restore ARK005
```

## Part of the Arkn ecosystem

[github.com/fernando-terra/arkn](https://github.com/fernando-terra/arkn) · [nuget.org/packages/Arkn.Analyzers](https://www.nuget.org/packages/Arkn.Analyzers)
