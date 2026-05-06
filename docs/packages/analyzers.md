# Arkn.Analyzers

Roslyn analyzers that enforce Arkn patterns at compile time.

```bash
dotnet add package Arkn.Analyzers
```

## Rules

| ID | Title | Severity |
|---|---|---|
| ARK001 | Domain methods should return `Result` or `Result<T>` | ⚠️ Warning |
| ARK002 | Error code must follow `Namespace.Reason` pattern | ⚠️ Warning |
| ARK003 | `Result` must not be silently discarded | ⚠️ Warning |
| ARK004 | `IArknJob.ExecuteAsync` must return `Task<Result>` | ❌ Error |
| ARK005 | Raw `HttpClient` instead of `AddArknHttp<T>()` | ⚠️ Warning |
| ARK006 | MEL `ILogger` used where `IArknLogger` is available | ⚠️ Warning |
| ARK007 | `throw new` in domain method | ⚠️ Warning |
| ARK008 | `catch` block swallowing exception without `Result.Failure` | ⚠️ Warning |

## ARK001 — Result returns in Domain

```csharp
// ❌ ARK001 — throws in a Domain method
namespace MyApp.Domain.Entities;
public class Order : Entity {
    public void Cancel() {
        if (!IsActive) throw new InvalidOperationException("Already cancelled"); // ARK001
    }
}

// ✅ Correct
public Result Cancel() {
    if (!IsActive) return Error.Conflict("Order.AlreadyCancelled", "Order is already cancelled.");
    // ...
    return Result.Success();
}
```

## ARK002 — Error code naming

```csharp
// ❌ ARK002
Error.NotFound("usernotfound", "msg");  // no dot separator
Error.NotFound("user.notfound", "msg"); // lowercase start

// ✅ Correct
Error.NotFound("User.NotFound", "msg");
```

## ARK003 — Result discard

```csharp
// ❌ ARK003 — Result is discarded
GetUser(id); // result thrown away

// ✅ Correct
var result = GetUser(id);
result.Match(onSuccess: ..., onFailure: ...);
```

## ARK004 — Job return type

```csharp
// ❌ ARK004 — wrong return type
public class MyJob : IArknJob {
    public Task ExecuteAsync(ArknJobContext ctx) { ... }  // ARK004: Error
}

// ✅ Correct
public Task<Result> ExecuteAsync(ArknJobContext ctx) { ... }
```

## ARK005 — Raw HttpClient <Badge type="tip" text="v0.3.0" />

```csharp
// ❌ ARK005
var client = new HttpClient();

// ✅ Use typed client via AddArknHttp
builder.Services.AddArknHttp<PaymentClient>("https://api.pay.com");
```

## ARK006 — MEL ILogger instead of IArknLogger <Badge type="tip" text="v0.3.0" />

```csharp
// ❌ ARK006 — MEL logger in an Arkn component
public class MyService(ILogger<MyService> logger) { ... }
// ❌ Also triggers
Console.WriteLine("something happened");

// ✅ Use IArknLogger for structured, sink-routed logging
public class MyService(IArknLogger logger) { ... }
```

## ARK007 — throw in domain method

```csharp
// ❌ ARK007
public Result Process() {
    throw new InvalidOperationException("bad state"); // ARK007
}

// ✅ Return failure instead
public Result Process() {
    return Result.Failure(Error.Failure("Order.InvalidState", "Invalid state."));
}
```

## ARK008 — Empty catch swallowing exceptions

```csharp
// ❌ ARK008 — exception is swallowed, caller has no idea it failed
try { DoWork(); }
catch (Exception) { }  // ARK008

// ✅ Surface the failure via Result
try { return DoWork(); }
catch (Exception ex) {
    return Result.Failure(Error.Failure("Service.Unexpected", ex.Message));
}
```

## Suppressing a rule

```csharp
#pragma warning disable ARK001
public void LegacyMethod() { throw new NotImplementedException(); }
#pragma warning restore ARK001
```
