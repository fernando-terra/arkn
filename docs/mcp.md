# Arkn.MCP

**MCP Server para AI assistants — scaffolding e validação, sem hospedagem.**

`Arkn.MCP` é um [Model Context Protocol](https://modelcontextprotocol.io) server distribuído como `dotnet tool`. Instale uma vez e qualquer AI assistant compatível (Claude, Cursor, GitHub Copilot) passa a ter acesso a ferramentas reais do Arkn durante a conversa.

## Instalação

```bash
dotnet tool install -g Arkn.MCP
```

## Configuração por assistente

### Claude Desktop

`~/Library/Application Support/Claude/claude_desktop_config.json` (macOS) ou `%APPDATA%\Claude\claude_desktop_config.json` (Windows):

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

`.cursor/mcp.json` na raiz do projeto:

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

Gera um ErrorGroup tipado para um domínio.

**Input:** `domain` — nome do domínio (ex: `"Payment"`, `"User"`, `"Invoice"`)

**Exemplo de uso (conversa com AI):**
> "Cria os errors para o domínio de Pagamento"

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

Gera um `IArknJob` completo com registro no DI.

**Input:** `name`, `cron`, `description` (opcional)

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

Gera um `ArknHttpClient` tipado.

**Input:** `name`, `baseUrl`, `operations` (lista de nomes de operação)

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

Analisa um trecho de código C# e retorna violações das regras Arkn.

**Input:** `code` — trecho de código C#

**Regras verificadas:**

| Regra | Detecta | Severidade |
|---|---|---|
| ARK001 | `Error.*()` inline fora de um ErrorGroup | error |
| ARK002 | Error code sem `Namespace.Reason` (sem ponto) | error |
| ARK003 | `.Value` acessado diretamente em `Result` | error |
| ARK004 | `IArknJob.ExecuteAsync` retornando `Task` | error |
| ARK005 | `new HttpClient()` ou `HttpClient` injetado diretamente | warning |
| ARK006 | `Console.Write` / `ILogger` MEL em componente Arkn | warning |
| ARK007 | `throw` em método de domínio | error |
| ARK008 | `catch` sem `Result.Failure` de retorno | warning |

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

Busca na documentação do Arkn — funciona offline, conteúdo embutido no binário.

**Input:** `query` — termo ou pergunta (ex: `"como registrar um job"`, `"Result.Match"`)

---

## Como funciona

O `arkn-mcp` roda como processo local via **stdio transport** — o AI assistant o inicia automaticamente quando detecta a configuração e o chama conforme necessário durante a conversa. Não há porta aberta, não há servidor remoto, não há autenticação.

```
[AI Assistant]  →  stdin/stdout  →  [arkn-mcp process]
```

## Compatibilidade

| Assistente | Suporte |
|---|---|
| Claude Desktop | ✅ |
| Cursor | ✅ |
| VS Code + GitHub Copilot | ✅ |
| JetBrains AI | em breve |
| Windsurf | em breve |
