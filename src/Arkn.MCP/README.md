# Arkn.MCP

Model Context Protocol server for Arkn — scaffolding, migration, and validation tools for AI assistants.

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
      "command": "arkn-mcp"
    }
  }
}
```

## Available tools

### Scaffolding

#### `scaffold_errors`
Generates a complete Arkn error group class for a given domain.
```
Input:  domain = "User"
Output: [ArknErrors] public static partial class UserErrors { ... }
```

#### `scaffold_job`
Generates an `IArknJob` implementation and DI registration snippet.
```
Input:  name = "InvoiceProcessor", cron = "0 2 * * *"
Output: public sealed class InvoiceProcessorJob : IArknJob { ... }
```

#### `scaffold_http_client`
Generates a typed `ArknHttpClient` with methods from operation names.
```
Input:  name = "Payment", baseUrl = "https://api.pay.com", operations = "GetPayment,CreatePayment"
Output: public sealed class PaymentClient : ArknHttpClient { ... }
```

#### `scaffold_minimal_api` *(v0.3.0)*
Scaffolds a complete Minimal API endpoint with `Result` pattern matching.
```
Input:  entity = "Order", operations = "Get,Create,Cancel"
Output: app.MapGet/MapPost/MapDelete with full Result.Match routing
```

#### `scaffold_domain_entity` *(v0.3.0)*
Scaffolds a domain entity implementing `IAggregateRoot` with error group.
```
Input:  name = "Order", properties = "CustomerId:Guid,TotalAmount:decimal"
Output: public sealed class Order : Entity, IAggregateRoot { ... } + OrderErrors class
```

### Validation & Analysis

#### `validate_pattern`
Analyses a C# code snippet and returns violations with line numbers and fix suggestions.
Rules checked: ARK001–ARK008 (see [Arkn.Analyzers](https://www.nuget.org/packages/Arkn.Analyzers) for full list).

#### `project_health` *(v0.3.0)*
Analyses a project for ARK001–ARK008 violations and returns a health summary with counts per rule and top offending files.

### Migration

#### `migrate_exception_to_result` *(v0.3.0)*
Converts `throw`/`try-catch` patterns in C# code to `Result`-based returns following Arkn conventions.

#### `migrate_httpclient_to_arkn` *(v0.3.0)*
Migrates raw `HttpClient` usage to typed `ArknHttpClient` with interceptors.

### Documentation

#### `docs_lookup`
Searches embedded Arkn documentation by keyword. No HTTP required.
Topics: `result`, `error`, `iarknjob`, `iarknlogger`, `addarknhttp`, `analyzers`, `sourcegen`, `templates`

#### `list_arkn_types` *(v0.3.0)*
Lists all Arkn types available in a given namespace or assembly with their summary docs.

---

### Refactoring *(v0.3.1)*

#### `refactor_to_result`
Converts C# code that uses exception-based error handling into the Arkn `Result<T>` pattern. Automatically:
- Generates an `[ArknErrors]` class for the detected domain
- Replaces `throw new XException(...)` with `DomainErrors.Reason(...)` calls
- Changes `void` returns to `Result` and `T` returns to `Result<T>`
- Converts rethrows and empty catch blocks

```
Input:  C# code with try/catch or throw statements, optional domain name
Output: ErrorGroup class + refactored method with changes log
```

#### `migrate_exception`
Converts a specific `catch` block into a `Result.Failure` return with the semantically correct Arkn `ErrorType`. Analyses the exception type (`ArgumentNullException` → `Validation`, `KeyNotFoundException` → `NotFound`, etc.) and preserves any existing logging.

```
Input:  A catch block, optional error code (Namespace.Reason)
Output: Migrated catch block + Why-this-ErrorType explanation
```

---

### Review & Explanation *(v0.3.1)*

#### `explain_error`
Explains an Arkn error code in natural language. Returns the error type, meaning, recommended HTTP status, when to use it, and a complete usage example (ErrorGroup definition, domain method, API endpoint handler).

```
Input:  Error code in Namespace.Reason format, e.g. "User.NotFound"
Output: Markdown explanation with usage examples and HTTP mapping
```

#### `review_pattern`
Like `validate_pattern` but returns a human-readable markdown code review instead of raw JSON. Includes a score (0–100), grade (A–F), grouped violations with descriptions, and actionable next steps.

```
Input:  C# source code, optional file name for the review header
Output: Markdown review with score, grade, violations and recommendations
```

---

### Test Generation *(v0.3.1)*

#### `generate_tests`
Generates xUnit unit tests for a method returning `Result` or `Result<T>`. Produces a success test and one failure test per error code. Uses NSubstitute for mocking. Tests follow the Arrange/Act/Assert pattern.

```
Input:  Method signature, class name, comma-separated error codes
Output: Complete xUnit test class with success + failure test cases
```

## Part of the Arkn ecosystem

[github.com/fernando-terra/arkn](https://github.com/fernando-terra/arkn) · [nuget.org/packages/Arkn.MCP](https://www.nuget.org/packages/Arkn.MCP)
