# Arkn

[![CI](https://github.com/fernando-terra/arkn/actions/workflows/ci.yml/badge.svg)](https://github.com/fernando-terra/arkn/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Arkn.Results.svg)](https://www.nuget.org/packages/Arkn.Results)
[![License: Apache-2.0](https://img.shields.io/badge/License-Apache--2.0-blue.svg)](https://www.apache.org/licenses/LICENSE-2.0)
[![.NET](https://img.shields.io/badge/.NET-9%20%7C%2010-blue.svg)](https://dotnet.microsoft.com/)

**Conventions you can read. Patterns you can enforce.**

Arkn is a composable, zero-dependency .NET framework. Each package delivers one pattern, depends only on what it needs, and integrates naturally with the others.

🌐 [Landing page](https://fernando-terra.github.io/arkn) · 📦 [NuGet](https://www.nuget.org/packages/Arkn.Results) · 📖 [Docs](https://fernando-terra.github.io/arkn/getting-started)

---

## Packages

| Package | What it does | NuGet |
|---|---|---|
| `Arkn.Core` | Domain primitives — `IEntity`, `IValueObject`, `IAggregateRoot` | [![NuGet](https://img.shields.io/nuget/v/Arkn.Core.svg)](https://www.nuget.org/packages/Arkn.Core) |
| `Arkn.Results` | `Result<T>` and `Error` — failures as first-class types, no exceptions | [![NuGet](https://img.shields.io/nuget/v/Arkn.Results.svg)](https://www.nuget.org/packages/Arkn.Results) |
| `Arkn.Http` | Typed HTTP client — `Result<T>` on every call, OAuth2, mTLS, debug logging | [![NuGet](https://img.shields.io/nuget/v/Arkn.Http.svg)](https://www.nuget.org/packages/Arkn.Http) |
| `Arkn.Logging` | Structured logging — ANSI console, rotating file, pluggable sinks | [![NuGet](https://img.shields.io/nuget/v/Arkn.Logging.svg)](https://www.nuget.org/packages/Arkn.Logging) |
| `Arkn.Jobs` | Cron scheduler — retry, timeout, `Result<T>` contract, failure notifications | [![NuGet](https://img.shields.io/nuget/v/Arkn.Jobs.svg)](https://www.nuget.org/packages/Arkn.Jobs) |
| `Arkn.Notifications` | Pluggable notifier — Slack, Email (SMTP + SendGrid), fan-out to N channels | [![NuGet](https://img.shields.io/nuget/v/Arkn.Notifications.svg)](https://www.nuget.org/packages/Arkn.Notifications) |
| `Arkn.Analyzers` | Roslyn analyzers — ARK001–ARK004 enforcing Arkn patterns at compile time | [![NuGet](https://img.shields.io/nuget/v/Arkn.Analyzers.svg)](https://www.nuget.org/packages/Arkn.Analyzers) |
| `Arkn.SourceGen` | Source generator — generates `Error` factories from `[ArknErrors]` partial classes | [![NuGet](https://img.shields.io/nuget/v/Arkn.SourceGen.svg)](https://www.nuget.org/packages/Arkn.SourceGen) |
| `Arkn.Templates` | `dotnet new` templates — `arkn-api`, `arkn-job`, `arkn-lib` | [![NuGet](https://img.shields.io/nuget/v/Arkn.Templates.svg)](https://www.nuget.org/packages/Arkn.Templates) |

### Extensions

| Package | What it does |
|---|---|
| `Arkn.Extensions.Logging.ApplicationInsights` | Application Insights sink for `Arkn.Logging` |
| `Arkn.Extensions.Logging.Seq` | Seq sink via HTTP + CLEF — zero Serilog dependency (.NET 10+) |
| `Arkn.Extensions.Logging.Elasticsearch` | Elasticsearch Bulk API sink — zero NEST dependency (.NET 10+) |
| `Arkn.Extensions.Notifications.Slack` | Slack via Incoming Webhook + Block Kit |
| `Arkn.Extensions.Notifications.Email` | SMTP + SendGrid email notifier |

---

## Getting Started

For examples, recipes, and full API reference → **[arkn docs](https://fernando-terra.github.io/arkn/getting-started)**

---

## Philosophy

- **Zero lock-in** — `Arkn.Core` and `Arkn.Results` have no external NuGet dependencies
- **Composability** — each package is independently useful; combine only what you need
- **Explicit over magic** — no hidden behaviors, no ambient context, no surprising conventions
- **Failures are first-class** — `Result<T>` makes every failure visible at the type level

---

## Running Tests

```bash
dotnet test
# 0 failures — ubuntu-latest + windows-latest on every push
```

## Contributing

PRs welcome — open an issue first for non-trivial changes. See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

Apache 2.0 © [Fernando Terra](https://github.com/fernando-terra)
