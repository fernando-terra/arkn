using Arkn.Notifications.Abstractions;
using Arkn.Notifications.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Arkn.Extensions.Notifications.Teams.Extensions;

/// <summary>Extension methods for registering the Teams notifier.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="TeamsNotifier"/> as an <see cref="IArknNotifier"/>.
    /// Call <c>AddArknNotifications()</c> first (or chain after it).
    /// </summary>
    public static ArknNotificationsBuilder AddTeamsNotifier(
        this ArknNotificationsBuilder builder,
        Action<TeamsNotifierOptions> configure)
    {
        var opts = new TeamsNotifierOptions();
        configure(opts);

        var http     = new HttpClient();
        var notifier = new TeamsNotifier(http, opts);

        builder.AddNotifier(notifier);
        return builder;
    }
}
