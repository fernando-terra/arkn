namespace Arkn.Core.Abstractions;

/// <summary>
/// Marker interface for aggregate roots.
/// An aggregate root is the entry point to a cluster of domain objects.
/// It is responsible for enforcing invariants across the aggregate.
/// </summary>
public interface IAggregateRoot : IEntity
{
    /// <summary>Gets the domain events raised by this aggregate.</summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>Clears all pending domain events.</summary>
    void ClearDomainEvents();
}
