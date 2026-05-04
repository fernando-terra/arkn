using Arkn.Jobs.Abstractions;
using Arkn.Jobs.Models;
using Arkn.Logging.Abstractions;
using Arkn.Logging.Models;
using Arkn.Logging.Sinks;
using Arkn.Notifications.Abstractions;
using Arkn.Notifications.Models;
using Arkn.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Arkn.Jobs.Core;

/// <summary>
/// Executes a single job with timeout, linear retry (no Polly), scoped logging,
/// records the result in <see cref="ArknJobHistory"/>, and dispatches lifecycle notifications.
/// </summary>
public sealed class ArknJobRunner
{
    private readonly IServiceProvider _services;
    private readonly ArknJobHistory   _history;
    private readonly IArknLogger      _logger;
    private readonly InMemoryLogSink? _memorySink;
    private readonly ArknJobRegistry  _registry;

    /// <summary>Initializes the runner with its required services and optional in-memory log sink.</summary>
    public ArknJobRunner(
        IServiceProvider services,
        ArknJobHistory   history,
        IArknLogger      logger,
        ArknJobRegistry  registry,
        InMemoryLogSink? memorySink = null)
    {
        _services   = services;
        _history    = history;
        _logger     = logger;
        _registry   = registry;
        _memorySink = memorySink;
    }

    /// <summary>
    /// Runs the job described by <paramref name="options"/>, respecting timeout and retry settings.
    /// </summary>
    public async Task RunAsync(
        ArknJobOptions    options,
        DateTimeOffset    scheduledAt,
        CancellationToken hostToken)
    {
        var runId     = Guid.NewGuid();
        var jobName   = options.JobName;
        var startedAt = DateTimeOffset.UtcNow;

        _logger.Info($"[{jobName}] Starting run {runId} (scheduled {scheduledAt:HH:mm:ss})");

        // Fire Started notification
        await MaybeNotifyAsync(options, JobEvent.Started, runId, jobName,
            ArknJobStatus.Running, null, null, null, hostToken);

        ArknJobStatus finalStatus = ArknJobStatus.Failed;
        Error?        finalError  = null;

        for (int attempt = 1; attempt <= options.MaxAttempts; attempt++)
        {
            using var timeoutCts = options.Timeout.HasValue
                ? new CancellationTokenSource(options.Timeout.Value)
                : new CancellationTokenSource();

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                hostToken, timeoutCts.Token);

            var ctx = new ArknJobContext(runId, jobName, scheduledAt, linkedCts.Token, _logger);

            Result result;
            try
            {
                await using var scope = _services.CreateAsyncScope();
                var job = (IArknJob)scope.ServiceProvider.GetRequiredService(options.JobType);
                result = await job.ExecuteAsync(ctx);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                finalStatus = ArknJobStatus.TimedOut;
                finalError  = Error.Failure($"{jobName}.TimedOut",
                    $"Job '{jobName}' exceeded timeout of {options.Timeout}.");
                _logger.Warning($"[{jobName}] Run {runId} timed out (attempt {attempt}/{options.MaxAttempts})");
                break;
            }
            catch (Exception ex)
            {
                result = Result.Failure(
                    Error.Failure($"{jobName}.UnhandledException", ex.Message));
                _logger.Error($"[{jobName}] Unhandled exception on attempt {attempt}", ex);
            }

            if (result.IsSuccess)
            {
                finalStatus = ArknJobStatus.Succeeded;
                _logger.Info($"[{jobName}] Run {runId} succeeded (attempt {attempt})");
                break;
            }

            finalError = result.Error;

            if (attempt < options.MaxAttempts)
            {
                var delay = options.RetryDelay * attempt;
                _logger.Warning($"[{jobName}] Attempt {attempt} failed: {result.Error.Message}. Retrying in {delay.TotalSeconds}s…");
                await Task.Delay(delay, hostToken);
            }
            else
            {
                finalStatus = ArknJobStatus.Failed;
                _logger.Error($"[{jobName}] Run {runId} failed after {attempt} attempt(s): {result.Error.Message}");
            }
        }

        var finishedAt = DateTimeOffset.UtcNow;
        var duration   = finishedAt - startedAt;

        var logs = _memorySink?.GetEntries(runId.ToString())
            ?? (IReadOnlyList<LogEntry>)[];

        _history.Record(new ArknJobExecution(
            runId, jobName, finalStatus, startedAt, finishedAt, duration, finalError, logs));

        _memorySink?.Clear(runId.ToString());

        // Fire outcome notifications
        var outcomeEvent = finalStatus switch
        {
            ArknJobStatus.Succeeded => JobEvent.Succeeded,
            ArknJobStatus.TimedOut  => JobEvent.TimedOut,
            _                       => JobEvent.Failed,
        };

        await MaybeNotifyAsync(options, outcomeEvent, runId, jobName, finalStatus,
            finalError, duration, logs, hostToken);
    }

    private async Task MaybeNotifyAsync(
        ArknJobOptions           options,
        JobEvent                 @event,
        Guid                     runId,
        string                   jobName,
        ArknJobStatus            status,
        Error?                   error,
        TimeSpan?                duration,
        IReadOnlyList<LogEntry>? logs,
        CancellationToken        ct)
    {
        bool shouldNotify = options.NotifyOn.HasFlag(@event)
            || (@event is JobEvent.Failed or JobEvent.TimedOut
                && options.NotifyOn == JobEvent.None
                && _registry.GlobalFailureNotifierType is not null);

        if (!shouldNotify) return;

        var notifier = _services.GetService<IArknNotifierRegistry>();
        if (notifier is null) return;

        var level = @event switch
        {
            JobEvent.Started   => NotificationLevel.Info,
            JobEvent.Succeeded => NotificationLevel.Info,
            JobEvent.Failed    => NotificationLevel.Error,
            JobEvent.TimedOut  => NotificationLevel.Error,
            _                  => NotificationLevel.Warning,
        };

        var emoji = @event switch
        {
            JobEvent.Started   => "▶️",
            JobEvent.Succeeded => "✅",
            JobEvent.Failed    => "❌",
            JobEvent.TimedOut  => "⏱️",
            _                  => "🔔",
        };

        var title = $"{emoji} {jobName} — {status}";

        var bodyParts = new List<string>();
        if (duration.HasValue) bodyParts.Add($"Duration: {duration.Value:mm\\:ss}");
        bodyParts.Add($"Run: {runId.ToString()[..8]}…");
        if (error is not null) bodyParts.Add($"Error: {error.Message}");

        var logSnippet = logs is { Count: > 0 }
            ? string.Join("\n", logs.TakeLast(5).Select(l => $"[{l.Level}] {l.Message}"))
            : null;

        var metadata = new Dictionary<string, object?>
        {
            ["RunId"]   = runId.ToString(),
            ["Status"]  = status.ToString(),
            ["JobName"] = jobName,
        };
        if (duration.HasValue) metadata["Duration"] = duration.Value.ToString(@"mm\:ss");
        if (logSnippet is not null) metadata["logs"] = logSnippet;

        var notification = new ArknNotification(
            title,
            string.Join("  |  ", bodyParts),
            level,
            $"Arkn.Jobs/{jobName}",
            metadata);

        await notifier.DispatchAsync(notification, ct);
    }
}
