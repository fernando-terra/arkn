# Introducing Arkn: A Zero-Dependency .NET Framework Built for Clarity

## Conventions you can read. Patterns you can enforce.

---

*Building with Arkn — Part 1 of 12*

---

You've been there. You start a new .NET project and before you write a single line of business logic, you're already knee-deep in framework decisions. Which ORM? Which HTTP abstraction? Which logging library? Which retry library? Which scheduler? You install NuGet packages that pull in other NuGet packages, and suddenly you have a dependency graph that looks like a bowl of spaghetti and an `appsettings.json` with a hundred keys you don't fully understand.

The code you actually care about — the domain, the rules, the logic that makes your product valuable — gets buried under layers of framework ceremony.

Arkn is built around a different idea: **the framework should earn its place in every line it adds**.

## The Problem with Heavy Frameworks

Modern .NET is extraordinarily powerful. But the ecosystem has evolved in a direction where convenience often trumps clarity. You add a package for HTTP retries and it brings Polly. You add a job scheduler and it brings Quartz with its database tables and clustering configuration. You add structured logging and you're configuring Serilog enrichers before you've written a single domain model.

Each of these choices is individually reasonable. Collectively, they create what I call **framework gravity** — the tendency for the framework to dominate the codebase, making it hard to see where the framework ends and your application begins.

The specific symptoms:

- **Exceptions as control flow.** A missing record in a database causes a `KeyNotFoundException` that propagates up three layers before being caught — or worse, not caught at all.
- **Implicit contracts.** Your service returns `User?`. Is `null` "not found" or "not loaded yet" or "access denied"? Nobody knows until runtime.
- **Lock-in.** You want to swap your HTTP client for a different one and you realize every service in your codebase references `RestSharp` types directly.
- **Verbose boilerplate.** The same retry logic copy-pasted across twelve services because the abstraction never got built.

Arkn is an answer to these symptoms.

## Philosophy: Zero Lock-in, Composability, Explicitness

Arkn is built on three principles that drive every API decision:

**Zero lock-in.** Every Arkn package depends only on other Arkn packages and the .NET BCL. No Polly. No Serilog. No Hangfire. No ORMs. If you want to replace Arkn's HTTP client with something else tomorrow, your domain code is untouched — because your domain code only knows about `Result<T>`, not about `ArknHttp`.

**Composability.** You install only what you need. Using `Arkn.Results` doesn't force you to use `Arkn.Http`. Using `Arkn.Jobs` doesn't force you to use `Arkn.Notifications`. Each package is independently useful and integrates naturally with the others when you choose to combine them.

**Explicitness.** Failures are values, not exceptions. Every operation that can fail returns a `Result<T>`. Every error has a machine-readable code, a human-readable message, and a semantic type. There is no `User?` — there is `Result<User>`, and the compiler forces you to handle both paths.

## A Quick Tour of the Packages

Here's what the Arkn ecosystem looks like today:

| Package | Purpose |
|---|---|
| `Arkn.Core` | Domain primitives: `IEntity`, `IValueObject`, `IAggregateRoot` |
| `Arkn.Results` | `Result<T>`, `Error`, `ErrorType` — the lingua franca of the framework |
| `Arkn.Http` | Fluent typed HTTP client with built-in retry and timeout |
| `Arkn.Jobs` | Cron scheduler with retry, timeout, distributed lock, DLQ, and `Result<T>` contracts |
| `Arkn.Logging` | Structured logging with pluggable sinks and a MEL bridge |
| `Arkn.Notifications` | Pluggable notification abstraction |
| `Arkn.Extensions.Notifications.Slack` | Slack via Incoming Webhook + Block Kit, zero external deps |
| `Arkn.Extensions.Notifications.Email` | SMTP + SendGrid notifier, zero external deps |
| `Arkn.Extensions.Notifications.Teams` | Microsoft Teams via Adaptive Cards 1.4, zero external deps |
| `Arkn.Extensions.Notifications.Discord` | Discord via Embeds + Webhook, zero external deps |
| `Arkn.Extensions.Logging.ApplicationInsights` | Application Insights sink — maps `LogEntry` to telemetry automatically |
| `Arkn.Extensions.Logging.Seq` | Seq sink via CLEF-over-HTTP with batching |
| `Arkn.Extensions.Logging.Elasticsearch` | Elasticsearch sink via Bulk API with date-pattern indices |
| `Arkn.Analyzers` | Roslyn analyzers (ARK001–ARK008) that enforce Arkn patterns at compile time |
| `Arkn.SourceGen` | Source generator — eliminates error factory boilerplate via `[ArknErrors]` |
| `Arkn.MCP` | Native MCP server — lets AI assistants scaffold and validate Arkn code correctly |
| `Arkn.Templates` | `dotnet new` templates for API, Worker and Library projects |

Each package has a clear job. None of them surprise you.

## The Result<T> Teaser

The single idea that unifies everything in Arkn is `Result<T>`. Let me show you the difference it makes.

Here's a traditional service method:

```csharp
// Traditional approach — what does null mean?
public async Task<User?> GetUserAsync(Guid id)
{
    var user = await _db.Users.FindAsync(id);
    return user; // null = not found? not authorized? not loaded?
}
```

Here's the same method with Arkn:

```csharp
// Arkn approach — the type tells the whole story
public async Task<Result<User>> GetUserAsync(Guid id)
{
    var user = await _db.Users.FindAsync(id);

    if (user is null)
        return Error.NotFound("User.NotFound", $"User {id} does not exist.");

    return user; // implicit conversion to Result<User>
}
```

At the call site, the compiler forces you to handle both outcomes:

```csharp
var result = await userService.GetUserAsync(id);

return result.Match(
    onSuccess: user  => Ok(user),
    onFailure: error => error.Type switch
    {
        ErrorType.NotFound => NotFound(new { error.Code, error.Message }),
        _                  => Problem(error.Message)
    });
```

No hidden nulls. No surprise exceptions. No ambiguous contracts. The type system does the work.

We'll go deep on `Result<T>` in the next post. For now, just notice that it makes the unhappy path as visible as the happy path.

## How to Install

All Arkn packages target **.NET 9 and .NET 10**. Install only the ones you need:

```bash
# The core Result pattern — start here
dotnet add package Arkn.Results

# Domain primitives
dotnet add package Arkn.Core

# Fluent typed HTTP client
dotnet add package Arkn.Http

# Background job scheduling (with persistence, distributed lock, and DLQ)
dotnet add package Arkn.Jobs

# Structured logging
dotnet add package Arkn.Logging

# Notification abstraction
dotnet add package Arkn.Notifications

# Notifiers (pick what you need)
dotnet add package Arkn.Extensions.Notifications.Slack
dotnet add package Arkn.Extensions.Notifications.Email
dotnet add package Arkn.Extensions.Notifications.Teams
dotnet add package Arkn.Extensions.Notifications.Discord

# Logging sinks (pick what you need)
dotnet add package Arkn.Extensions.Logging.ApplicationInsights
dotnet add package Arkn.Extensions.Logging.Seq
dotnet add package Arkn.Extensions.Logging.Elasticsearch

# Roslyn analyzers — enforce Arkn patterns at compile time (ARK001–ARK008)
dotnet add package Arkn.Analyzers

# Source generator — eliminate error factory boilerplate
dotnet add package Arkn.SourceGen

# MCP server — AI-assisted scaffolding and validation
dotnet tool install -g Arkn.MCP
```

Or, if you want to scaffold a full project from a template:

```bash
dotnet new install Arkn.Templates

dotnet new arkn-api -n MyApi        # Minimal API with Results + Http
dotnet new arkn-job -n MyWorker     # Worker Service with Jobs + Notifications
dotnet new arkn-lib -n MyLibrary    # Class library with Core + Results
```

## What This Series Covers

This is the first post in a twelve-part series. Here's the roadmap:

1. **Introducing Arkn** ← you are here
2. **Stop Throwing Exceptions: The `Result<T>` Pattern with `Arkn.Results`**
3. **Domain Primitives Done Right: `Arkn.Core`**
4. **Typed HTTP Clients Without the Boilerplate: `Arkn.Http`**
5. **Structured Logging That Actually Makes Sense: `Arkn.Logging`**
6. **Cron Jobs, Retry, and Timeout Done Right: `Arkn.Jobs`**
7. **Slack, Teams, Discord and Email Notifications Without External SDKs: `Arkn.Notifications`**
8. **Making Your .NET Framework AI-Ready: `Arkn.Analyzers` and `Arkn.MCP`**
9. **Zero Boilerplate: Generating Error Factories with `Arkn.SourceGen`**
10. **The First .NET Framework with a Native MCP Server**
11. **Arkn v0.3.0: What's New**
12. **Structured Logging Without Serilog: `Arkn.Logging` + Seq + Elasticsearch**

Each post stands alone. If you care about HTTP clients, jump to post 4. If you're building a worker service, jump to post 6. If you want to understand the foundation first, read them in order.

## A Final Thought

Frameworks are tools. The best tools are the ones you stop noticing because they fit so naturally into the work. Arkn's goal is to be the kind of tool where, six months into a project, you're not fighting the framework — you're just writing code.

Source code, samples, and documentation are all on GitHub: [github.com/fernando-terra/arkn](https://github.com/fernando-terra/arkn)

---

**Next in the series:** [Stop Throwing Exceptions: The Result\<T\> Pattern with Arkn.Results](./02-arkn-results.md) — where we go deep on the `Result<T>` API, error factories, functional chaining, and real-world examples.

---

*All packages available on [NuGet](https://www.nuget.org/packages?q=Arkn) — current version: **v0.3.0**.*

*Author: Fernando Terra | [github.com/fernando-terra/arkn](https://github.com/fernando-terra/arkn)*
