# Arkn Framework — Copilot Instructions

This repository uses the **Arkn** framework. Follow these rules strictly when generating C# code.

## Result<T> — Never throw, always return

All domain and application methods **must** return `Result` or `Result<T>`. Never use `throw` for business logic failures.

```csharp
// ✅ Correct
public Result<Order> PlaceOrder(PlaceOrderRequest request)
{
    if (request.Items.Count == 0)
        return Error.Validation("Order.EmptyItems", "An order must have at least one item.");

    var order = new Order(request);
    return Result.Success(order);
}

// ❌ Wrong
public Order PlaceOrder(PlaceOrderRequest request)
{
    if (request.Items.Count == 0)
        throw new ArgumentException("An order must have at least one item.");

    return new Order(request);
}
```

## ErrorGroup pattern — define errors in one place

Never scatter `Error.*` calls across services. Group them in a static class per domain:

```csharp
public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User '{id}' was not found.");

    public static readonly Error InvalidEmail =
        Error.Validation("User.InvalidEmail", "Email format is invalid.");

    public static readonly Error EmailAlreadyRegistered =
        Error.Conflict("User.EmailAlreadyRegistered", "This email is already in use.");
}

// Usage — no inline Error.* in services
public Result<User> GetById(Guid id)
{
    var user = _repo.Find(id);
    return user is null ? UserErrors.NotFound(id) : Result.Success(user);
}
```

## Error factories — message is optional

```csharp
// Concise — message defaults to code
Error.NotFound("User.NotFound")
Error.Validation("Order.QuantityInvalid")

// Explicit — when a custom user-facing message is needed
Error.NotFound("User.NotFound", $"User '{id}' was not found.")
```

## Error codes — always Namespace.Reason

Error codes must follow the `Namespace.Reason` pattern. Both segments start with uppercase.

```csharp
// ✅ Correct
Error.NotFound("User.NotFound", "User not found.")
Error.Validation("Order.ItemQuantityInvalid", "Quantity must be positive.")
Error.Conflict("Email.AlreadyRegistered", "This email is already in use.")

// ❌ Wrong
Error.NotFound("user_not_found", "User not found.")
Error.Validation("invalid", "Quantity must be positive.")
```

## Consuming Result — always use Match, Map, or Bind

Never access `.Value` directly without checking `.IsSuccess` first. Prefer `.Match()` for branching.

```csharp
// ✅ Correct — HTTP endpoint
return result.Match(
    onSuccess: order => Results.Created($"/orders/{order.Id}", order),
    onFailure: error => error.Type switch
    {
        ErrorType.Validation => Results.BadRequest(new { error.Code, error.Message }),
        ErrorType.NotFound   => Results.NotFound(new { error.Code, error.Message }),
        _                    => Results.Problem(error.Message)
    });

// ✅ Correct — chaining
Result<string> summary = await GetOrder(id)
    .Map(order => order.ToSummary())
    .BindAsync(summary => FormatAsync(summary));

// ❌ Wrong
var result = await GetOrder(id);
var order = result.Value; // throws if failure — don't do this
```

## IArknJob — ExecuteAsync must return Task<Result>

```csharp
// ✅ Correct
public class InvoiceProcessorJob : IArknJob
{
    public async Task<Result> ExecuteAsync(CancellationToken cancellationToken)
    {
        // ...
        return Result.Success();
    }
}

// ❌ Wrong
public class InvoiceProcessorJob : IArknJob
{
    public async Task ExecuteAsync(CancellationToken cancellationToken) { }
}
```

## Package reference guide

| Need | Package |
|------|---------|
| Result<T>, Error, ErrorType | `Arkn.Results` |
| HTTP typed client | `Arkn.Http` |
| Background jobs with cron | `Arkn.Jobs` |
| Structured logging | `Arkn.Logging` |
| Notifications (Slack, etc.) | `Arkn.Notifications` |
| Pattern enforcement at compile time | `Arkn.Analyzers` |

## GlobalUsings for Minimal API projects

Add to `GlobalUsings.cs` to avoid namespace conflicts:

```csharp
global using static Microsoft.AspNetCore.Http.Results;
global using Arkn.Results;
```

## Analyzers — rules enforced at compile time

The following rules are checked by `Arkn.Analyzers` and will produce warnings/errors if violated:

- **ARK001** — Domain methods must return `Result` or `Result<T>`
- **ARK002** — Error codes must follow `Namespace.Reason` pattern
- **ARK003** — `Result` must not be silently discarded
- **ARK004** — `IArknJob.ExecuteAsync` must return `Task<Result>`
