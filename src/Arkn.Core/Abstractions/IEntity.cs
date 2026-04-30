namespace Arkn.Core.Abstractions;

/// <summary>
/// Marker interface for domain entities.
/// An entity is defined by its identity, not its attributes.
/// </summary>
public interface IEntity
{
    /// <summary>Gets the unique identifier of the entity.</summary>
    Guid Id { get; }
}
