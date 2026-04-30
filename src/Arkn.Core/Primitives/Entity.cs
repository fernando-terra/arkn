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

    protected void MarkUpdated() => UpdatedAt = DateTime.UtcNow;

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj is Entity entity && Id == entity.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? left, Entity? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}
