# Arkn.Analyzers

> **Conventions you can read. Patterns you can enforce.**

Roslyn analyzers that enforce Arkn patterns at compile time. Part of Arkn.Copilot — making your codebase ready for AI-assisted development.

## Install

```bash
dotnet add package Arkn.Analyzers
```

> This package is a development dependency — it doesn't ship with your application.

## Rules

| ID | Description | Severity |
|----|-------------|----------|
| **ARK001** | Domain methods must return `Result` or `Result<T>` — not throw | Warning |
| **ARK002** | Error codes must follow `Namespace.Reason` pattern (e.g. `User.NotFound`) | Warning |
| **ARK003** | `Result` / `Result<T>` must not be silently discarded | Warning |
| **ARK004** | `IArknJob.ExecuteAsync` must return `Task<Result>` or `Task<Result<T>>` | Error |

ARK001 includes a **code fix** that wraps the return type in `Result<T>` automatically.

## Copilot-ready

Also ships with IDE instruction files in the Arkn repository:
- `.github/copilot-instructions.md` — GitHub Copilot
- `.cursor/rules/arkn.mdc` — Cursor
- `CLAUDE.md` — Claude Code and AI agents

## Part of the Arkn ecosystem

[github.com/fernando-terra/arkn](https://github.com/fernando-terra/arkn) · [nuget.org/packages/Arkn.Analyzers](https://www.nuget.org/packages/Arkn.Analyzers)
