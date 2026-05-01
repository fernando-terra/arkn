using Arkn.Logging.Abstractions;
using Arkn.Logging.Core;
using Arkn.Logging.Models;
using Arkn.Logging.Sinks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arkn.Logging.Extensions;

/// <summary>
/// Fluent builder for configuring Arkn logging during DI setup.
/// </summary>
public sealed class ArknLoggingBuilder
{
    private readonly List<IArknLogSink> _sinks = [];
    private ArknLogLevel _minimumLevel = ArknLogLevel.Trace;

    internal ArknLoggingBuilder() { }

    internal IReadOnlyList<IArknLogSink> Sinks => _sinks;
    internal ArknLogLevel MinimumLevel => _minimumLevel;

    public ArknLoggingBuilder SetMinimumLevel(ArknLogLevel level)
    {
        _minimumLevel = level;
        return this;
    }

    public ArknLoggingBuilder AddConsoleSink()
    {
        _sinks.Add(new ConsoleLogSink());
        return this;
    }

    public ArknLoggingBuilder AddFileSink(string filePath)
    {
        _sinks.Add(new FileSink(filePath));
        return this;
    }

    /// <summary>
    /// Adds an <see cref="InMemoryLogSink"/> and registers it as a singleton so it can be
    /// injected directly for log inspection (e.g., by Arkn.Jobs).
    /// </summary>
    public ArknLoggingBuilder AddInMemorySink(int maxEntries = 10_000)
    {
        _sinks.Add(new InMemoryLogSink(maxEntries));
        return this;
    }

    public ArknLoggingBuilder AddSink(IArknLogSink sink)
    {
        _sinks.Add(sink);
        return this;
    }
}

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IArknLogger"/> and configured sinks into the DI container.
    /// </summary>
    public static IServiceCollection AddArknLogging(
        this IServiceCollection services,
        Action<ArknLoggingBuilder>? configure = null)
    {
        var builder = new ArknLoggingBuilder();
        configure?.Invoke(builder);

        // Register each sink individually so they can be resolved directly
        foreach (var sink in builder.Sinks)
        {
            services.AddSingleton(sink.GetType(), sink);
            services.AddSingleton<IArknLogSink>(sink);

            // InMemoryLogSink registered by concrete type for direct injection by Arkn.Jobs
            if (sink is InMemoryLogSink memSink)
                services.AddSingleton(memSink);
        }

        var minimumLevel = builder.MinimumLevel;

        services.AddSingleton<IArknLogger>(sp =>
        {
            var sinks = sp.GetServices<IArknLogSink>();
            var melFactory = sp.GetService<ILoggerFactory>();
            var factory = new ArknLoggerFactory(melFactory);
            return factory.Create(sinks, minimumLevel);
        });

        return services;
    }
}
