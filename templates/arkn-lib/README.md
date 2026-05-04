# ArknLib

A class library built with [Arkn](https://github.com/fernando-terra/arkn) primitives.

## Structure

```
Domain/          # Entities, Value Objects, Aggregate Roots
Application/     # Use cases (add when needed)
```

## Patterns used

- **Entity / AggregateRoot** from `Arkn.Core`
- **Result<T>** from `Arkn.Results` — no exceptions for expected failures
- **Arkn.Analyzers** enforce patterns at compile time (ARK001–ARK004)
