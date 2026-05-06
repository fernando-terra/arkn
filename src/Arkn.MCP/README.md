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

## Part of the Arkn ecosystem

[github.com/fernando-terra/arkn](https://github.com/fernando-terra/arkn) · [nuget.org/packages/Arkn.MCP](https://www.nuget.org/packages/Arkn.MCP)
