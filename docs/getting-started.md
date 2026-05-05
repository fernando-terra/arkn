# Getting Started

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

# Roslyn analyzers (compile-time enforcement)
dotnet add package Arkn.Analyzers

# Source generator (eliminates Error boilerplate)
dotnet add package Arkn.SourceGen
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
