using Arkn.Jobs.Abstractions;
using Arkn.Jobs.Models;
using Arkn.Results;

namespace ArknJob.Jobs;

/// <summary>
/// Starter job — rename, move to a feature folder, and implement your logic.
/// </summary>
public sealed class SampleJob : IArknJob
{
    public async Task<Result> ExecuteAsync(ArknJobContext ctx)
    {
        ctx.Log("SampleJob started");

        // TODO: inject your services via constructor and implement business logic
        await Task.Delay(100, ctx.CancellationToken);

        ctx.Log("SampleJob completed");
        return Result.Success();
    }
}
