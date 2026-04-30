# Arkn

[![CI](https://github.com/fernando-terra/arkn/actions/workflows/ci.yml/badge.svg)](https://github.com/fernando-terra/arkn/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)](https://dotnet.microsoft.com/)

> **Architecture Kernel for modern .NET** — design patterns as composable packages.

Arkn is a collection of lightweight, zero-dependency packages that bring
well-known software architecture patterns to .NET 10 projects. Each pattern
lives in its own NuGet package so you only pull in what you actually need.

---

## Philosophy

| Principle         | What it means for Arkn                                              |
|-------------------|----------------------------------------------------------------------|
| **Zero lock-in**  | `Arkn.Core` and `Arkn.Results` depend solely on the .NET BCL.      |
| **Composable**    | Every pattern is an independent package — mix and match.            |
| **Interface-first** | Consumers code against `Arkn.Core` interfaces, not concrete types.|
| **Test-friendly** | Immutable value types and pure functions make mocking trivial.      |

---

## Packages

| Package          | Description                                      | Status |
|------------------|--------------------------------------------------|--------|
| `Arkn.Core`      | Base interfaces and abstractions                 | ✅ v0.1.0 |
| `Arkn.Results`   | Result<T> pattern with full functional API       | ✅ v0.1.0 |
| `Arkn.CQRS`      | ICommand / IQuery / IHandler                     | 🗺️ Roadmap |
| `Arkn.Events`    | Domain events and IEventBus                      | 🗺️ Roadmap |
| `Arkn.Persistence` | IRepository / IUnitOfWork                     | 🗺️ Roadmap |
| `Arkn.Http`      | Result → IActionResult / IResult helpers         | 🗺️ Roadmap |

---

## Quick Start

### Install

```bash
dotnet add package Arkn.Results
```

### Create errors

```csharp
using Arkn.Results;

var notFound     = Error.NotFound    ("USER.NOT_FOUND",   "User with id 42 was not found.");
var invalid      = Error.Validation  ("USER.EMAIL_INVALID","Email format is invalid.");
var conflict     = Error.Conflict    ("USER.EMAIL_TAKEN", "Email already registered.");
var unauthorized = Error.Unauthorized("AUTH.TOKEN_EXPIRED","Token has expired.");
var failure      = Error.Failure     ("SVC.TIMEOUT",      "Downstream timeout.");
```

### Return results

```csharp
// Success
Result<User> ok = Result<User>.Ok(user);
// or via implicit conversion
Result<User> ok2 = user;

// Failure — single error
Result<User> fail = Result<User>.Fail(Error.NotFound("USER.NF", "Not found."));
// or via implicit conversion
Result<User> fail2 = Error.NotFound("USER.NF", "Not found.");

// Failure — multiple validation errors
var errors = new List<IError>
{
    Error.Validation("VAL.NAME",  "Name is required."),
    Error.Validation("VAL.EMAIL", "Email is invalid."),
};
Result<User> multiError = Result<User>.Fail(errors);
```

### Chain operations (railway-oriented)

```csharp
public Result<OrderDto> PlaceOrder(PlaceOrderRequest req) =>
    ValidateRequest(req)
        .Bind(_ => ReserveInventory(req.ProductId, req.Quantity))
        .Bind(reservation => CreateOrder(req, reservation))
        .Map(order => OrderDto.From(order));
```

### Unwrap at the boundary

```csharp
// In a minimal API handler:
return result.Match<IResult>(
    user   => Results.Ok(user),
    errors => MapErrors(errors));
```

---

## Project Structure

```
src/
  Arkn.Core/          ← Interfaces: IResult, IResult<T>, IError, ErrorType
  Arkn.Results/       ← Result<T>, Result, Error — full implementation

tests/
  Arkn.Core.Tests/
  Arkn.Results.Tests/

samples/
  Arkn.Sample.Api/    ← Minimal API demonstrating end-to-end usage

docs/
  architecture.md     ← Design decisions and package layout
  results.md          ← Arkn.Results API reference
```

---

## Roadmap

- [x] `Arkn.Core` — base abstractions
- [x] `Arkn.Results` — Result pattern with functional API
- [ ] `Arkn.CQRS` — command/query pipeline
- [ ] `Arkn.Events` — domain events
- [ ] `Arkn.Persistence` — repository pattern
- [ ] `Arkn.Http` — HTTP adapter helpers
- [ ] Async variants (`MapAsync`, `BindAsync`)
- [ ] Source generator for exhaustive Match

---

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). All PRs must include tests.

## License

MIT © [Bluezee](https://github.com/fernando-terra)
