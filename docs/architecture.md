# Arkn Architecture

## Philosophy

Arkn is built around three principles:

1. **Zero lock-in** — Core packages have no external dependencies. You bring your own tools.
2. **Composability** — Each package is independent. Use only what you need.
3. **Explicitness** — Patterns are explicit and inspectable, not magic.

## Package Dependency Graph

```
Arkn.Core          (no deps)
    └── Arkn.Results      (→ Arkn.Core)
    └── Arkn.Repository   (→ Arkn.Core) [planned]
    └── Arkn.CQRS         (→ Arkn.Core) [planned]

Arkn.Extensions.EfCore    (→ Arkn.Repository + EF Core) [planned]
Arkn.Extensions.MediatR   (→ Arkn.CQRS + MediatR)       [planned]
```

## Layers

### `Arkn.Core`
Pure abstractions — interfaces and base classes only. Depends on nothing but the .NET runtime.
- `IEntity`, `IAggregateRoot`, `IDomainEvent`
- `IRepository<TAggregate, TId>`, `IUnitOfWork`
- `Entity`, `ValueObject`, `AggregateRoot` base classes

### `Arkn.Results`
Result Pattern implementation. Depends only on `Arkn.Core`.
- `Result`, `Result<T>`, `Error`, `ErrorType`
- Functional combinators: `Map`, `Bind`, `Match`, `Tap`, `Ensure`
- Full async support via `ResultExtensions`
