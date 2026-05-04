using Arkn.Jobs.Core;
using Arkn.Jobs.Models;
using Arkn.Notifications.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Arkn.Jobs.Extensions;

/// <summary>Per-job fluent configuration builder.</summary>
public sealed class ArknJobBuilder<TJob>
{
    private readonly ArknJobOptions _options;
    private readonly IServiceCollection _services;

    internal ArknJobBuilder(ArknJobOptions options, IServiceCollection services)
    {
        _options  = options;
        _services = services;
        _services.AddScoped(typeof(TJob));
    }

    /// <summary>Sets a human-readable name for this job.</summary>
    public ArknJobBuilder<TJob> WithName(string name)
    {
        _options.JobName = name;
        return this;
    }

    /// <summary>Sets a description for this job.</summary>
    public ArknJobBuilder<TJob> WithDescription(string description)
    {
        _options.Description = description;
        return this;
    }

    /// <summary>Sets the maximum execution time before the job is cancelled.</summary>
    public ArknJobBuilder<TJob> WithTimeout(TimeSpan timeout)
    {
        _options.Timeout = timeout;
        return this;
    }

    /// <summary>Configures retry attempts and optional delay between attempts.</summary>
    public ArknJobBuilder<TJob> WithRetry(int maxAttempts, TimeSpan? retryDelay = null)
    {
        _options.MaxAttempts = maxAttempts;
        if (retryDelay.HasValue) _options.RetryDelay = retryDelay.Value;
        return this;
    }

    /// <summary>Configures which job lifecycle events trigger notifications for this job.</summary>
    public ArknJobBuilder<TJob> NotifyOn(JobEvent events)
    {
        _options.NotifyOn = events;
        return this;
    }
}

/// <summary>Top-level fluent builder for registering multiple jobs.</summary>
public sealed class ArknJobsBuilder
{
    private readonly ArknJobRegistry _registry;
    private readonly IServiceCollection _services;

    internal ArknJobsBuilder(ArknJobRegistry registry, IServiceCollection services)
    {
        _registry = registry;
        _services = services;
    }

    /// <summary>Registers a job with the given cron expression.</summary>
    public ArknJobBuilder<TJob> Add<TJob>(string cronExpression)
        where TJob : class, Arkn.Jobs.Abstractions.IArknJob
    {
        var options = new ArknJobOptions
        {
            JobName        = typeof(TJob).Name,
            CronExpression = cronExpression,
            JobType        = typeof(TJob),
        };

        _registry.Register(options);
        return new ArknJobBuilder<TJob>(options, _services);
    }

    /// <summary>
    /// Registers a global notifier that fires on Failed and TimedOut for all jobs
    /// that don't have explicit NotifyOn set.
    /// </summary>
    public ArknJobsBuilder OnFailure<TNotifier>()
        where TNotifier : class, IArknNotifier
    {
        _services.AddSingleton<IArknNotifier, TNotifier>();
        _registry.SetGlobalFailureNotifier(typeof(TNotifier));
        return this;
    }
}
