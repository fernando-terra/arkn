using Arkn.Jobs.Abstractions;
using Arkn.Jobs.Core;
using Arkn.Logging.Abstractions;
using Arkn.Logging.Sinks;
using Microsoft.Extensions.DependencyInjection;

namespace Arkn.Jobs.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Arkn.Jobs infrastructure and all jobs configured via <paramref name="configure"/>.
    /// </summary>
    public static IServiceCollection AddArknJobs(
        this IServiceCollection services,
        Action<ArknJobsBuilder>? configure = null)
    {
        // Core infrastructure
        services.AddSingleton<ArknJobRegistry>();
        services.AddSingleton<ArknJobHistory>();

        services.AddSingleton<ArknJobRunner>(sp =>
        {
            var logger     = sp.GetRequiredService<IArknLogger>();
            var history    = sp.GetRequiredService<ArknJobHistory>();
            var memorySink = sp.GetService<InMemoryLogSink>(); // optional
            return new ArknJobRunner(sp, history, logger, memorySink);
        });

        services.AddSingleton<ArknJobScheduler>();
        services.AddHostedService(sp => sp.GetRequiredService<ArknJobScheduler>());
        services.AddSingleton<IArknJobScheduler>(sp => sp.GetRequiredService<ArknJobScheduler>());
        services.AddSingleton<IArknJobRegistry>(sp => sp.GetRequiredService<ArknJobRegistry>());

        // Run user configuration
        var registry = new ArknJobRegistry();
        var builder  = new ArknJobsBuilder(registry, services);
        configure?.Invoke(builder);

        return services;
    }
}
