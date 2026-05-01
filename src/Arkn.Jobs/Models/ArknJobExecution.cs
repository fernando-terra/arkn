using Arkn.Logging.Models;
using Arkn.Results;

namespace Arkn.Jobs.Models;

/// <summary>
/// Immutable record of a completed (or running) job execution.
/// Captured at the end of each run and stored in <c>ArknJobHistory</c>.
/// </summary>
public sealed record ArknJobExecution(
    Guid RunId,
    string JobName,
    ArknJobStatus Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    TimeSpan? Duration,
    Error? Error,
    IReadOnlyList<LogEntry> Logs);
