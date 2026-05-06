# Arkn.MCP

**Servidor MCP para assistentes de IA — ferramentas de scaffold e validação, sem necessidade de hosting.**

`Arkn.MCP` é um servidor [Model Context Protocol](https://modelcontextprotocol.io) distribuído como uma `dotnet tool`. Instale uma vez e qualquer assistente de IA compatível (Claude, Cursor, GitHub Copilot) ganha acesso às ferramentas reais do Arkn durante sua conversa.

## Instalação

```bash
dotnet tool install -g Arkn.MCP
```

## Configuração

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

## Ferramentas

### `scaffold_errors`

Gera um ErrorGroup tipado para um domínio.

**Input:** `domain` — nome do domínio (ex: `"Payment"`, `"User"`, `"Invoice"`)

**Exemplo de prompt:**
> "Crie o grupo de erros para o domínio de Payment"

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

Gera um `IArknJob` completo com registro de DI.

**Input:** `name`, `cron`, `description` (opcional)

**Output:**
```csharp
public sealed class InvoiceProcessorJob(IArknLogger logger) : IArknJob
{
    public async Task<Result> ExecuteAsync(CancellationToken ct)
    {
        logger.Info("Starting invoice processing...");
        // TODO: implementar
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

Analisa um trecho de C# e retorna violações das regras Arkn.

**Input:** `code` — trecho de código C#

**Regras verificadas:**

| Regra | Detecta | Severidade |
|---|---|---|
| ARK001 | Chamada inline `Error.*()` fora de um ErrorGroup | error |
| ARK002 | Código de erro sem o formato `Namespace.Reason` | error |
| ARK003 | `.Value` acessado diretamente em um `Result` | error |
| ARK004 | `IArknJob.ExecuteAsync` retornando `Task` em vez de `Task<Result>` | error |
| ARK005 | `new HttpClient()` ou injeção direta de `HttpClient` | warning |
| ARK006 | `Console.Write` / MEL `ILogger` em componente gerenciado pelo Arkn | warning |
| ARK007 | `throw` dentro de método de domínio | error |
| ARK008 | Bloco `catch` sem retorno de `Result.Failure` | warning |

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

Pesquisa a documentação do Arkn — funciona offline, o conteúdo está embutido no binário.

**Input:** `query` — termo ou pergunta (ex: `"how to register a job"`, `"Result.Match"`)

---

### `migrate_exception_to_result`

Converte um método C# que usa `throw` / `try-catch` para um que retorna `Result<T>`, aplicando as convenções de tratamento de erros do Arkn.

**Input:** `code` — corpo de método C# contendo `throw` ou blocos `try-catch`

**Exemplo de prompt:**
> "Converta este método para usar Result em vez de lançar exceções"

**Código de entrada:**
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

Todas as expressões `throw` são substituídas por retornos `Result.Failure(...)` / erros tipados. Blocos `try-catch` são desenrolados e falhas são mapeadas para `Result.Failure`. A ferramenta respeita ARK007 e ARK008.

---

### `scaffold_minimal_api`

Gera um grupo completo de endpoint Minimal API com mapeamento `Result` → status HTTP por tipo de operação.

**Input:** `entity`, `namespace`, `operations` — lista de `get | create | update | delete`

**Exemplo de prompt:**
> "Crie scaffold de uma Minimal API para a entidade Order com get, create e delete"

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

Os códigos HTTP seguem as convenções do Arkn: `NotFound` → 404, `Validation` → 422, `Conflict` → 409, `Unauthorized` → 401, `Failure` genérico → 500.

---

### `project_health`

Varre uma lista de arquivos `.cs`, verifica ARK001–ARK008 em toda a codebase e retorna um relatório de saúde com pontuação.

**Input:** `files` — lista de caminhos de arquivos `.cs` (globs aceitos)

**Exemplo de prompt:**
> "Verifique a saúde da minha camada de domínio"

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

**Tabela de pontuação:**

| Pontuação | Veredicto |
|---|---|
| 90–100 | Excellent |
| 75–89 | Good |
| 50–74 | Fair |
| 0–49 | Needs Work |

---

### `list_arkn_types`

Lista todos os construtores específicos do Arkn encontrados em um conjunto de arquivos de código: classes `[ArknErrors]`, implementações de `IArknJob`, subclasses de `ArknHttpClient` e códigos de erro registrados.

**Input:** `files` — lista de caminhos de arquivos `.cs`

**Exemplo de prompt:**
> "Quais tipos Arkn existem no meu projeto de Infrastructure?"

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

Útil para onboarding, auditorias e para fornecer contexto ao assistente de IA antes de gerar novo código.

---

### `scaffold_domain_entity`

Gera um `Entity` ou `AggregateRoot` de domínio com membros `ValueObject` e um método de fábrica retornando `Result<T>`, seguindo as convenções do `Arkn.Core`.

**Input:** `name`, `namespace`, `aggregateRoot` (bool), `properties` (lista de name/type/valueObject)

**Exemplo de prompt:**
> "Crie scaffold de uma entidade de domínio para Subscription com propriedades PlanId e StartDate"

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

Converte uso direto de `HttpClient` para um `ArknHttpClient` tipado com registro de DI, aplicando as convenções do ARK005.

**Input:** `code` — classe ou método C# usando `new HttpClient()` ou `HttpClient` injetado

**Exemplo de prompt:**
> "Migre meu PaymentGatewayService para usar ArknHttpClient"

**Código de entrada:**
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

Todas as chamadas `EnsureSuccessStatusCode()` são removidas. Falhas HTTP são expostas como `Result.Failure` através do tratamento tipado de resposta do Arkn.

---

## Como funciona

`arkn-mcp` roda como um processo local via **transporte stdio** — o assistente de IA o inicia automaticamente quando detecta a configuração e chama suas ferramentas conforme necessário durante a conversa. Sem porta aberta, sem servidor remoto, sem autenticação.

```
[Assistente de IA]  →  stdin/stdout  →  [processo arkn-mcp]
```

## Compatibilidade

| Assistente | Suporte |
|---|---|
| Claude Desktop | ✅ |
| Cursor | ✅ |
| VS Code + GitHub Copilot | ✅ |
| JetBrains AI | em breve |
| Windsurf | em breve |
