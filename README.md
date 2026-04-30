# Arkn

[![CI](https://github.com/fernando-terra/arkn/actions/workflows/ci.yml/badge.svg)](https://github.com/fernando-terra/arkn/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)

**Architecture Kernel for modern .NET** — design patterns as composable, dependency-free packages.

## Philosophy

> *Stop pulling in entire frameworks to get one pattern. Use only what you need.*

Arkn is built on three principles:

- **Zero lock-in** — `Arkn.Core` and `Arkn.Results` have no external NuGet dependencies. No MediatR. No FluentValidation. No EF Core. Just .NET.
- **Composability** — each pattern lives in its own package. Take one, take all.
- **Explicitness** — no magic. Every behavior is visible, testable, and overridable.

## Packages

| Package | Description | Status |
|---|---|---|
| `Arkn.Core` | Interfaces and base primitives (Entity, ValueObject, AggregateRoot) | ✅ v0.1.0 |
| `Arkn.Results` | Result Pattern — explicit success/failure without exceptions | ✅ v0.1.0 |
| `Arkn.CQRS` | Commands, Queries, and dispatcher abstractions | 🔜 Planned |
| `Arkn.Repository` | Repository + Unit of Work abstractions | 🔜 Planned |
| `Arkn.Extensions.EfCore` | EF Core implementations of Arkn.Repository | 🔜 Planned |
| `Arkn.Extensions.MediatR` | MediatR adapter for Arkn.CQRS | 🔜 Planned |

## Quick Start

### Installation
```bash
dotnet add package Arkn.Results
```

### Result Pattern

```csharp
using Arkn.Results;

// ── Creating results ───────────────────────────────────────────────────────

Result<User> success = Result.Success(user);
Result<User> success = user;                    // implicit conversion

Result<User> failure = Result.Failure<User>(Error.NotFound("User.NotFound", "User not found"));
Result<User> failure = Error.NotFound("User.NotFound", "User not found"); // implicit

// ── Error types ────────────────────────────────────────────────────────────

Error.Failure("Order.Failed",         "Order processing failed");
Error.NotFound("Product.NotFound",    "Product not found");
Error.Validation("Email.Invalid",     "Email is not valid");
Error.Conflict("User.Exists",         "User already exists");
Error.Unauthorized("Auth.Required",   "Authentication required");
Error.Forbidden("User.Forbidden",     "Access denied");

// ── Functional chaining ────────────────────────────────────────────────────

Result<string> name = await GetUserAsync(id)          // Task<Result<User>>
    .MapAsync(u => u.Name)                             // Task<Result<string>>
    .BindAsync(name => ValidateNameAsync(name));        // Task<Result<string>>

// ── Match — branch on outcome ──────────────────────────────────────────────

IResult response = result.Match(
    onSuccess: user  => Results.Ok(user),
    onFailure: error => error.Type switch
    {
        ErrorType.NotFound     => Results.NotFound(new { error.Code, error.Message }),
        ErrorType.Validation   => Results.BadRequest(new { error.Code, error.Message }),
        ErrorType.Unauthorized => Results.Unauthorized(),
        _                      => Results.Problem(error.Message)
    });

// ── Ensure — validate inline ───────────────────────────────────────────────

result
    .Ensure(u => u.IsActive, Error.Validation("User.Inactive", "User must be active"))
    .Tap(u => logger.LogInformation("Processing user {Id}", u.Id))
    .Match(onSuccess: ..., onFailure: ...);

// ── Multiple errors ────────────────────────────────────────────────────────

var errors = validationErrors.Select(e => Error.Validation(e.Field, e.Message));
return Result.Failure<CreateOrderResponse>(errors);
// result.Errors → IReadOnlyList<Error>
```

### Domain Primitives (Arkn.Core)

```csharp
using Arkn.Core.Primitives;

public sealed class Order : AggregateRoot
{
    public string CustomerId { get; private set; }

    private Order() { }

    public static Order Create(string customerId)
    {
        var order = new Order { CustomerId = customerId };
        order.Raise(new OrderCreatedEvent(order.Id, customerId));
        return order;
    }
}

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency) { Amount = amount; Currency = currency; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

## Running the Sample

```bash
cd samples/Arkn.Sample.Api
dotnet run
# → http://localhost:5000/users
```

## Running Tests

```bash
dotnet test
```

## Roadmap

- [ ] `Arkn.CQRS` — command/query dispatcher
- [ ] `Arkn.Repository` — generic repository + unit of work
- [ ] `Arkn.Extensions.EfCore` — EF Core integration
- [ ] `Arkn.Extensions.MediatR` — MediatR adapter
- [ ] `Arkn.Pagination` — cursor + offset pagination primitives
- [ ] NuGet publishing automation (tag → release)

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). PRs are welcome — open an issue first for non-trivial changes.

## License

MIT © [Fernando Terra](https://github.com/fernando-terra)
