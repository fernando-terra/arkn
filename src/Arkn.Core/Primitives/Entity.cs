using Arkn.Core.Abstractions;

namespace Arkn.Core.Primitives;

/// <summary>
/// Base class for all domain entities.
/// Provides identity-based equality and audit timestamps.
/// </summary>
public abstract class Entity : IEntity
{
    /// <inheritdoc />
    public Guid Id { get; protected init; } = Guid.NewGuid();

    /// <summary>UTC timestamp when this entity was created.</summary>
    public DateTime CreatedAt { get; protected init; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of the last update, if any.</summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>Sets <see cref="UpdatedAt"/> to the current UTC time.</summary>
    protected void MarkUpdated() => UpdatedAt = DateTime.UtcNow;

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj is Entity entity && Id == entity.Id;
    }

    /// <inheritdoc />
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>Returns <c>true</c> when both entities share the same <see cref="Entity.Id"/>.</summary>
    public static bool operator ==(Entity? left, Entity? right) =>
        left?.Equals(right) ?? right is null;

    /// <summary>Returns <c>true</c> when the two entities have different identities.</summary>
    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}
