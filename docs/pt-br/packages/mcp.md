# Arkn.MCP

Servidor MCP distribuído como uma `dotnet tool` — expõe ferramentas de scaffold e validação para assistentes de IA (Claude, Cursor, GitHub Copilot) via **Model Context Protocol** (transporte stdio).

```bash
dotnet tool install -g Arkn.MCP
```

> Para instruções completas de instalação e configuração, consulte o [**guia do Servidor MCP**](/pt-br/mcp).

## Ferramentas disponíveis

### `scaffold_errors`
Gera uma classe de grupo de erros Arkn completa para um domínio.

```
Input:  domain = "User"
Output: [ArknErrors] public static partial class UserErrors { ... }
```

Gera `NotFound`, `Invalid`, `Conflict`, `Unauthorized` — todos seguindo a convenção de nomenclatura **ARK002** automaticamente.

---

### `scaffold_job`
Gera uma implementação de `IArknJob` e um trecho de registro de DI.

```
Input:  name = "InvoiceProcessor", cron = "0 2 * * *", description = "Processes pending invoices"
Output: public sealed class InvoiceProcessorJob : IArknJob { ... }
        + registro AddArknJobs() com retry, timeout, NotifyOn
```

---

### `scaffold_http_client`
Gera um `ArknHttpClient` tipado com métodos inferidos a partir dos nomes de operação.

```
Input:  name = "Payment", baseUrl = "https://api.pay.com", operations = "GetPayment,CreatePayment,CancelPayment"
Output: public sealed class PaymentClient : ArknHttpClient { ... }
        + registro AddArknHttp<PaymentClient>()
```

---

### `validate_pattern`
Analisa um trecho de código C# e retorna violações com números de linha e sugestões de correção.

**Regras verificadas:** ARK001–ARK008 (veja [Arkn.Analyzers](/pt-br/packages/analyzers) para a lista completa)

```json
[
  { "rule": "ARK002", "line": 12, "message": "...", "suggestion": "..." }
]
```

---

### `docs_lookup`
Pesquisa a documentação do Arkn embutida por palavra-chave. Sem HTTP necessário.

```
Input:  query = "result pattern"
Output: # Arkn.Results — Result Pattern ...
```

Tópicos: `result`, `error`, `iarknjob`, `iarknlogger`, `addarknhttp`, `analyzers`, `sourcegen`, `templates`
