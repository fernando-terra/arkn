# Contributing to Arkn

Thank you for your interest in contributing! This document covers everything you need to get started.

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git

### Setup
```bash
git clone https://github.com/fernando-terra/arkn.git
cd arkn
dotnet restore
dotnet build
dotnet test
```

## Branch Conventions

| Prefix | Purpose |
|---|---|
| `feat/` | New feature |
| `fix/` | Bug fix |
| `docs/` | Documentation changes |
| `refactor/` | Code restructuring |
| `chore/` | Tooling, build, deps |
| `test/` | Test additions only |

Example: `feat/result-tap-async`

## Commit Messages

We follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <subject>

[optional body]
[optional footer]
```

Examples:
```
feat(results): add TapAsync extension method
fix(core): entity equality when Id is Guid.Empty
docs(results): add railway-oriented programming example
```

## Pull Request Process

1. **Open an issue first** for non-trivial changes — discuss before implementing.
2. Fork the repo and create your branch from `main`.
3. Write or update tests for every change.
4. Ensure `dotnet test` passes locally.
5. Update `CHANGELOG.md` under `[Unreleased]`.
6. Submit the PR — fill in the PR template.
7. At least **one code review approval** is required before merging.

## Key Rules

- `Arkn.Core` and `Arkn.Results` must have **zero external NuGet dependencies**.
- Every public API needs XML doc comments.
- No breaking changes without a major version bump and migration guide.
