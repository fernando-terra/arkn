using Arkn.Jobs.Models;

namespace Arkn.Jobs.Abstractions;

/// <summary>Abstraction over the background scheduling loop.</summary>
public interface IArknJobScheduler
{
    /// <summary>Returns the history of past executions for a given job, most recent first.</summary>
    IReadOnlyList<ArknJobExecution> GetHistory(string jobName);

    /// <summary>Returns the history across all registered jobs.</summary>
    IReadOnlyList<ArknJobExecution> GetAllHistory();
}
