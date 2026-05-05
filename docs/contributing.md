# Contributing

See [CONTRIBUTING.md](https://github.com/fernando-terra/arkn/blob/main/CONTRIBUTING.md) for the full guide.

## Quick reference

### Branch naming

| Prefix | Purpose |
|---|---|
| `feat/` | New feature |
| `fix/` | Bug fix |
| `docs/` | Documentation |
| `chore/` | Tooling, build |
| `test/` | Tests only |

### Commit format (Conventional Commits)

```
feat(results): add TapAsync extension method
fix(jobs): prevent double-fire within the same minute
docs(logging): add Elasticsearch sink example
```

### Rules

- Open an issue before implementing non-trivial features
- Every PR must include tests
- `Arkn.Core` and `Arkn.Results` must remain **zero external dependency**
- Minimum 1 code review approval before merge
