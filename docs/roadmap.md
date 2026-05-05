# Roadmap

## v0.1.0 — Current ✅

| Package | Status |
|---|---|
| `Arkn.Core` | ✅ Released |
| `Arkn.Results` | ✅ Released |
| `Arkn.Logging` | ✅ Released |
| `Arkn.Jobs` | ✅ Released |
| `Arkn.Notifications` | ✅ Released |
| `Arkn.Extensions.Notifications.Slack` | ✅ Released |
| `Arkn.Extensions.Notifications.Email` | ✅ Released |
| `Arkn.Extensions.Logging.Seq` | ✅ Released |
| `Arkn.Extensions.Logging.Elasticsearch` | ✅ Released |
| `Arkn.Analyzers` | ✅ Released |
| `Arkn.SourceGen` | ✅ Released |
| `Arkn.Templates` | ✅ Released |

## v0.2.0 — Planned 🔜

- `Arkn.CQRS` — command/query dispatcher abstractions
- `Arkn.Repository` — generic repository + unit of work implementations
- `Arkn.Extensions.EfCore` — Entity Framework Core adapter
- `Arkn.Extensions.MediatR` — MediatR adapter for CQRS
- `Arkn.Pagination` — cursor + offset pagination primitives
- NuGet publishing automation (tag → release)

## v0.3.0 — Future 💡

- `Arkn.Http` — typed HTTP client with Result-based error handling
- `Arkn.Resilience` — retry, circuit breaker (no Polly required)
- `Arkn.Caching` — Result-aware cache abstraction
- OpenTelemetry integration
