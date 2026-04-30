# Changelog

All notable changes to Arkn are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
Versioning follows [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [0.1.0] — 2026-04-30

### Added
- `Arkn.Core` — `IEntity`, `IAggregateRoot`, `IDomainEvent`, `IRepository<T,TId>`, `IUnitOfWork`
- `Arkn.Core` — `Entity`, `ValueObject`, `AggregateRoot` base classes
- `Arkn.Results` — `Result`, `Result<T>`, `Error`, `ErrorType`
- `Arkn.Results` — functional combinators: `Map`, `Bind`, `BindAsync`, `Match`, `MatchAsync`, `Tap`, `Ensure`
- `Arkn.Results` — implicit conversions from `T` and `Error` to `Result<T>`
- `Arkn.Results` — multiple-error support (`Result.Failure<T>(IEnumerable<Error>)`)
- `Arkn.Sample.Api` — minimal API demonstrating Result Pattern with Minimal APIs
- GitHub Actions CI matrix (ubuntu + windows)
