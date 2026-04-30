namespace Arkn.Core.Abstractions;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something that happened in the domain.
/// Use a mediator or event dispatcher to publish these — Arkn does not prescribe one.
/// </summary>
public interface IDomainEvent
{
    /// <summary>The moment this event occurred (UTC).</summary>
    DateTime OccurredOn { get; }
}
