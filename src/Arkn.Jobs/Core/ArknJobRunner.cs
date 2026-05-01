using Arkn.Jobs.Abstractions;
using Arkn.Jobs.Models;
using Arkn.Logging.Abstractions;
using Arkn.Logging.Core;
using Arkn.Logging.Sinks;
using Arkn.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Arkn.Jobs.Core;

/// <summary>
/// Executes a single job with timeout, linear retry (no Polly), scoped logging,
/// and records the result in <see cref="ArknJobHistory"/>.
/// </summary>
public sealed class ArknJobRunner
{
    private readonly IServiceProvider _services;
    private readonly ArknJobHistory _history;
    private readonly IArknLogger _logger;
    private readonly InMemoryLogSink? _memorySink;

    public ArknJobRunner(
        IServiceProvider services,
        ArknJobHistory history,
        IArknLogger logger,
        InMemoryLogSink? memorySink = null)
    {
        _services   = services;
        _history    = history;
        _logger     = logger;
        _memorySink = memorySink;
    }

    /// <summary>
    /// Runs the job described by <paramref name="options"/>, respecting timeout and retry settings.
    /// </summary>
    public async Task RunAsync(
        ArknJobOptions options,
        DateTimeOffset scheduledAt,
        CancellationToken hostToken)
    {
        var runId    = Guid.NewGuid();
        var jobName  = options.JobName;
        var startedAt = DateTimeOffset.UtcNow;

        _logger.Info($"[{jobName}] Starting run {runId} (scheduled {scheduledAt:HH:mm:ss})");

        ArknJobStatus finalStatus = ArknJobStatus.Failed;
        Error?        finalError  = null;

        for (int attempt = 1; attempt <= options.MaxAttempts; attempt++)
        {
            // Build per-attempt CancellationToken (timeout + host)
            using var timeoutCts = options.Timeout.HasValue
                ? new CancellationTokenSource(options.Timeout.Value)
                : new CancellationTokenSource();

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                hostToken, timeoutCts.Token);

            var ctx = new ArknJobContext(
                runId, jobName, scheduledAt, linkedCts.Token, _logger);

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
                break; // Never retry a timed-out run
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

        // Collect scoped logs from InMemoryLogSink
        var logs = _memorySink?.GetEntries(runId.ToString())
            ?? (IReadOnlyList<Arkn.Logging.Models.LogEntry>)[];

        _history.Record(new ArknJobExecution(
            runId, jobName, finalStatus, startedAt, finishedAt, duration, finalError, logs));

        // Clean up this run's entries from InMemoryLogSink to avoid unbounded growth
        _memorySink?.Clear(runId.ToString());
    }
}
