using Arkn.Notifications.Abstractions;
using Arkn.Notifications.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Arkn.Notifications.Extensions;

/// <summary>Extension methods for registering Arkn notification services.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="IArknNotifierRegistry"/> and any notifiers added via <paramref name="configure"/>.
    /// </summary>
    public static IServiceCollection AddArknNotifications(
        this IServiceCollection services,
        Action<ArknNotificationsBuilder>? configure = null)
    {
        var builder = new ArknNotificationsBuilder(services);
        configure?.Invoke(builder);

        services.AddSingleton<IArknNotifierRegistry>(sp =>
            new ArknNotifierRegistry(sp.GetServices<IArknNotifier>()));

        return services;
    }
}

/// <summary>Fluent builder for registering notifiers.</summary>
public sealed class ArknNotificationsBuilder
{
    private readonly IServiceCollection _services;
    internal ArknNotificationsBuilder(IServiceCollection services) => _services = services;

    /// <summary>Registers a notifier of the specified type as a singleton.</summary>
    public ArknNotificationsBuilder AddNotifier<TNotifier>()
        where TNotifier : class, IArknNotifier
    {
        _services.AddSingleton<TNotifier>();
        _services.AddSingleton<IArknNotifier>(sp => sp.GetRequiredService<TNotifier>());
        return this;
    }

    /// <summary>Registers a pre-existing notifier instance as a singleton.</summary>
    public ArknNotificationsBuilder AddNotifier(IArknNotifier instance)
    {
        _services.AddSingleton(instance);
        _services.AddSingleton<IArknNotifier>(instance);
        return this;
    }
}
