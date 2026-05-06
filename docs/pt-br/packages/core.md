# Arkn.Core

Abstrações e primitivos do core. **Zero dependências externas.**

```bash
dotnet add package Arkn.Core
```

## Abstrações

| Interface | Finalidade |
|---|---|
| `IEntity` | Marcador para entidades de domínio (possui `Id: Guid`) |
| `IAggregateRoot` | Estende `IEntity` com suporte a eventos de domínio |
| `IDomainEvent` | Marcador para eventos de domínio |
| `IRepository<T, TId>` | Contrato genérico de repositório |
| `IUnitOfWork` | Contrato de Unit of Work |

## Classes base

### Entity

```csharp
public sealed class Order : Entity
{
    public string CustomerId { get; private set; }
    // ...
}
```

Fornece igualdade baseada em identidade, `CreatedAt`, `UpdatedAt` e `MarkUpdated()`.

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

Estende `Entity` com `Raise(IDomainEvent)`, `DomainEvents` e `ClearDomainEvents()`.

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

Fornece igualdade estrutural (comparado por componentes, não por referência).
