using Arkn.Logging.Abstractions;
using Arkn.Logging.Core;

namespace Arkn.Jobs.Models;

/// <summary>
/// Runtime context injected into every job execution.
/// Provides identity, timing, cancellation, and a scoped logger.
/// </summary>
public sealed class ArknJobContext
{
    /// <summary>Unique identifier for this specific run.</summary>
    public Guid RunId { get; }

    /// <summary>The registered name of the job.</summary>
    public string JobName { get; }

    /// <summary>When this run was scheduled to execute.</summary>
    public DateTimeOffset ScheduledAt { get; }

    /// <summary>Cancellation token combining the host's lifetime and the job's timeout.</summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>Logger pre-scoped to this run's <see cref="RunId"/>.</summary>
    public IArknLogger Logger { get; }

    private readonly IArknLogContext _logContext;

    /// <summary>Initializes a new job execution context.</summary>
    public ArknJobContext(
        Guid runId,
        string jobName,
        DateTimeOffset scheduledAt,
        CancellationToken cancellationToken,
        IArknLogger logger)
    {
        RunId = runId;
        JobName = jobName;
        ScheduledAt = scheduledAt;
        CancellationToken = cancellationToken;
        Logger = logger;
        _logContext = ArknLogContext.ForScope(runId.ToString());
    }

    /// <summary>Logs an informational message scoped to this run's <see cref="RunId"/>.</summary>
    public void Log(string message) => Logger.Info(message, _logContext);

    /// <summary>Logs a warning scoped to this run.</summary>
    public void LogWarning(string message) => Logger.Warning(message, _logContext);

    /// <summary>Logs an error scoped to this run.</summary>
    public void LogError(string message, Exception? ex = null) => Logger.Error(message, ex, _logContext);
}
