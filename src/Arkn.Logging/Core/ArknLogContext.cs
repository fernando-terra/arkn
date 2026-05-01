using Arkn.Logging.Abstractions;

namespace Arkn.Logging.Core;

/// <summary>
/// Immutable, composable log context backed by a dictionary.
/// </summary>
public sealed class ArknLogContext : IArknLogContext
{
    private readonly IReadOnlyDictionary<string, object?> _properties;

    /// <summary>An empty context with no scope and no properties.</summary>
    public static readonly ArknLogContext Empty = new(null, new Dictionary<string, object?>());

    private ArknLogContext(string? scope, IReadOnlyDictionary<string, object?> properties)
    {
        Scope = scope;
        _properties = properties;
    }

    /// <inheritdoc />
    public string? Scope { get; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> Properties => _properties;

    /// <summary>Creates a context with the given scope name.</summary>
    public static ArknLogContext ForScope(string scope) =>
        new(scope, new Dictionary<string, object?>());

    /// <inheritdoc />
    public IArknLogContext With(string key, object? value)
    {
        var next = new Dictionary<string, object?>(_properties) { [key] = value };
        return new ArknLogContext(Scope, next);
    }
}
