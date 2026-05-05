# Arkn.Core

Core abstractions and primitives. **Zero external dependencies.**

```bash
dotnet add package Arkn.Core
```

## Abstractions

| Interface | Purpose |
|---|---|
| `IEntity` | Marker for domain entities (has `Id: Guid`) |
| `IAggregateRoot` | Extends `IEntity` with domain event support |
| `IDomainEvent` | Marker for domain events |
| `IRepository<T, TId>` | Generic repository contract |
| `IUnitOfWork` | Unit of Work contract |

## Base classes

### Entity

```csharp
public sealed class Order : Entity
{
    public string CustomerId { get; private set; }
    // ...
}
```

Provides identity-based equality, `CreatedAt`, `UpdatedAt`, and `MarkUpdated()`.

### AggregateRoot

```csharp
public sealed class Order : AggregateRoot
{
    public static Order Create(string customerId)
    {
        var order = new Order { CustomerId = customerId };
        order.Raise(new OrderCreatedEvent(order.Id));
        return order;
    }
}
```

Extends `Entity` with `Raise(IDomainEvent)`, `DomainEvents`, and `ClearDomainEvents()`.

### ValueObject

```csharp
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency) { ... }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

Provides structural equality (compared by components, not reference).
