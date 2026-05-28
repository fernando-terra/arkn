# Getting Started

> **v0.3.1** — This guide covers the current stable release, fully supporting **.NET 8, 9, and 10**.

Welcome to Arkn! This framework was built to solve the most common architectural pains in .NET: **Vendor Lock-in**, **Boilerplate**, and **Hidden Control Flow**. 

Before jumping into the code, understand our core philosophy: **Stop using exceptions to handle business logic.**

## The Arkn Way: Result<T>

In traditional .NET, missing data or validation errors are often handled by throwing exceptions. This hides control flow and makes method signatures lie.

```csharp
// ❌ Before: The signature says it returns a User, but it might throw!
public async Task<User> GetUserAsync(Guid id)
{
    var user = await _repo.FindAsync(id);
    if (user is null) throw new NotFoundException($"User {id} not found");
    return user;
}
```

With Arkn, **failures are explicit**:

```csharp
// ✅ After: The signature tells the truth. It returns a User OR an Error.
public async Task<Result<User>> GetUserAsync(Guid id)
{
    var user = await _repo.FindAsync(id);
    if (user is null) return UserErrors.NotFound(id);
    return user;
}
```

---

## Installation

Arkn is extremely modular. You only install what you actually need.

### 1. The Core (Zero Dependencies)
If you are building a Domain Layer or Application Layer, these two packages are all you need. They have no external references.

```bash
# Domain primitives (IEntity, IValueObject, IAggregateRoot)
dotnet add package Arkn.Core

# Result pattern and Error types
dotnet add package Arkn.Results
```

### 2. Infrastructure & Tooling
For your external layers, choose the capabilities you need:

```bash
# Compile-time enforcement (ARK001–ARK008 rules)
dotnet add package Arkn.Analyzers

# Typed HTTP client with retry, OAuth2, mTLS
dotnet add package Arkn.Http

# Zero-dependency cron job scheduler
dotnet add package Arkn.Jobs

# Structured logging with pluggable sinks
dotnet add package Arkn.Logging
```

---

## Example: Building a Minimal API

Here is how Arkn connects your domain directly to the edge of your API:

```csharp
using Arkn.Results;

// 1. Define your errors once
public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User {id} was not found.");

    public static Error InvalidEmail =>
        Error.Validation("User.InvalidEmail", "Email address is not valid.");
}

// 2. Handle at the boundary using .Match()
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

Arkn ships a **Model Context Protocol (MCP) server** that integrates directly with Claude, Cursor, and GitHub Copilot. This gives your AI assistant direct knowledge of Arkn patterns so it can generate correct code on the first try.

```bash
# Install the MCP tool globally
dotnet tool install -g Arkn.MCP
```

Add this to your AI client configuration (e.g., Claude Desktop or Cursor):

```json
{
  "mcpServers": {
    "arkn": { "command": "arkn-mcp", "args": [] }
  }
}
```

Once connected, your assistant can natively:
- Generate Error groups (`scaffold_errors`)
- Scaffold Background Jobs
- Validate code against ARK001–ARK008 rules

→ See the full [MCP Server guide](/mcp) for configuration details.

---

## Scaffolding with dotnet new

Want a working project in 5 seconds? Use our templates.

```bash
# Install templates
dotnet new install Arkn.Templates

# Create a Minimal API project wired with Result<T> end-to-end
dotnet new arkn-api -n MyApi

# Create a Worker service with background jobs
dotnet new arkn-job -n MyWorker
```
