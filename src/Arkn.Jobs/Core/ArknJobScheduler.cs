using Arkn.Jobs.Abstractions;
using Arkn.Jobs.Models;
using Arkn.Jobs.Scheduling;
using Arkn.Logging.Abstractions;
using Microsoft.Extensions.Hosting;

namespace Arkn.Jobs.Core;

/// <summary>
/// <see cref="IHostedService"/> that drives the scheduling loop.
/// Polls at 1-second resolution and fires due jobs concurrently.
/// </summary>
public sealed class ArknJobScheduler : BackgroundService, IArknJobScheduler
{
    private readonly ArknJobRegistry  _registry;
    private readonly ArknJobRunner    _runner;
    private readonly ArknJobHistory   _history;
    private readonly IArknLogger      _logger;

    // Tracks the last fire time per job to avoid double-firing within the same minute
    private readonly Dictionary<string, DateTimeOffset> _lastFired = new();

    public ArknJobScheduler(
        ArknJobRegistry registry,
        ArknJobRunner   runner,
        ArknJobHistory  history,
        IArknLogger     logger)
    {
        _registry = registry;
        _runner   = runner;
        _history  = history;
        _logger   = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Info($"[ArknJobScheduler] Started. {_registry.Jobs.Count} job(s) registered.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;

            foreach (var job in _registry.Jobs)
            {
                if (!ShouldFire(job, now)) continue;

                _lastFired[job.JobName] = now;

                // Fire-and-forget per job; exceptions are handled inside ArknJobRunner
                _ = Task.Run(
                    () => _runner.RunAsync(job, now, stoppingToken),
                    stoppingToken);
            }

            try { await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken); }
            catch (OperationCanceledException) { break; }
        }

        _logger.Info("[ArknJobScheduler] Stopped.");
    }

    private bool ShouldFire(ArknJobOptions job, DateTimeOffset now)
    {
        var cron = CronParser.Parse(job.CronExpression);
        // Normalise to minute boundary
        var thisMinute = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Offset);

        // Check if this minute matches the cron
        if (!cron.Minutes.Contains(thisMinute.Minute)) return false;
        if (!cron.Hours.Contains(thisMinute.Hour))     return false;
        if (!cron.DaysOfMonth.Contains(thisMinute.Day)) return false;
        if (!cron.Months.Contains(thisMinute.Month))   return false;
        if (!cron.DaysOfWeek.Contains((int)thisMinute.DayOfWeek)) return false;

        // Avoid double-firing in the same minute
        if (_lastFired.TryGetValue(job.JobName, out var last) &&
            last.Year == thisMinute.Year && last.Month == thisMinute.Month &&
            last.Day  == thisMinute.Day  && last.Hour  == thisMinute.Hour &&
            last.Minute == thisMinute.Minute)
            return false;

        return true;
    }

    // ── IArknJobScheduler ─────────────────────────────────────────────────────

    public IReadOnlyList<ArknJobExecution> GetHistory(string jobName) =>
        _history.GetHistory(jobName);

    public IReadOnlyList<ArknJobExecution> GetAllHistory() =>
        _history.GetAllHistory();
}
