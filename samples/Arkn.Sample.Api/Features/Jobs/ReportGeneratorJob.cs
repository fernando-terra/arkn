using Arkn.Jobs.Abstractions;
using Arkn.Jobs.Models;
using Arkn.Logging.Abstractions;

namespace Arkn.Sample.Api.Features.Jobs;

/// <summary>
/// Example job: generates a daily report.
///
/// Demonstrates:
/// - IArknJob contract (ExecuteAsync returns Task&lt;Result&gt; — enforced by ARK004)
/// - Structured logging via IArknLogger (injected, not static)
/// - Explicit success/failure via Result — no exceptions for expected failures
/// - Context metadata (RunId, ScheduledAt) available on ArknJobContext
/// </summary>
public sealed class ReportGeneratorJob(IArknLogger logger) : IArknJob
{
    public async Task<Result> ExecuteAsync(ArknJobContext ctx)
    {
        logger.Info($"[ReportGeneratorJob] Starting. RunId={ctx.RunId:N}");

        // Simulate fetching data
        await Task.Delay(200, ctx.CancellationToken);

        var reportLines = GenerateReport();

        if (reportLines.Count == 0)
        {
            logger.Warning("[ReportGeneratorJob] No data to report. Skipping.");
            // Return failure — will trigger .NotifyOn(JobEvent.Failed) if configured
            return Error.Failure("Report.NoData", "No records found for the reporting period.");
        }

        logger.Info($"[ReportGeneratorJob] Report generated. Lines={reportLines.Count}");

        // Simulate sending / saving report
        await Task.Delay(100, ctx.CancellationToken);

        logger.Info("[ReportGeneratorJob] Completed successfully.");
        return Result.Success();
    }

    private static List<string> GenerateReport()
    {
        // Simulated data — in a real job this would query a DB or external API
        return
        [
            "User alice@example.com: 3 sessions",
            "User bob@example.com: 1 session",
        ];
    }
}
