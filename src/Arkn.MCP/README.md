# Arkn.MCP

Model Context Protocol server for Arkn — scaffolding and validation tools for AI assistants.

## Installation

```bash
dotnet tool install -g Arkn.MCP
```

## Configuration

### Claude Desktop (`claude_desktop_config.json`)
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

### Cursor (`~/.cursor/mcp.json`)
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

### VS Code / GitHub Copilot (`.vscode/mcp.json`)
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

## Available tools

### `scaffold_errors`
Generates a complete Arkn error group for a domain.

**Input:** `domain` — e.g. `"User"`, `"Payment"`, `"Invoice"`

**Example prompt:** *"Use scaffold_errors to create errors for the Order domain"*

**Output:**
```csharp
[ArknErrors]
public static partial class OrderErrors
{
    [ArknErrorCode("NotFound", "Order was not found")]
    public static partial Error NotFound(string? detail = null);
    // ...
}
```

---

### `scaffold_job`
Generates an `IArknJob` implementation and DI registration.

**Inputs:** `name`, `cron`, `description` (optional)

**Example prompt:** *"Scaffold a job named InvoiceProcessor that runs at 2am daily"*

---

### `scaffold_http_client`
Generates a typed `ArknHttpClient` with inferred methods.

**Inputs:** `name`, `baseUrl`, `operations` (comma-separated)

**Example prompt:** *"Create an HTTP client for PaymentGateway at https://api.pay.com with GetPayment, CreatePayment, CancelPayment"*

---

### `validate_pattern`
Validates a C# code snippet against Arkn rules.

**Input:** `code` — C# source code

**Rules checked:**

| Rule | Detects |
|---|---|
| ARK001 | Domain method returning void/Task without Result |
| ARK002 | Error code not following Namespace.Reason |
| ARK003 | .Value accessed without IsSuccess check |
| ARK004 | ExecuteAsync not returning Task\<Result\> |
| ARK005 | new HttpClient() instead of AddArknHttp |
| ARK006 | Console.Write or MEL ILogger instead of IArknLogger |
| ARK007 | throw new in domain code |
| ARK008 | Empty catch swallowing exceptions |

---

### `docs_lookup`
Searches Arkn documentation by keyword.

**Input:** `query` — e.g. `"result pattern"`, `"job registration"`, `"error code naming"`

**Topics:** result, error, iarknjob, iarknlogger, addarknhttp, analyzers, sourcegen, templates

---

### `migrate_exception_to_result`
Refactors a C# method using throw/try-catch into one that returns `Result` or `Result<T>`.

**Input:** `code` — C# method source code

---

### `migrate_httpclient_to_arkn`
Refactors code using raw `HttpClient` into a typed `ArknHttpClient` with Result-based error handling.

**Input:** `code` — C# source code with HttpClient usage

---

### `project_health`
Analyzes multiple C# files and returns an aggregate health report with score and top issues.

**Input:** `files` — array of C# source file contents
**Returns:** `{ score, totalViolations, byRule, topIssues, verdict }`

---

### `list_arkn_types`
Scans source files and returns an inventory of Arkn types (error groups, jobs, HTTP clients, error codes).

**Input:** `files` — array of C# source file contents

---

### `scaffold_minimal_api`
Generates Minimal API endpoint groups with full Result→HTTP mapping.

**Input:** `resource`, `operations` (get,getall,create,update,delete), `errorGroup` (optional)

---

### `scaffold_domain_entity`
Generates a domain Entity or AggregateRoot with Value Objects using Arkn.Core.

**Input:** `name`, `valueObjects` (comma-separated), `isAggregate` (bool)

---

## Running locally (development)

```bash
cd src/Arkn.MCP
dotnet run
```
