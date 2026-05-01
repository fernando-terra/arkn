using Arkn.Jobs.Models;

namespace Arkn.Jobs.Abstractions;

/// <summary>Read-only view of registered jobs.</summary>
public interface IArknJobRegistry
{
    IReadOnlyList<ArknJobOptions> Jobs { get; }
}
