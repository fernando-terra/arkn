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
