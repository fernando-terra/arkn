# Getting Started

> **v0.2.0** — This guide covers the current stable release.

## Installation

Install only the packages you need:

```bash
# Core abstractions (Entity, ValueObject, AggregateRoot)
dotnet add package Arkn.Core

# Result pattern — explicit success/failure
dotnet add package Arkn.Results

# Structured logging with pluggable sinks
dotnet add package Arkn.Logging

# Zero-dependency cron job scheduler
dotnet add package Arkn.Jobs

# Notification abstractions + Slack
dotnet add package Arkn.Notifications
dotnet add package Arkn.Extensions.Notifications.Slack

# Typed HTTP client with retry, OAuth2, mTLS
dotnet add package Arkn.Http

# Roslyn analyzers — compile-time enforcement (ARK001–ARK008)
dotnet add package Arkn.Analyzers

# Source generator — eliminates Error factory boilerplate
dotnet add package Arkn.SourceGen

# MCP Server — scaffold + validate tools for AI assistants
dotnet tool install -g Arkn.MCP
```

## Quick start with Result pattern

```csharp
using Arkn.Results;

// Define errors (manually or via Arkn.SourceGen)
public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User {id} was not found.");

    public static Error InvalidEmail =>
        Error.Validation("User.InvalidEmail", "Email address is not valid.");
}

// Return Result from your domain/application methods
public async Task<Result<UserDto>> GetUserAsync(Guid id)
{
    var user = await _repo.FindAsync(id);
    if (user is null) return UserErrors.NotFound(id);
    return new UserDto(user.Id, user.Name, user.Email);
}

// Handle at the boundary (e.g. Minimal API)
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

## AI-assisted development

Arkn ships a **Model Context Protocol server** that integrates with Claude, Cursor and GitHub Copilot, giving your AI assistant direct knowledge of Arkn patterns:

```bash
# Install the MCP tool
dotnet tool install -g Arkn.MCP
```

Add to your AI client config (example for Claude Desktop):

```json
{
  "mcpServers": {
    "arkn": { "command": "arkn-mcp", "args": [] }
  }
}
```

Once connected, your assistant can:
- Generate error groups (`scaffold_errors`)
- Scaffold jobs and HTTP clients
- Validate code against ARK001–ARK008 rules
- Search Arkn documentation inline

→ See the full [MCP Server guide](/mcp) for configuration details.

## Use dotnet new templates

```bash
# Install templates
dotnet new install Arkn.Templates

# Create a Minimal API project
dotnet new arkn-api -n MyApi

# Create a Worker with background jobs
dotnet new arkn-job -n MyWorker

# Create a class library
dotnet new arkn-lib -n MyLibrary
```
