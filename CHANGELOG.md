# Changelog

All notable changes to Arkn are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
Versioning follows [Semantic Versioning](https://semver.org/).

---

## [Unreleased]

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

## [0.1.4] — 2026-04-27

### Added
- `Arkn.Http` — mTLS support: PFX, PEM, `X509Certificate2`, and certificate store (`WithClientCertificate`)

---

## [0.1.0] — 2026-04-20

### Added
- `Arkn.Core` — `IEntity`, `IAggregateRoot`, `IDomainEvent`, `IRepository<T,TId>`, `IUnitOfWork`
- `Arkn.Core` — `Entity`, `ValueObject`, `AggregateRoot` base classes
- `Arkn.Results` — `Result`, `Result<T>`, `Error`, `ErrorType`
- `Arkn.Results` — functional combinators: `Map`, `Bind`, `BindAsync`, `Match`, `MatchAsync`, `Tap`, `Ensure`
- `Arkn.Results` — implicit conversions from `T` and `Error` to `Result<T>`
- `Arkn.Results` — multiple-error support (`Result.Failure<T>(IEnumerable<Error>)`)
- GitHub Actions CI matrix (ubuntu-latest + windows-latest)

[Unreleased]: https://github.com/fernando-terra/arkn/compare/v0.1.6...HEAD
[0.1.6]: https://github.com/fernando-terra/arkn/compare/v0.1.5...v0.1.6
[0.1.5]: https://github.com/fernando-terra/arkn/compare/v0.1.4...v0.1.5
[0.1.4]: https://github.com/fernando-terra/arkn/compare/v0.1.0...v0.1.4
[0.1.0]: https://github.com/fernando-terra/arkn/releases/tag/v0.1.0
