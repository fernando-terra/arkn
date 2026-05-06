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
