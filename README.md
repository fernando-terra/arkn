# Arkn Framework

[![CI](https://github.com/fernando-terra/arkn/actions/workflows/ci.yml/badge.svg)](https://github.com/fernando-terra/arkn/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Arkn.Core.svg)](https://www.nuget.org/packages/Arkn.Core)
[![License: Apache-2.0](https://img.shields.io/badge/License-Apache--2.0-blue.svg)](https://www.apache.org/licenses/LICENSE-2.0)
[![.NET](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-512BD4.svg)](https://dotnet.microsoft.com/)

**Conventions you can read. Patterns you can enforce.**

Arkn is a composable, zero-dependency .NET framework tailored for Clean Architecture and Domain-Driven Design (DDD). Each package delivers one pattern, integrates naturally with the others, and is enforced at compile time via Roslyn Analyzers.

🌐 [Docs](https://fernando-terra.github.io/arkn) · 📦 [NuGet](https://www.nuget.org/packages/Arkn.Core) · 📖 [Getting Started](https://fernando-terra.github.io/arkn/getting-started)

---

## Why create another framework?

The .NET ecosystem is full of powerful tools, but they often come with heavy costs: **Vendor Lock-in**, **Boilerplate**, and **Hidden Control Flow**. 
Arkn was built from the ground up to solve these architectural pains:

1. **Zero Lock-in in the Core:** `Arkn.Core` and `Arkn.Results` have **zero** external NuGet dependencies. Your domain layer remains completely pure. No Entity Framework, no MediatR, no third-party libraries polluting your core logic.
2. **Failures as First-Class Citizens:** Exceptions should be for exceptional circumstances, not control flow. With `Result<T>`, failures are explicit and visible at the type level.
3. **Explicit over Magic:** No hidden behaviors, no ambient contexts, no surprising conventions. What you see is what executes.
4. **Composability:** Every package is an isolated piece of the puzzle. Need just the Result pattern? Use `Arkn.Results`. Need scheduling? Use `Arkn.Jobs`. You are never forced to adopt the entire ecosystem.

---

## Quick Start: The Arkn Way

Stop using exceptions to handle business logic. 

```bash
dotnet add package Arkn.Results
```

```csharp
using Arkn.Results;

// 1. Define your domain errors explicitly
public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User {id} was not found.");

    public static Error InvalidEmail =>
        Error.Validation("User.InvalidEmail", "Email address is not valid.");
}

// 2. Return Result<T> from your Application layer
public async Task<Result<UserDto>> GetUserAsync(Guid id)
{
    var user = await _repo.FindAsync(id);
    
    if (user is null) 
        return UserErrors.NotFound(id);
        
    return new UserDto(user.Id, user.Name, user.Email);
}

// 3. Handle it elegantly at the boundary (Minimal APIs)
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

## AI-Assisted Development (MCP)

Arkn is the first .NET framework with native **Model Context Protocol (MCP)** support. AI assistants like Claude, Cursor, and GitHub Copilot can scaffold correct Arkn code on the first try.

```bash
dotnet tool install -g Arkn.MCP
```

By hooking the MCP server into your AI client, the assistant will automatically generate jobs, errors, and domain structures following the `ARK001`–`ARK008` rules without hallucinating third-party patterns.

→ See the [MCP Server guide](https://fernando-terra.github.io/arkn/mcp) for setup instructions.

---

## The Ecosystem

| Package | Purpose | NuGet |
|---|---|---|
| `Arkn.Core` | DDD primitives (`IEntity`, `IValueObject`, `IAggregateRoot`). **Zero dependencies.** | [![NuGet](https://img.shields.io/nuget/v/Arkn.Core.svg)](https://www.nuget.org/packages/Arkn.Core) |
| `Arkn.Results` | `Result<T>` and `Error` — failures as explicit types. **Zero dependencies.** | [![NuGet](https://img.shields.io/nuget/v/Arkn.Results.svg)](https://www.nuget.org/packages/Arkn.Results) |
| `Arkn.Http` | Typed HTTP client wrapping `Result<T>` on every call, with OAuth2 and mTLS support. | [![NuGet](https://img.shields.io/nuget/v/Arkn.Http.svg)](https://www.nuget.org/packages/Arkn.Http) |
| `Arkn.Jobs` | Cron scheduler with built-in retry, timeout, and `Result<T>` contracts. | [![NuGet](https://img.shields.io/nuget/v/Arkn.Jobs.svg)](https://www.nuget.org/packages/Arkn.Jobs) |
| `Arkn.Logging` | Structured logging (ANSI console, rotating file, pluggable sinks). | [![NuGet](https://img.shields.io/nuget/v/Arkn.Logging.svg)](https://www.nuget.org/packages/Arkn.Logging) |
| `Arkn.Notifications` | Pluggable fan-out notifier for Slack, Discord, Teams, and Email. | [![NuGet](https://img.shields.io/nuget/v/Arkn.Notifications.svg)](https://www.nuget.org/packages/Arkn.Notifications) |
| `Arkn.Analyzers` | Roslyn analyzers enforcing Arkn architectural patterns at compile time. | [![NuGet](https://img.shields.io/nuget/v/Arkn.Analyzers.svg)](https://www.nuget.org/packages/Arkn.Analyzers) |
| `Arkn.SourceGen` | Source generator for `Error` factories via `[ArknErrors]`. | [![NuGet](https://img.shields.io/nuget/v/Arkn.SourceGen.svg)](https://www.nuget.org/packages/Arkn.SourceGen) |

<details>
<summary><b>View Extensions (Sinks & Channels)</b></summary>

| Package | What it does | NuGet |
|---|---|---|
| `Arkn.Extensions.Logging.ApplicationInsights` | App Insights sink for `Arkn.Logging` | [![NuGet](https://img.shields.io/nuget/v/Arkn.Extensions.Logging.ApplicationInsights.svg)](https://www.nuget.org/packages/Arkn.Extensions.Logging.ApplicationInsights) |
| `Arkn.Extensions.Logging.Seq` | Seq sink via HTTP + CLEF | [![NuGet](https://img.shields.io/nuget/v/Arkn.Extensions.Logging.Seq.svg)](https://www.nuget.org/packages/Arkn.Extensions.Logging.Seq) |
| `Arkn.Extensions.Logging.Elasticsearch` | Elasticsearch Bulk API sink | [![NuGet](https://img.shields.io/nuget/v/Arkn.Extensions.Logging.Elasticsearch.svg)](https://www.nuget.org/packages/Arkn.Extensions.Logging.Elasticsearch) |
| `Arkn.Extensions.Notifications.Slack` | Slack via Incoming Webhook + Block Kit | [![NuGet](https://img.shields.io/nuget/v/Arkn.Extensions.Notifications.Slack.svg)](https://www.nuget.org/packages/Arkn.Extensions.Notifications.Slack) |
| `Arkn.Extensions.Notifications.Discord` | Discord Webhook Embeds | [![NuGet](https://img.shields.io/nuget/v/Arkn.Extensions.Notifications.Discord.svg)](https://www.nuget.org/packages/Arkn.Extensions.Notifications.Discord) |
| `Arkn.Extensions.Notifications.Teams` | Microsoft Teams Adaptive Cards | [![NuGet](https://img.shields.io/nuget/v/Arkn.Extensions.Notifications.Teams.svg)](https://www.nuget.org/packages/Arkn.Extensions.Notifications.Teams) |
| `Arkn.Extensions.Notifications.Email` | SMTP + SendGrid email notifier | [![NuGet](https://img.shields.io/nuget/v/Arkn.Extensions.Notifications.Email.svg)](https://www.nuget.org/packages/Arkn.Extensions.Notifications.Email) |

</details>

---

## Scaffolding

Generate clean code instantly using the `dotnet new` templates:

```bash
# Install templates
dotnet new install Arkn.Templates

# Generate a Minimal API with Result<T> wired end-to-end
dotnet new arkn-api -n MyApi

# Generate a background worker with Arkn.Jobs
dotnet new arkn-job -n MyWorker
```

---

## Contributing

PRs are welcome! Arkn maintains a strict **0-warning** compiler policy. Please ensure your code compiles warning-free and passes all xUnit tests before opening a pull request. See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

Apache 2.0 © [Fernando Terra](https://github.com/fernando-terra)
