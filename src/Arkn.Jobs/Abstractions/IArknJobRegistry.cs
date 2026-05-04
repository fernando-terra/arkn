using Arkn.Jobs.Models;

namespace Arkn.Jobs.Abstractions;

/// <summary>Read-only view of registered jobs.</summary>
public interface IArknJobRegistry
{
    /// <summary>Gets all registered job options.</summary>
    IReadOnlyList<ArknJobOptions> Jobs { get; }
}
