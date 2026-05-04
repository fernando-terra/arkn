namespace Arkn.Core.Primitives;

/// <summary>
/// Base class for value objects.
/// Value objects are immutable and compared by their structural components.
/// </summary>
public abstract class ValueObject
{
    /// <summary>Returns the components used for equality comparison.</summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        return GetEqualityComponents()
            .SequenceEqual(((ValueObject)obj).GetEqualityComponents());
    }

    /// <inheritdoc />
    public override int GetHashCode() =>
        GetEqualityComponents()
            .Aggregate(0, (hash, component) =>
                HashCode.Combine(hash, component?.GetHashCode() ?? 0));

    /// <summary>Returns <c>true</c> when both value objects are structurally equal.</summary>
    public static bool operator ==(ValueObject? left, ValueObject? right) =>
        left?.Equals(right) ?? right is null;

    /// <summary>Returns <c>true</c> when the two value objects are not structurally equal.</summary>
    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}
