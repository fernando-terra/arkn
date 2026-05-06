using Arkn.Jobs.Abstractions;
using Arkn.Jobs.Core;
using Arkn.Jobs.Dlq;
using Arkn.Jobs.Locking;
using Arkn.Jobs.Persistence;
using Arkn.Logging.Abstractions;
using Arkn.Logging.Sinks;
using Microsoft.Extensions.DependencyInjection;

namespace Arkn.Jobs.Extensions;

/// <summary>Extension methods for registering Arkn.Jobs infrastructure.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Arkn.Jobs infrastructure and all jobs configured via <paramref name="configure"/>.
    /// </summary>
    public static IServiceCollection AddArknJobs(
        this IServiceCollection services,
        Action<ArknJobsBuilder>? configure = null)
    {
        var registry = new ArknJobRegistry();
        services.AddSingleton(registry);
        services.AddSingleton<ArknJobHistory>();

        services.AddSingleton<ArknJobRunner>(sp =>
        {
            var logger       = sp.GetRequiredService<IArknLogger>();
            var history      = sp.GetRequiredService<ArknJobHistory>();
            var memorySink   = sp.GetService<InMemoryLogSink>(); // optional
            var reg          = sp.GetRequiredService<ArknJobRegistry>();
            var historyStore = sp.GetService<IJobHistoryStore>();
            var distLock     = sp.GetService<IDistributedJobLock>() ?? new NoOpDistributedJobLock();
            var dlq          = sp.GetService<IJobDlq>();
            return new ArknJobRunner(sp, history, logger, reg, memorySink, historyStore, distLock, dlq);
        });

        services.AddSingleton<ArknJobScheduler>();
        services.AddHostedService(sp => sp.GetRequiredService<ArknJobScheduler>());
        services.AddSingleton<IArknJobScheduler>(sp => sp.GetRequiredService<ArknJobScheduler>());
        services.AddSingleton<IArknJobRegistry>(sp => sp.GetRequiredService<ArknJobRegistry>());

        var builder = new ArknJobsBuilder(registry, services);
        configure?.Invoke(builder);

        return services;
    }
}
