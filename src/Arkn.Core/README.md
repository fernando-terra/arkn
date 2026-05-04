# Arkn.Core

> **Conventions you can read. Patterns you can enforce.**

Domain primitives for clean software design — zero external dependencies.

## What's included

| Type | Description |
|------|-------------|
| `IEntity<TId>` | Base interface for domain entities with a typed identity |
| `IValueObject` | Marker for value objects with structural equality |
| `IAggregateRoot` | Marks the root of a domain aggregate |
| `IError` | Structured error contract: `Code`, `Message`, `Type`, `Metadata` |
| `ErrorType` | Enum: `Failure`, `NotFound`, `Validation`, `Conflict`, `Unauthorized`, `Forbidden` |

## Install

```bash
dotnet add package Arkn.Core
```

## Quick example

```csharp
public class Order : IEntity<Guid>, IAggregateRoot
{
    public Guid Id { get; }
    public IReadOnlyList<OrderItem> Items { get; }

    public Order(Guid id) => Id = id;
}

public record Money(decimal Amount, string Currency) : IValueObject;
```

## Part of the Arkn ecosystem

[github.com/fernando-terra/arkn](https://github.com/fernando-terra/arkn) · [nuget.org/packages/Arkn.Core](https://www.nuget.org/packages/Arkn.Core)
