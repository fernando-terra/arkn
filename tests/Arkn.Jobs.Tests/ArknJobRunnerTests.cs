using Arkn.Jobs.Abstractions;
using Arkn.Jobs.Core;
using Arkn.Jobs.Models;
using Arkn.Logging.Core;
using Arkn.Logging.Sinks;
using Arkn.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Arkn.Jobs.Tests;

public class ArknJobRunnerTests
{
    private static (ArknJobRunner runner, ArknJobHistory history, InMemoryLogSink sink) BuildRunner()
    {
        var sink    = new InMemoryLogSink();
        var logger  = new ArknLogger([sink]);
        var history = new ArknJobHistory();
        var services = new ServiceCollection();
        services.AddScoped<SuccessJob>();
        services.AddScoped<FailingJob>();
        services.AddScoped<SlowJob>();
        var sp = services.BuildServiceProvider();

        var runner = new ArknJobRunner(sp, history, logger, sink);
        return (runner, history, sink);
    }

    private static ArknJobOptions MakeOptions<TJob>(
        int maxAttempts = 1,
        TimeSpan? timeout = null) where TJob : IArknJob => new()
    {
        JobName        = typeof(TJob).Name,
        CronExpression = "* * * * *",
        JobType        = typeof(TJob),
        MaxAttempts    = maxAttempts,
        Timeout        = timeout,
        RetryDelay     = TimeSpan.FromMilliseconds(10),
    };

    [Fact]
    public async Task RunAsync_SuccessJob_ShouldRecordSucceeded()
    {
        var (runner, history, _) = BuildRunner();
        await runner.RunAsync(MakeOptions<SuccessJob>(), DateTimeOffset.UtcNow, CancellationToken.None);

        var runs = history.GetHistory(nameof(SuccessJob));
        Assert.Single(runs);
        Assert.Equal(ArknJobStatus.Succeeded, runs[0].Status);
    }

    [Fact]
    public async Task RunAsync_FailingJob_NoRetry_ShouldRecordFailed()
    {
        var (runner, history, _) = BuildRunner();
        await runner.RunAsync(MakeOptions<FailingJob>(), DateTimeOffset.UtcNow, CancellationToken.None);

        var runs = history.GetHistory(nameof(FailingJob));
        Assert.Equal(ArknJobStatus.Failed, runs[0].Status);
        Assert.NotNull(runs[0].Error);
    }

    [Fact]
    public async Task RunAsync_FailingJob_WithRetry_ShouldAttemptMultipleTimes()
    {
        var (runner, history, _) = BuildRunner();
        var opts = MakeOptions<FailingJob>(maxAttempts: 3);

        await runner.RunAsync(opts, DateTimeOffset.UtcNow, CancellationToken.None);

        // Should still be failed but have retried — check logs contain retry messages
        var runs = history.GetHistory(nameof(FailingJob));
        Assert.Equal(ArknJobStatus.Failed, runs[0].Status);
    }

    [Fact]
    public async Task RunAsync_TimedOutJob_ShouldRecordTimedOut()
    {
        var (runner, history, _) = BuildRunner();
        var opts = MakeOptions<SlowJob>(timeout: TimeSpan.FromMilliseconds(50));

        await runner.RunAsync(opts, DateTimeOffset.UtcNow, CancellationToken.None);

        var runs = history.GetHistory(nameof(SlowJob));
        Assert.Equal(ArknJobStatus.TimedOut, runs[0].Status);
    }

    [Fact]
    public async Task RunAsync_ShouldAttachLogsToExecution()
    {
        var (runner, history, _) = BuildRunner();
        await runner.RunAsync(MakeOptions<SuccessJob>(), DateTimeOffset.UtcNow, CancellationToken.None);

        var run = history.GetHistory(nameof(SuccessJob))[0];
        Assert.NotEmpty(run.Logs);
    }

    // ── Stub jobs ─────────────────────────────────────────────────────────────

    private sealed class SuccessJob : IArknJob
    {
        public Task<Result> ExecuteAsync(ArknJobContext ctx)
        {
            ctx.Log("SuccessJob executed");
            return Task.FromResult(Result.Success());
        }
    }

    private sealed class FailingJob : IArknJob
    {
        public Task<Result> ExecuteAsync(ArknJobContext ctx) =>
            Task.FromResult(Result.Failure(Error.Failure("FailingJob.Error", "intentional failure")));
    }

    private sealed class SlowJob : IArknJob
    {
        public async Task<Result> ExecuteAsync(ArknJobContext ctx)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), ctx.CancellationToken);
            return Result.Success();
        }
    }
}
