# Arkn

[![CI](https://github.com/fernando-terra/arkn/actions/workflows/ci.yml/badge.svg)](https://github.com/fernando-terra/arkn/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Arkn.Results.svg)](https://www.nuget.org/packages/Arkn.Results)
[![License: Apache-2.0](https://img.shields.io/badge/License-Apache--2.0-blue.svg)](https://www.apache.org/licenses/LICENSE-2.0)
[![.NET](https://img.shields.io/badge/.NET-9%20%7C%2010-blue.svg)](https://dotnet.microsoft.com/)

**Conventions you can read. Patterns you can enforce.**

Arkn is a composable, zero-dependency .NET framework. Each package delivers one pattern, integrates naturally with the others, and is enforced at compile time.

🌐 [Docs](https://fernando-terra.github.io/arkn) · 📦 [NuGet](https://www.nuget.org/packages/Arkn.Results) · 📖 [Getting Started](https://fernando-terra.github.io/arkn/getting-started)

---

## Why Arkn?

```csharp
// ❌ Before: exceptions as control flow, ambiguous contracts
public async Task<User> GetUserAsync(Guid id)
{
    var user = await _repo.FindAsync(id);
    if (user is null) throw new NotFoundException($"User {id} not found");
    return user;
}

// ✅ After: failures are first-class, visible at the type level
public async Task<Result<User>> GetUserAsync(Guid id)
{
    var user = await _repo.FindAsync(id);
    if (user is null) return UserErrors.NotFound(id);
    return user;
}
```

No hidden exceptions. No ambiguous nulls. Every failure explicit.

---

## Quick Start

```bash
dotnet add package Arkn.Results
```

```csharp
using Arkn.Results;

// Define your errors once
public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User {id} was not found.");

    public static Error InvalidEmail =>
        Error.Validation("User.InvalidEmail", "Email address is not valid.");
}

// Return Result<T> from domain and application methods
public async Task<Result<UserDto>> GetUserAsync(Guid id)
{
    var user = await _repo.FindAsync(id);
    if (user is null) return UserErrors.NotFound(id);
    return new UserDto(user.Id, user.Name, user.Email);
}

// Handle at the boundary — Minimal API example
app.MapGet("/users/{id:guid}", async (Guid id, IUserService svc) =>
{
    var result = await svc.GetUserAsync(id);
    return result.Match(
        onSuccess: dto   => Results.Ok(dto),
        onFailure: error => error.Type switch
        {
            ErrorType.NotFound   => Results.NotFound(new { error.Code, error.Message }),
            ErrorType.Validation => Results.BadRequest(new { error.Code, error.Message }),
            _                    => Results.Problem(error.Message)
        });
});
```

---

## AI-Assisted Development

Arkn ships a **Model Context Protocol server** — the first .NET framework with native MCP support. Claude, Cursor and GitHub Copilot scaffold correct Arkn code on the first try.

```bash
dotnet tool install -g Arkn.MCP
```

Add to your AI client config:

```json
{
  "mcpServers": {
    "arkn": { "command": "arkn-mcp", "args": [] }
  }
}
```

Your AI assistant can then scaffold error groups, generate jobs, validate code against ARK001–ARK008 rules, and search Arkn documentation inline — without hallucinating patterns.

→ See the [MCP Server guide](https://fernando-terra.github.io/arkn/mcp) for all available tools.

---

## Scaffold in Seconds

```bash
# Install templates
dotnet new install Arkn.Templates

# Minimal API with Result<T> wired end-to-end
dotnet new arkn-api -n MyApi

# Background worker with Arkn.Jobs
dotnet new arkn-job -n MyWorker

# Class library with Arkn conventions
dotnet new arkn-lib -n MyLibrary
```

---

## Packages

| Package | What it does | NuGet |
|---|---|---|
| `Arkn.Core` | Domain primitives — `IEntity`, `IValueObject`, `IAggregateRoot` | [![NuGet](https://img.shields.io/nuget/v/Arkn.Core.svg)](https://www.nuget.org/packages/Arkn.Core) |
| `Arkn.Results` | `Result<T>` and `Error` — failures as first-class types, no exceptions | [![NuGet](https://img.shields.io/nuget/v/Arkn.Results.svg)](https://www.nuget.org/packages/Arkn.Results) |
| `Arkn.Http` | Typed HTTP client — `Result<T>` on every call, OAuth2, mTLS, debug logging | [![NuGet](https://img.shields.io/nuget/v/Arkn.Http.svg)](https://www.nuget.org/packages/Arkn.Http) |
| `Arkn.Logging` | Structured logging — ANSI console, rotating file, pluggable sinks | [![NuGet](https://img.shields.io/nuget/v/Arkn.Logging.svg)](https://www.nuget.org/packages/Arkn.Logging) |
| `Arkn.Jobs` | Cron scheduler — retry, timeout, `Result<T>` contract, failure notifications | [![NuGet](https://img.shields.io/nuget/v/Arkn.Jobs.svg)](https://www.nuget.org/packages/Arkn.Jobs) |
| `Arkn.Notifications` | Pluggable notifier — Slack, Discord, Teams, Email — fan-out to N channels | [![NuGet](https://img.shields.io/nuget/v/Arkn.Notifications.svg)](https://www.nuget.org/packages/Arkn.Notifications) |
| `Arkn.Analyzers` | Roslyn analyzers — ARK001–ARK008 enforcing Arkn patterns at compile time | [![NuGet](https://img.shields.io/nuget/v/Arkn.Analyzers.svg)](https://www.nuget.org/packages/Arkn.Analyzers) |
| `Arkn.SourceGen` | Source generator — generates `Error` factories from `[ArknErrors]` partial classes | [![NuGet](https://img.shields.io/nuget/v/Arkn.SourceGen.svg)](https://www.nuget.org/packages/Arkn.SourceGen) |
| `Arkn.Templates` | `dotnet new` templates — `arkn-api`, `arkn-job`, `arkn-lib` | [![NuGet](https://img.shields.io/nuget/v/Arkn.Templates.svg)](https://www.nuget.org/packages/Arkn.Templates) |
| `Arkn.MCP` ✨ | MCP Server (`dotnet tool`) — scaffold + validate tools for AI assistants | [![NuGet](https://img.shields.io/nuget/v/Arkn.MCP.svg)](https://www.nuget.org/packages/Arkn.MCP) |

<details>
<summary>Extensions</summary>

| Package | What it does | NuGet |
|---|---|---|
| `Arkn.Extensions.Logging.ApplicationInsights` | Application Insights sink for `Arkn.Logging` | [![NuGet](https://img.shields.io/nuget/v/Arkn.Extensions.Logging.ApplicationInsights.svg)](https://www.nuget.org/packages/Arkn.Extensions.Logging.ApplicationInsights) |
| `Arkn.Extensions.Logging.Seq` | Seq sink via HTTP + CLEF — zero Serilog dependency | [![NuGet](https://img.shields.io/nuget/v/Arkn.Extensions.Logging.Seq.svg)](https://www.nuget.org/packages/Arkn.Extensions.Logging.Seq) |
| `Arkn.Extensions.Logging.Elasticsearch` | Elasticsearch Bulk API sink — zero NEST dependency | [![NuGet](https://img.shields.io/nuget/v/Arkn.Extensions.Logging.Elasticsearch.svg)](https://www.nuget.org/packages/Arkn.Extensions.Logging.Elasticsearch) |
| `Arkn.Extensions.Notifications.Slack` | Slack via Incoming Webhook + Block Kit | [![NuGet](https://img.shields.io/nuget/v/Arkn.Extensions.Notifications.Slack.svg)](https://www.nuget.org/packages/Arkn.Extensions.Notifications.Slack) |
| `Arkn.Extensions.Notifications.Discord` | Discord Webhook Embeds | [![NuGet](https://img.shields.io/nuget/v/Arkn.Extensions.Notifications.Discord.svg)](https://www.nuget.org/packages/Arkn.Extensions.Notifications.Discord) |
| `Arkn.Extensions.Notifications.Teams` | Microsoft Teams Adaptive Cards | [![NuGet](https://img.shields.io/nuget/v/Arkn.Extensions.Notifications.Teams.svg)](https://www.nuget.org/packages/Arkn.Extensions.Notifications.Teams) |
| `Arkn.Extensions.Notifications.Email` | SMTP + SendGrid email notifier | [![NuGet](https://img.shields.io/nuget/v/Arkn.Extensions.Notifications.Email.svg)](https://www.nuget.org/packages/Arkn.Extensions.Notifications.Email) |

</details>

---

## Philosophy

- **Zero lock-in** — `Arkn.Core` and `Arkn.Results` have no external NuGet dependencies
- **Composability** — each package is independently useful; combine only what you need
- **Explicit over magic** — no hidden behaviors, no ambient context, no surprising conventions
- **Failures are first-class** — `Result<T>` makes every failure visible at the type level
- **AI-native** — MCP server ensures AI assistants generate correct code, not hallucinations

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
