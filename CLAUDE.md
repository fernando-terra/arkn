# Arkn — Context for AI Agents

## What is Arkn?

Arkn is a zero-dependency, composable .NET 10 framework. It enforces **explicit, predictable patterns** so that AI-generated code is correct on the first attempt.

## Core pattern: Result<T>

**Never throw exceptions for business logic. Always return Result or Result<T>.**

```csharp
// Creating results
Result.Success()                          // void success
Result.Success(value)                     // typed success
Result<T>.Ok(value)                       // shorthand
Result.Failure(Error.NotFound(...))       // failure
Result<T>.Fail(error)                     // shorthand

// Error factories
Error.Failure("Order.Failed", "message")
Error.NotFound("User.NotFound", "message")
Error.Validation("Email.Invalid", "message")
Error.Conflict("Email.AlreadyRegistered", "message")
Error.Unauthorized("Auth.TokenExpired", "message")
```

**Error code format: always `Namespace.Reason`** — both segments PascalCase.

## Error factories — message is optional

All `Error.*` factories accept an optional `message`. When omitted, it defaults to the code.

```csharp
// Concise — message defaults to code
Error.NotFound("User.NotFound")
Error.Validation("Order.QuantityInvalid")

// Explicit — custom user-facing message
Error.NotFound("User.NotFound", $"User '{id}' was not found.")
```

## ErrorGroup pattern — define errors in one place

Never scatter `Error.*` calls across services. Group them in a static class per domain:

```csharp
public static class UserErrors
{
    // Dynamic — takes runtime context
    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User '{id}' was not found.");

    // Static — no runtime context needed
    public static readonly Error InvalidEmail =
        Error.Validation("User.InvalidEmail", "Email format is invalid.");

    public static readonly Error EmailAlreadyRegistered =
        Error.Conflict("User.EmailAlreadyRegistered", "This email is already in use.");
}

// Usage — clean, no inline Error.* calls
public Result<User> GetById(Guid id)
{
    var user = _repo.Find(id);
    return user is null ? UserErrors.NotFound(id) : Result.Success(user);
}
```

## Consuming results

```csharp
// Match — preferred for endpoints
result.Match(
    onSuccess: value => ...,
    onFailure: error => ...);

// Chaining
result.Map(v => transform(v))
      .Bind(v => anotherOperation(v))
      .Tap(v => sideEffect(v));

// FirstError for single-error results
if (result.IsFailure) log(result.FirstError);
```

## Packages

| Package | Namespace | Purpose |
|---------|-----------|---------|
| `Arkn.Core` | `Arkn.Core` | IEntity, IValueObject, IAggregateRoot |
| `Arkn.Results` | `Arkn.Results` | Result<T>, Error, ErrorType |
| `Arkn.Http` | `Arkn.Http` | Fluent typed HTTP client |
| `Arkn.Jobs` | `Arkn.Jobs` | Cron scheduler, retry, timeout |
| `Arkn.Logging` | `Arkn.Logging` | Structured logging with pluggable sinks |
| `Arkn.Notifications` | `Arkn.Notifications` | Pluggable notifiers |
| `Arkn.Extensions.Notifications.Slack` | — | Slack via Incoming Webhook + Block Kit |
| `Arkn.Analyzers` | — | Roslyn analyzers ARK001–ARK004 |

## Minimal API pattern

```csharp
// GlobalUsings.cs — required to avoid namespace conflict with Arkn.Results
global using static Microsoft.AspNetCore.Http.Results;
global using Arkn.Results;

// Endpoint
app.MapGet("/users/{id}", async (Guid id, IUserService svc) =>
{
    Result<UserDto> result = await svc.GetByIdAsync(id);
    return result.Match(
        onSuccess: dto   => Ok(dto),
        onFailure: error => error.Type switch
        {
            ErrorType.NotFound   => NotFound(new { error.Code, error.Message }),
            ErrorType.Validation => BadRequest(new { error.Code, error.Message }),
            _                    => Problem(error.Message)
        });
});
```

## Job pattern

```csharp
// Must return Task<Result> — enforced by ARK004 analyzer
public class InvoiceProcessorJob : IArknJob
{
    private readonly ILogger _logger;

    public InvoiceProcessorJob(ILogger logger) => _logger = logger;

    public async Task<Result> ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.Log("Processing invoices...");
        // business logic
        return Result.Success();
    }
}

// Registration
builder.Services.AddArknJobs(jobs =>
{
    jobs.Add<InvoiceProcessorJob>("0 2 * * *")
        .WithTimeout(TimeSpan.FromMinutes(10))
        .WithRetry(maxAttempts: 3)
        .NotifyOn(JobEvent.Failed | JobEvent.TimedOut);
});
```

## Analyzers — violations to avoid

| Rule | What to avoid |
|------|--------------|
| ARK001 | `public Order GetOrder()` — must be `Result<Order>` |
| ARK002 | `Error.NotFound("usernotfound", ...)` — must be `"User.NotFound"` |
| ARK003 | `_ = GetOrder(id);` — Result cannot be discarded |
| ARK004 | `public async Task ExecuteAsync()` in IArknJob — must be `Task<Result>` |

## Templates

```bash
dotnet new install Arkn.Templates
dotnet new arkn-api -n MyApi        # Minimal API
dotnet new arkn-job -n MyWorker     # Background worker
dotnet new arkn-lib -n MyLibrary    # Class library
```
