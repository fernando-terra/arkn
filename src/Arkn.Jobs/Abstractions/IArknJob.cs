using Arkn.Jobs.Models;
using Arkn.Results;

namespace Arkn.Jobs.Abstractions;

/// <summary>
/// Contract for all Arkn background jobs.
/// Implement this interface and register via <c>AddArknJobs()</c>.
/// </summary>
public interface IArknJob
{
    /// <summary>
    /// Executes the job. Return <see cref="Result.Success()"/> on success
    /// or <see cref="Result.Failure(Error)"/> to signal a handled failure.
    /// Unhandled exceptions are also caught and treated as failures.
    /// </summary>
    Task<Result> ExecuteAsync(ArknJobContext ctx);
}
