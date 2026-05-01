using Arkn.Jobs.Abstractions;
using Arkn.Jobs.Models;
using Arkn.Results;

namespace Arkn.Sample.Api.Features.Jobs;

/// <summary>
/// Demonstrates a minimal Arkn job.
/// Runs every minute (* * * * *) — for demo purposes only.
/// </summary>
public sealed class SampleJob : IArknJob
{
    public Task<Result> ExecuteAsync(ArknJobContext ctx)
    {
        ctx.Log($"SampleJob executing. RunId={ctx.RunId}, ScheduledAt={ctx.ScheduledAt:HH:mm:ss}");
        ctx.Log("Simulating work…");
        // Real work goes here
        ctx.Log("SampleJob completed successfully.");
        return Task.FromResult(Result.Success());
    }
}
