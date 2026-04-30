# Arkn — Architecture

## Design Philosophy

Arkn is built on a single constraint: **zero external dependencies in core packages**.

Every abstraction in `Arkn.Core` is just a C# interface or enum. Every implementation in `Arkn.Results` uses only the .NET 10 BCL. No NuGet packages. No transitive version conflicts. No lock-in.

## Package Layout

```
Arkn.Core        ← interfaces, enums, shared types (no deps)
Arkn.Results     ← Result<T>, Error, functional API (depends on Arkn.Core only)
Arkn.CQRS        ← ICommand, IQuery, IHandler (roadmap)
Arkn.Events      ← domain events, IEventBus (roadmap)
Arkn.Persistence ← IRepository, IUnitOfWork (roadmap)
Arkn.Http        ← Result → HttpResponse helpers (roadmap)
```

Each package is independently versioned. Consumers pick exactly what they need.

## Core Abstractions

### `IError`

```csharp
public interface IError
{
    string Code { get; }
    string Message { get; }
    ErrorType Type { get; }
    IReadOnlyDictionary<string, object>? Metadata { get; }
}
```

`ErrorType` drives HTTP status code mapping and domain semantics:

| ErrorType     | Typical HTTP | Meaning                          |
|---------------|-------------|----------------------------------|
| Failure        | 500          | Generic, unclassified failure     |
| NotFound       | 404          | Resource not found                |
| Validation     | 422          | One or more rules violated        |
| Conflict       | 409          | State conflict                    |
| Unauthorized   | 401          | Missing / invalid credentials     |

### `IResult` / `IResult<T>`

```csharp
public interface IResult
{
    bool IsSuccess { get; }
    bool IsFailure { get; }
    IReadOnlyList<IError> Errors { get; }
}

public interface IResult<out T> : IResult
{
    T Value { get; }
}
```

## Railway-Oriented Flow

Operations are chained using `Map`, `Bind`, and `Match`. Failures short-circuit the chain:

```
Ok(input)
  .Map(transform)        // Success → transform value; Failure → skip
  .Bind(sideEffect)      // Success → run next op; Failure → skip
  .Match(onOk, onFail)   // Terminal: unwrap to a concrete type
```

## Validation Pattern

Collect *all* errors before returning — never stop at the first one:

```csharp
var errors = new List<IError>();

if (string.IsNullOrWhiteSpace(req.Name))
    errors.Add(Error.Validation("VAL.NAME", "Name is required."));

if (!IsValidEmail(req.Email))
    errors.Add(Error.Validation("VAL.EMAIL", "Email is invalid."));

if (errors.Count > 0)
    return Result<User>.Fail(errors);
```

## Dependency Graph

```
consumer → Arkn.Results → Arkn.Core → (nothing)
```

No transitive NuGet references. Ever.
