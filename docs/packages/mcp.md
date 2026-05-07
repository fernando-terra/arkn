# Arkn.MCP

MCP Server distributed as a `dotnet tool` — exposes scaffolding and validation tools to AI assistants (Claude, Cursor, GitHub Copilot) via the **Model Context Protocol** (stdio transport).

```bash
dotnet tool install -g Arkn.MCP
```

> For full installation and configuration instructions, see the [**MCP Server guide**](/mcp).

## Available tools

### `scaffold_errors`
Generates a complete Arkn error group class for a given domain.

```
Input:  domain = "User"
Output: [ArknErrors] public static partial class UserErrors { ... }
```

Generates `NotFound`, `Invalid`, `Conflict`, `Unauthorized` — all following the **ARK002** naming convention automatically.

---

### `scaffold_job`
Generates an `IArknJob` implementation and DI registration snippet.

```
Input:  name = "InvoiceProcessor", cron = "0 2 * * *", description = "Processes pending invoices"
Output: public sealed class InvoiceProcessorJob : IArknJob { ... }
        + AddArknJobs() registration with retry, timeout, NotifyOn
```

---

### `scaffold_http_client`
Generates a typed `ArknHttpClient` with methods inferred from operation names.

```
Input:  name = "Payment", baseUrl = "https://api.pay.com", operations = "GetPayment,CreatePayment,CancelPayment"
Output: public sealed class PaymentClient : ArknHttpClient { ... }
        + AddArknHttp<PaymentClient>() registration
```

---

### `validate_pattern`
Analyzes a C# code snippet and returns violations with line numbers and fix suggestions.

**Rules checked:** ARK001–ARK008 (see [Arkn.Analyzers](/packages/analyzers) for full list)

```json
[
  { "rule": "ARK002", "line": 12, "message": "...", "suggestion": "..." }
]
```

---

### `docs_lookup`
Searches embedded Arkn documentation by keyword. No HTTP required.

```
Input:  query = "result pattern"
Output: # Arkn.Results — Result Pattern ...
```

Topics: `result`, `error`, `iarknjob`, `iarknlogger`, `addarknhttp`, `analyzers`, `sourcegen`, `templates`

---

## Refactoring tools <Badge type="tip" text="v0.3.1" />

### `refactor_to_result`

Converts C# code with exception-based error handling into the Arkn `Result<T>` pattern. Generates an `[ArknErrors]` class for the detected domain and rewrites the method(s).

**What it does automatically:**
- Generates an `[ArknErrors]` ErrorGroup class for the domain (inferred from class name if not provided)
- Replaces `throw new XException(...)` with `DomainErrors.Reason(...)` calls
- Converts `void` returns to `Result` and `T` returns to `Result<T>`
- Converts rethrows and empty `catch` blocks
- Classifies exception types: `ArgumentNullException` → `Validation`, `KeyNotFoundException` → `NotFound`, `UnauthorizedAccessException` → `Unauthorized`, etc.

```
Input:  code   = "public class OrderService { public void Cancel() { throw new InvalidOperationException(\"already cancelled\"); } }"
        domain = "Order"   (optional — inferred from class name if omitted)
```

**Example output:**

```csharp
// STEP 1 — ErrorGroup class (add to your domain project)
[ArknErrors]
public static partial class OrderErrors
{
    [ArknErrorCode("Conflict", "Order invalidoperation error")]
    public static partial Error InvalidOperation(string? detail = null);
}

// STEP 2 — Refactored method
using Arkn.Results;

public class OrderService
{
    public Result Cancel()
    {
        return OrderErrors.InvalidOperation("already cancelled");
    }
}

// ✔ Added 'using Arkn.Results;'
// ✔ Changed return type from void to Result
// ✔ Replaced 'throw new InvalidOperationException' with OrderErrors.InvalidOperation()
```

**When to use:** Migrating legacy code that uses exceptions for control flow. Pass a whole class or individual methods.

---

### `migrate_exception`

Converts a specific `catch` block into a `Result.Failure` return with the semantically correct Arkn `ErrorType`. Preserves existing logging and explains the ErrorType choice.

```
Input:  catchBlock = "catch (ArgumentNullException ex) { throw; }"
        errorCode  = "Order.InvalidInput"   (optional — auto-generated if omitted)
```

**Example output:**

```csharp
// Migrated by Arkn.MCP migrate_exception

catch (ArgumentNullException ex)
{
    return Result.Failure(Error.Validation("Order.InvalidInput", ex.Message));
}

// ── Why this ErrorType?
// Exception type : ArgumentNullException
// → Arkn ErrorType: Error.Validation  (reason: indicates bad/missing input data)
// Error code      : "Order.InvalidInput"  (ARK002-compliant Namespace.Reason)

// ── Before / After summary
// Before: rethrow — exception propagated as unhandled (ARK007 / ARK008)
// After : Result.Failure returned — caller receives a typed, inspectable error
```

**Exception → ErrorType mapping:**

| Exception | ErrorType |
|---|---|
| `ArgumentNullException`, `ArgumentOutOfRangeException` | `Validation` |
| `KeyNotFoundException`, `FileNotFoundException` | `NotFound` |
| `InvalidOperationException` | `Conflict` |
| `UnauthorizedAccessException` | `Unauthorized` |
| `TimeoutException`, `TaskCanceledException` | `Failure` |
| Everything else | `Failure` |

**When to use:** Converting individual catch blocks one at a time. Use `refactor_to_result` when migrating entire methods.

---

## Review & explanation tools <Badge type="tip" text="v0.3.1" />

### `explain_error`

Explains an Arkn error code in natural language. Validates ARK002 compliance and returns the error type, meaning, recommended HTTP status, when to use it, and complete usage examples.

```
Input:  errorCode = "User.NotFound"
```

**Example output:**

```
# User.NotFound

| Property      | Value                                                  |
|---------------|--------------------------------------------------------|
| Error factory | Error.NotFound                                         |
| Meaning       | The requested resource does not exist                  |
| HTTP status   | 404 Not Found                                          |
| Use when      | when a database lookup or lookup by id returns nothing |

## Usage example

### 1. Define in an ErrorGroup
[ArknErrors]
public static partial class UserErrors
{
    [ArknErrorCode("NotFound", "The requested resource does not exist")]
    public static partial Error NotFound(string? detail = null);
}

### 2. Return from domain method
public async Task<Result<UserDto>> GetAsync(Guid id)
{
    var entity = await _repo.FindAsync(id);
    if (entity is null) return UserErrors.NotFound($"{id} was not found.");
    return new UserDto(entity);
}

### 3. Handle at API boundary
return result.Match(
    onSuccess: dto   => Results.Ok(dto),
    onFailure: error => error.Type switch
    {
        ErrorType.NotFound => Results.NotFound(new { error.Code, error.Message }),
        _                  => Results.Problem(error.Message)
    });
```

**Supported reason suffixes:** `NotFound`, `NotFound`, `Invalid`, `Validation`, `Conflict`, `Unauthorized`, `Forbidden`, `Failure`, `TimedOut`, `AlreadyExists`, `Expired`, `Inactive` — plus any custom reason (falls back to `Error.Failure`).

**When to use:** Understanding what an error code means before writing handlers, or looking up the correct HTTP status for an error type.

---

### `review_pattern`

Reviews a C# code snippet against ARK001–ARK008 and returns a human-readable **markdown code review** — unlike `validate_pattern` which returns raw JSON.

```
Input:  code     = "var client = new HttpClient(); throw new Exception(\"err\");"
        fileName = "PaymentService.cs"   (optional)
```

**Example output:**

```markdown
# Code Review — PaymentService.cs

**Score:** 84/100 (Grade B)  |  **Violations:** 2 (0 errors, 2 warnings)

---

## ⚠️ Warning ARK005 — HttpClient created directly or injected without Arkn

> **Fix:** Register a typed client: `builder.Services.AddArknHttp<TClient>(baseUrl)`

**Line 1:** HttpClient created directly or injected without Arkn.

## ⚠️ Warning ARK007 — throw new in a domain method

> **Fix:** Replace with `return Result.Failure(Error.*(code, message))` — no exceptions in domain logic.

**Line 1:** throw new found in code.

---

## Recommended next steps

4. **Replace raw HttpClient** — use `scaffold_http_client` to generate a typed client.
```

**When to use:** Paste code from a PR review, a new service, or a legacy class and get an instant Arkn compliance score.

---

## Test generation <Badge type="tip" text="v0.3.1" />

### `generate_tests`

Generates xUnit unit tests for a C# method returning `Result` or `Result<T>`. Produces one success test and one failure test per error code, using NSubstitute for mocking and the Arrange/Act/Assert pattern.

```
Input:  methodSignature = "public async Task<Result<UserDto>> GetUserAsync(Guid id)"
        className       = "UserService"                    (optional — inferred from signature)
        errorCodes      = "User.NotFound,User.InvalidEmail" (optional — inferred if omitted)
```

**Example output:**

```csharp
// Generated by Arkn.MCP generate_tests
using Arkn.Results;
using NSubstitute;
using Xunit;

namespace YourProject.Tests;

public class GetUserAsyncTests
{
    private static IUserService BuildSut() => Substitute.For<IUserService>();

    // ── Success path

    [Fact]
    public async Task GetUserAsync_ValidInput_ShouldReturnSuccess()
    {
        // Arrange
        var sut      = BuildSut();
        var id       = Guid.NewGuid();
        var expected = new UserDto(/* TODO: fill */);
        sut.GetUserAsync(id).Returns(Task.FromResult(Result.Success(expected)));

        // Act
        var result = await sut.GetUserAsync(id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    // ── Failure paths

    [Fact]
    public async Task GetUserAsync_NotFound_ShouldReturnFailure()
    {
        // Arrange
        var sut   = BuildSut();
        var id    = Guid.NewGuid();
        var error = Error.NotFound("User.NotFound", "TODO: expected message");
        sut.GetUserAsync(id).Returns(Task.FromResult(Result.Failure<UserDto>(error)));

        // Act
        var result = await sut.GetUserAsync(id);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
        Assert.Equal("User.NotFound", result.FirstError.Code);
    }
}
```

**When to use:** Bootstrapping test coverage for a new service method, or generating a test skeleton before writing the implementation (TDD).
