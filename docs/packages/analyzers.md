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
