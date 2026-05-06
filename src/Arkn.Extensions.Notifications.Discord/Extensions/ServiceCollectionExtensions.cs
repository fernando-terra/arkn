using Arkn.Notifications.Abstractions;
using Arkn.Notifications.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Arkn.Extensions.Notifications.Discord.Extensions;

/// <summary>Extension methods for registering the Discord notifier.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="DiscordNotifier"/> as an <see cref="IArknNotifier"/>.
    /// Call <c>AddArknNotifications()</c> first (or chain after it).
    /// </summary>
    public static ArknNotificationsBuilder AddDiscordNotifier(
        this ArknNotificationsBuilder builder,
        Action<DiscordNotifierOptions> configure)
    {
        var opts = new DiscordNotifierOptions();
        configure(opts);

        var http     = new HttpClient();
        var notifier = new DiscordNotifier(http, opts);

        builder.AddNotifier(notifier);
        return builder;
    }
}
