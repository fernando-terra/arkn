# Roadmap

## v0.4.0 — Em desenvolvimento

### Arkn.Repository <Badge type="warning" text="planejado" />

Implementação genérica do padrão Repository com suporte a EF Core e Dapper, seguindo os contratos de `Arkn.Core`.

- `IRepository<T, TId>` com operações CRUD retornando `Result<T>`
- Suporte a `IUnitOfWork` com transações explícitas
- Extensões para EF Core (`Arkn.Extensions.Repository.EfCore`)
- Extensões para Dapper (`Arkn.Extensions.Repository.Dapper`)

### Arkn.CQRS <Badge type="warning" text="planejado" />

Mediator leve para o padrão CQRS, sem dependências externas.

- `ICommand<TResult>` e `IQuery<TResult>` com retorno `Result<T>`
- Pipeline de behaviors composáveis (validação, logging, cache)
- Despacho em memória — sem MediatR, sem reflexão excessiva
- Integração com `Arkn.Analyzers` para verificação em tempo de compilação

---

## v0.3.0 — Lançado

- **Arkn.Jobs:** persistência de histórico (`IJobHistoryStore`), lock distribuído (`IDistributedJobLock`), Dead-Letter Queue (`IJobDlq`)
- **Arkn.Notifications.Teams:** notificações via Adaptive Cards 1.4, zero SDK externo
- **Arkn.Notifications.Discord:** notificações via Discord Embeds + Webhook, zero SDK externo
- **Arkn.Analyzers:** regras ARK005 (raw HttpClient) e ARK006 (MEL ILogger) adicionadas
- **Arkn.MCP:** servidor MCP nativo com `scaffold_errors`, `scaffold_job`, `scaffold_http_client`, `validate_pattern`, `docs_lookup`, `migrate_exception_to_result`, `scaffold_minimal_api`, `project_health`, `list_arkn_types`, `scaffold_domain_entity`, `migrate_httpclient_to_arkn`

## v0.2.0 — Lançado

- **Arkn.Http:** cliente HTTP tipado com retry, timeout, OAuth2 e mTLS
- **Arkn.Logging:** sinks para Seq e Elasticsearch adicionados
- **Arkn.Notifications:** notificadores de Email (SMTP + SendGrid)
- **Arkn.SourceGen:** gerador de código para grupos de erros via `[ArknErrors]`
- **Arkn.Templates:** templates `dotnet new` para API, Worker e Library

## v0.1.x — Lançado

- **Arkn.Core:** primitivos de domínio (`Entity`, `AggregateRoot`, `ValueObject`)
- **Arkn.Results:** padrão `Result<T>`, tipos de `Error`, API funcional (`Map`, `Bind`, `Match`, `Tap`, `Ensure`)
- **Arkn.Jobs:** agendador cron com retry e timeout
- **Arkn.Logging:** logging estruturado com sinks Console, File e InMemory
- **Arkn.Notifications:** abstração de notificações + notificador Slack
- **Arkn.Analyzers:** regras ARK001–ARK004, ARK007, ARK008
