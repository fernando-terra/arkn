# Arkn.MCP

**MCP Server for AI assistants — scaffolding and validation tools, no hosting required.**

`Arkn.MCP` is a [Model Context Protocol](https://modelcontextprotocol.io) server distributed as a `dotnet tool`. Install it once and any compatible AI assistant (Claude, Cursor, GitHub Copilot) gains access to real Arkn tools during your conversation.

## Instalação

```bash
dotnet tool install -g Arkn.MCP
```

## Configuration

### Claude Desktop

`~/Library/Application Support/Claude/claude_desktop_config.json` (macOS) or `%APPDATA%\Claude\claude_desktop_config.json` (Windows):

```json
{
  "mcpServers": {
    "arkn": {
      "command": "arkn-mcp",
      "args": []
    }
  }
}
```

### Cursor

`.cursor/mcp.json` at the project root:

```json
{
  "mcpServers": {
    "arkn": {
      "command": "arkn-mcp",
      "args": []
    }
  }
}
```

### VS Code (GitHub Copilot)

`.vscode/mcp.json`:

```json
{
  "servers": {
    "arkn": {
      "type": "stdio",
      "command": "arkn-mcp",
      "args": []
    }
  }
}
```

---

## Tools

### `scaffold_errors`

Generates a typed ErrorGroup for a domain.

**Input:** `domain` — domain name (e.g. `"Payment"`, `"User"`, `"Invoice"`)

**Example prompt:**
> "Create the error group for the Payment domain"

**Output:**
```csharp
[ArknErrors]
public static partial class PaymentErrors
{
    [ArknErrorCode("NotFound", "Payment was not found")]
    public static partial Error NotFound(string? detail = null);

    [ArknErrorCode("Validation", "Payment data is invalid")]
    public static partial Error Invalid(string? detail = null);

    [ArknErrorCode("Conflict", "Payment already exists")]
    public static partial Error Conflict(string? detail = null);

    [ArknErrorCode("Unauthorized", "Access to Payment is not allowed")]
    public static partial Error Unauthorized(string? detail = null);
}
```

---

### `scaffold_job`

Generates a complete `IArknJob` with DI registration.

**Input:** `name`, `cron`, `description` (optional)

**Output:**
```csharp
public sealed class InvoiceProcessorJob(IArknLogger logger) : IArknJob
{
    public async Task<Result> ExecuteAsync(CancellationToken ct)
    {
        logger.Info("Starting invoice processing...");
        // TODO: implement
        return Result.Success();
    }
}

// Program.cs
services.AddArknJobs(jobs =>
{
    jobs.Add<InvoiceProcessorJob>("0 2 * * *")
        .WithRetry(maxAttempts: 3)
        .WithTimeout(TimeSpan.FromMinutes(5))
        .NotifyOn(JobEvent.Failed);
});
```

---

### `scaffold_http_client`

Generates a typed `ArknHttpClient`.

**Input:** `name`, `baseUrl`, `operations` (list of operation names)

**Output:**
```csharp
public sealed class PaymentsClient(IArknHttp http)
    : ArknHttpClient(http, "https://api.payments.example.com")
{
    public Task<Result<Payment>> GetAsync(Guid id)
        => GetAs<Payment>("/payments/{id}", id);

    public Task<Result<Payment>> CreateAsync(PaymentRequest req)
        => PostAs<Payment>("/payments", req);

    public Task<Result> DeleteAsync(Guid id)
        => Delete("/payments/{id}", id);
}

// Program.cs
services.AddArknHttp<PaymentsClient>("https://api.payments.example.com")
    .WithRetry(maxAttempts: 3)
    .WithDebugLogging(DebugLoggingOptions.Production);
```

---

### `validate_pattern`

Analyzes a C# snippet and returns Arkn rule violations.

**Input:** `code` — C# code snippet

**Rules checked:**

| Rule | Detects | Severity |
|---|---|---|
| ARK001 | Inline `Error.*()` call outside an ErrorGroup | error |
| ARK002 | Error code missing `Namespace.Reason` format | error |
| ARK003 | `.Value` accessed directly on a `Result` | error |
| ARK004 | `IArknJob.ExecuteAsync` returning `Task` instead of `Task<Result>` | error |
| ARK005 | `new HttpClient()` or raw `HttpClient` injection | warning |
| ARK006 | `Console.Write` / MEL `ILogger` in an Arkn-managed component | warning |
| ARK007 | `throw` inside a domain method | error |
| ARK008 | `catch` block without a `Result.Failure` return | warning |

**Output:**
```json
[
  {
    "rule": "ARK007",
    "line": 14,
    "message": "throw in domain method.",
    "suggestion": "Return Result.Failure(Error.Failure(...)) instead of throwing."
  }
]
```

---

### `docs_lookup`

Searches the Arkn documentation — works offline, content is embedded in the binary.

**Input:** `query` — term or question (e.g. `"how to register a job"`, `"Result.Match"`)

---

### `migrate_exception_to_result`

Converts a C# method that uses `throw` / `try-catch` into one that returns `Result<T>`, applying Arkn error-handling conventions.

**Input:** `code` — C# method body containing `throw` statements or `try-catch` blocks

**Example prompt:**
> "Convert this method to use Result instead of throwing"

**Input code:**
```csharp
public async Task<Order> GetOrderAsync(Guid id)
{
    var order = await _db.Orders.FindAsync(id);
    if (order == null)
        throw new NotFoundException($"Order {id} not found");
    return order;
}
```

**Output:**
```csharp
public async Task<Result<Order>> GetOrderAsync(Guid id)
{
    var order = await _db.Orders.FindAsync(id);
    if (order == null)
        return OrderErrors.NotFound(id);
    return order;
}
```

All `throw` expressions are replaced with `Result.Failure(...)` / typed error returns. `try-catch` blocks are unwrapped and failures are mapped to `Result.Failure`. The tool respects ARK007 and ARK008.

---

### `scaffold_minimal_api`

Generates a complete Minimal API endpoint group with `Result` → HTTP status mapping per operation type.

**Input:** `entity`, `namespace`, `operations` — list of `get | create | update | delete`

**Example prompt:**
> "Scaffold a Minimal API for the Order entity with get, create and delete"

**Output:**
```csharp
public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders").WithTags("Orders");

        group.MapGet("/{id:guid}", GetOrderAsync);
        group.MapPost("/", CreateOrderAsync);
        group.MapDelete("/{id:guid}", DeleteOrderAsync);

        return app;
    }

    private static async Task<IResult> GetOrderAsync(Guid id, IOrderService service)
    {
        var result = await service.GetAsync(id);
        return result.Match(
            onSuccess: value => Results.Ok(value),
            onFailure: err => err.ToHttpResult());
    }

    private static async Task<IResult> CreateOrderAsync(CreateOrderRequest req, IOrderService service)
    {
        var result = await service.CreateAsync(req);
        return result.Match(
            onSuccess: value => Results.Created($"/orders/{value.Id}", value),
            onFailure: err => err.ToHttpResult());
    }

    private static async Task<IResult> DeleteOrderAsync(Guid id, IOrderService service)
    {
        var result = await service.DeleteAsync(id);
        return result.Match(
            onSuccess: _ => Results.NoContent(),
            onFailure: err => err.ToHttpResult());
    }
}
```

HTTP status codes follow Arkn conventions: `NotFound` → 404, `Validation` → 422, `Conflict` → 409, `Unauthorized` → 401, generic `Failure` → 500.

---

### `project_health`

Scans a list of `.cs` files, checks ARK001–ARK008 across the entire codebase, and returns a scored health report.

**Input:** `files` — list of `.cs` file paths (globs accepted)

**Example prompt:**
> "Check the health of my domain layer"

**Output:**
```json
{
  "score": 74,
  "verdict": "Good",
  "violations": {
    "ARK007": 3,
    "ARK005": 1,
    "ARK008": 2
  },
  "details": [
    { "rule": "ARK007", "file": "src/Domain/Orders/OrderService.cs", "line": 42, "message": "throw in domain method." },
    { "rule": "ARK005", "file": "src/Infrastructure/HttpClients/LegacyClient.cs", "line": 8, "message": "Raw HttpClient injection detected." }
  ]
}
```

**Score table:**

| Score | Verdict |
|---|---|
| 90–100 | Excellent |
| 75–89 | Good |
| 50–74 | Fair |
| 0–49 | Needs Work |

---

### `list_arkn_types`

Lists all Arkn-specific constructs found in a set of source files: `[ArknErrors]` classes, `IArknJob` implementations, `ArknHttpClient` subclasses, and registered error codes.

**Input:** `files` — list of `.cs` file paths

**Example prompt:**
> "What Arkn types exist in my Infrastructure project?"

**Output:**
```json
{
  "errorGroups": ["PaymentErrors", "OrderErrors", "UserErrors"],
  "jobs": ["InvoiceSyncJob", "CustomerChurnJob"],
  "httpClients": ["StripeApiClient", "NotificationClient"],
  "errorCodes": [
    "Payment.NotFound", "Payment.Conflict",
    "Order.NotFound", "Order.InvalidAmount",
    "User.NotFound", "User.Unauthorized"
  ]
}
```

Useful for onboarding, audits, and prompting the AI with existing context before generating new code.

---

### `scaffold_domain_entity`

Generates a domain `Entity` or `AggregateRoot` with `ValueObject` members and a factory method returning `Result<T>`, following Arkn.Core conventions.

**Input:** `name`, `namespace`, `aggregateRoot` (bool), `properties` (list of name/type/valueObject)

**Example prompt:**
> "Scaffold a domain entity for Subscription with properties PlanId and StartDate"

**Output:**
```csharp
namespace MyApp.Domain.Subscriptions;

public sealed class Subscription : AggregateRoot
{
    public PlanId PlanId { get; private set; }
    public SubscriptionStartDate StartDate { get; private set; }

    private Subscription(PlanId planId, SubscriptionStartDate startDate)
    {
        PlanId    = planId;
        StartDate = startDate;
    }

    public static Result<Subscription> Create(Guid planId, DateTimeOffset startDate)
    {
        var planResult  = PlanId.Create(planId);
        var startResult = SubscriptionStartDate.Create(startDate);

        if (planResult.IsFailure)  return planResult.Error;
        if (startResult.IsFailure) return startResult.Error;

        return new Subscription(planResult.Value, startResult.Value);
    }
}

public sealed record PlanId
{
    public Guid Value { get; }
    private PlanId(Guid value) => Value = value;

    public static Result<PlanId> Create(Guid value)
    {
        if (value == Guid.Empty)
            return SubscriptionErrors.Invalid("PlanId cannot be empty.");
        return new PlanId(value);
    }
}
```

---

### `migrate_httpclient_to_arkn`

Converts raw `HttpClient` usage into a typed `ArknHttpClient` with DI registration, applying ARK005 conventions.

**Input:** `code` — C# class or method using `new HttpClient()` or injected `HttpClient`

**Example prompt:**
> "Migrate my PaymentGatewayService to use ArknHttpClient"

**Input code:**
```csharp
public class PaymentGatewayService
{
    private readonly HttpClient _http;

    public PaymentGatewayService(HttpClient http) => _http = http;

    public async Task<string> GetStatusAsync(Guid id)
    {
        var response = await _http.GetAsync($"/payments/{id}/status");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
```

**Output:**
```csharp
public sealed class PaymentGatewayClient(IArknHttp http)
    : ArknHttpClient(http, "https://api.payments.example.com")
{
    public Task<Result<PaymentStatusDto>> GetStatusAsync(Guid id)
        => GetAs<PaymentStatusDto>($"/payments/{id}/status");
}

// Program.cs
services.AddArknHttp<PaymentGatewayClient>("https://api.payments.example.com")
    .WithRetry(maxAttempts: 3)
    .WithDebugLogging(DebugLoggingOptions.Production);
```

All `EnsureSuccessStatusCode()` calls are removed. HTTP failures are surfaced as `Result.Failure` through Arkn's typed response handling.

---

## How it works

`arkn-mcp` runs as a local process via **stdio transport** — the AI assistant starts it automatically when it detects the configuration and calls its tools as needed during the conversation. No open port, no remote server, no authentication.

```
[AI Assistant]  →  stdin/stdout  →  [arkn-mcp process]
```

## Compatibility

| Assistant | Support |
|---|---|
| Claude Desktop | ✅ |
| Cursor | ✅ |
| VS Code + GitHub Copilot | ✅ |
| JetBrains AI | coming soon |
| Windsurf | coming soon |
