# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added
- Initial project scaffolding

---

## [0.1.0] — 2026-04-30

### Added
- `Arkn.Core` — `IResult`, `IResult<T>`, `IError`, `ErrorType`
- `Arkn.Results` — `Result`, `Result<T>`, `Error` with full functional API
  - Error factory methods: `Failure`, `NotFound`, `Validation`, `Conflict`, `Unauthorized`
  - Functional combinators: `Map`, `Bind`, `Match`
  - Multiple-error support for validation scenarios
  - Implicit conversions: `T → Result<T>`, `Error → Result<T>`
- Unit tests for all public APIs (`Arkn.Core.Tests`, `Arkn.Results.Tests`)
- Sample minimal API (`Arkn.Sample.Api`)
- GitHub Actions CI (Ubuntu + Windows)
- GitHub Actions Release workflow (pack on tag push)
- Documentation: architecture overview, Results API reference

[Unreleased]: https://github.com/fernando-terra/arkn/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/fernando-terra/arkn/releases/tag/v0.1.0
