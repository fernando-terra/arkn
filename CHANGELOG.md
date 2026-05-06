# Changelog

All notable changes to Arkn are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
Versioning follows [Semantic Versioning](https://semver.org/).

---

## [Unreleased]

---

## [0.3.0] — 2026-05-06

### Added

#### `Arkn.Jobs`
- `IJobHistoryStore` / `InMemoryJobHistoryStore` — pluggable persistence for job execution records; circular-buffer, capacity-bounded per job, thread-safe
- `IDistributedJobLock` / `NoOpDistributedJobLock` — distributed-lock abstraction preventing concurrent execution across instances; `NoOp` default preserves single-instance behaviour; plug any implementation (e.g. Redis) via `WithDistributedLock<T>()`
- `IJobDlq` / `InMemoryJobDlq` — dead-letter queue for permanently failed jobs after all retry attempts are exhausted; supports per-job `ClearAsync`
- Full execution pipeline: lock → execute → retry → persist → DLQ → notify
- Fluent builder helpers: `WithInMemoryHistory()`, `WithInMemoryDlq()`, `WithDistributedLock<T>()`, `WithHistoryStore<T>()`
- `JobExecutionRecord` — immutable persistence record with `Duration` computed property

#### `Arkn.Analyzers`
- **ARK005** (`AvoidRawHttpClient`) — warns when `new HttpClient()` is used directly; recommends `AddArknHttp<T>()`
- **ARK006** (`PreferIArknLogger`) — warns when MEL `ILogger` or `Console` is used in Arkn components; recommends `IArknLogger`

#### `Arkn.Extensions.Notifications.Teams` *(new package)*
- `TeamsNotifier` — `IArknNotifier` implementation posting to Microsoft Teams via Incoming Webhook
- `TeamsCardBuilder` — Adaptive Cards 1.4 payload builder: level-coloured `Container` header (`good/warning/attention`), `FactSet` for metadata, monospace log snippet, footer with source + timestamp
- `TeamsNotifierOptions` — `WebhookUrl`, `MinimumLevel`, `MaxLogLines`, `Timeout`
- `AddTeamsNotifier()` — fluent extension on `ArknNotificationsBuilder`
- Zero external dependencies — `System.Text.Json` only

#### `Arkn.Extensions.Notifications.Discord` *(new package)*
- `DiscordNotifier` — `IArknNotifier` implementation posting to Discord via Webhook Embeds
- `DiscordEmbedBuilder` — Discord embed payload builder: level-coloured sidebar, inline metadata fields, code-block log snippet, ISO 8601 timestamp, footer with source
- `DiscordNotifierOptions` — `WebhookUrl`, `Username`, `AvatarUrl`, `MinimumLevel`, `MaxLogLines`, `Timeout`
- `AddDiscordNotifier()` — fluent extension on `ArknNotificationsBuilder`
- Zero external dependencies — `System.Text.Json` only

#### `Arkn.MCP`
- `migrate_exception_to_result` — converts `throw`/`try-catch` patterns to `Result`-based returns
- `migrate_httpclient_to_arkn` — migrates raw `HttpClient` usage to typed `ArknHttpClient`
- `project_health` — analyses a project for ARK001–ARK008 violations and returns a health summary
- `list_arkn_types` — lists all Arkn types available in a given namespace or assembly
- `scaffold_minimal_api` — scaffolds a complete Minimal API endpoint with `Result` matching
- `scaffold_domain_entity` — scaffolds a domain entity implementing `IAggregateRoot` with error group

### Changed
- All packages bumped to `v0.3.0`

### Fixed
- N/A

---

## [0.2.0] — 2026-05-06

### Added
- `Arkn.MCP` — MCP Server as `dotnet tool`; exposes `scaffold_errors`, `scaffold_job`, `scaffold_http_client`, `validate_pattern` (ARK001–ARK008), and `docs_lookup` to any compatible AI assistant (Claude, Cursor, Copilot)
- `Arkn.SourceGen` — Roslyn incremental source generator; `[ArknErrors]` + `[ArknErrorCode]` generate `Error` factory methods
- `Arkn.Extensions.Logging.Seq` — Seq sink via HTTP + CLEF, zero Serilog dependency (.NET 10+)
- `Arkn.Extensions.Logging.Elasticsearch` — Elasticsearch Bulk API sink, zero NEST dependency (.NET 10+)
- `Arkn.Extensions.Notifications.Email` — SMTP (native) and SendGrid (HTTP, zero SDK) email notifier
- ARK005–ARK008 validation rules (raw HttpClient, ILogger over IArknLogger, throw in domain, swallowed catch)

### Changed
- All packages bumped to `v0.2.0`

---

## [0.1.6] — 2026-05-05

### Added
- `Arkn.SourceGen` — Roslyn incremental source generator; `[ArknErrors]` + `[ArknErrorCode]` generate `Error` factory methods, eliminating boilerplate
- `Arkn.Extensions.Logging.Seq` — Seq sink via HTTP + CLEF (JSON Lines), zero Serilog dependency (.NET 10+)
- `Arkn.Extensions.Logging.Elasticsearch` — Elasticsearch Bulk API sink, zero NEST dependency (.NET 10+)
- `Arkn.Extensions.Notifications.Email` — email notifier with SMTP (native) and SendGrid (HTTP, zero SDK); HTML + plain-text support

---

## [0.1.5] — 2026-04-30

### Added
- `Arkn.Http` — `WithApiKey` interceptor for static header auth
- `Arkn.Http` — `WithRateLimitHandling` with automatic 429 + Retry-After support
- `Arkn.Http` — `WithResponseCaching` with in-memory GET cache (5 min TTL)
- `Arkn.Http` — shorthand methods: `GetAs<T>`, `PostAs<T>`, `Delete`
- `Arkn.Http` — OAuth2 Client Credentials flow (`WithClientCredentials`) with `InMemoryTokenStore`
- `Arkn.Http` — `BearerTokenInterceptor` for pluggable token injection
- `Arkn.Http` — `DebugLoggingOptions` presets: `Development`, `Production`, `FailuresOnly`
- `Arkn.Logging` — ANSI console sink with auto-disable on redirect
- `Arkn.Logging` — rotating file sink with daily pattern and JSON format
- `Arkn.Extensions.Logging.ApplicationInsights` — Application Insights sink for `Arkn.Logging`
- `Arkn.Notifications` — pluggable notifier abstraction with fan-out to N channels
- `Arkn.Extensions.Notifications.Slack` — Slack via Incoming Webhook + Block Kit, zero SDKs
- `Arkn.Jobs` — in-process cron scheduler with retry, timeout, and `Result<T>` contract
- `Arkn.Jobs` — `NotifyOn(JobEvent)` failure hooks wired into `Arkn.Notifications`
- `Arkn.Analyzers` — ARK001–ARK004 enforcing Arkn patterns at compile time
- `Arkn.Templates` — `dotnet new` templates: `arkn-api`, `arkn-job`, `arkn-lib`

### Changed
- License: MIT → Apache 2.0

---

[0.3.0]: https://github.com/fernando-terra/arkn/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/fernando-terra/arkn/compare/v0.1.6...v0.2.0
[0.1.6]: https://github.com/fernando-terra/arkn/compare/v0.1.5...v0.1.6
[0.1.5]: https://github.com/fernando-terra/arkn/releases/tag/v0.1.5
