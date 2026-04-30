# Arkn.Results — Reference

## Overview

`Arkn.Results` implements the **Result pattern** for .NET 10. It provides a
discriminated union between success and failure, with a rich functional API
and zero external dependencies.

---

## `Error`

Immutable record representing a typed error.

```csharp
// Factory methods
Error.Failure    ("code", "message", metadata?)
Error.NotFound   ("code", "message", metadata?)
Error.Validation ("code", "message", metadata?)
Error.Conflict   ("code", "message", metadata?)
Error.Unauthorized("code", "message", metadata?)
```

Errors are records — equality is structural, not reference-based.

### Metadata

Attach arbitrary key-value context:

```csharp
var error = Error.Validation("VAL.AGE", "Age must be positive.", 
    new Dictionary<string, object> { ["field"] = "age", ["given"] = -1 });
```

---

## `Result` (void)

For operations that succeed or fail without producing a value.

```csharp
// Create
Result.Ok()
Result.Fail(error)
Result.Fail(IEnumerable<IError> errors)

// Inspect
result.IsSuccess
result.IsFailure
result.Errors          // IReadOnlyList<IError>
result.FirstError      // first error or Error.None

// Chain
result.Bind(() => Result.Ok())          // void → void
result.Bind(() => Result<T>.Ok(value))  // void → typed

// Unwrap
result.Match(onSuccess: () => ..., onFailure: errors => ...)
```

---

## `Result<T>`

For operations that produce a value on success.

```csharp
// Create
Result<T>.Ok(value)
Result<T>.Fail(error)
Result<T>.Fail(IEnumerable<IError> errors)

// Implicit conversions
Result<int> r1 = 42;                          // from value
Result<int> r2 = Error.NotFound("NF", "...");  // from Error

// Inspect
result.Value   // throws InvalidOperationException on failure — always check IsSuccess first
result.IsSuccess
result.IsFailure
result.Errors
result.FirstError

// Map — transform the success value
Result<int>.Ok(5).Map(v => v * 2)  // Result<int> Ok(10)

// Bind — chain to another Result
Result<int>.Ok(3).Bind(v => Result<string>.Ok($"n={v}"))  // Result<string> Ok("n=3")
Result<int>.Ok(3).Bind(v => Result.Ok())                  // Result Ok

// Match — terminal unwrap
result.Match(
    onSuccess: value  => $"Got {value}",
    onFailure: errors => $"Error: {errors[0].Message}");
```

---

## Full Pipeline Example

```csharp
public Result<OrderDto> PlaceOrder(PlaceOrderRequest req)
{
    return ValidateRequest(req)
        .Bind(_ => ReserveInventory(req.ProductId, req.Quantity))
        .Bind(reservation => CreateOrder(req, reservation))
        .Map(order => OrderDto.From(order));
}
```

---

## HTTP Mapping

Map `ErrorType` → HTTP status codes:

```csharp
var status = error.Type switch
{
    ErrorType.NotFound     => 404,
    ErrorType.Validation   => 422,
    ErrorType.Conflict     => 409,
    ErrorType.Unauthorized => 401,
    _                      => 500
};
```

See `samples/Arkn.Sample.Api/Program.cs` for a complete minimal-API integration.

---

## Error Codes Convention

Use dot-separated, SCREAMING_SNAKE namespaces:

```
DOMAIN.NOUN_VERB
USER.NOT_FOUND
ORDER.QUANTITY_INVALID
PAYMENT.CARD_DECLINED
```

This makes grep and log filtering trivial in production.
