using Arkn.Notifications.Abstractions;
using Arkn.Notifications.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Arkn.Extensions.Notifications.Slack.Extensions;

/// <summary>Extension methods for registering the Slack notifier.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="SlackNotifier"/> as an <see cref="IArknNotifier"/>.
    /// Call <c>AddArknNotifications()</c> first (or chain after it).
    /// </summary>
    public static ArknNotificationsBuilder AddSlackNotifier(
        this ArknNotificationsBuilder builder,
        Action<SlackNotifierOptions> configure)
    {
        var opts = new SlackNotifierOptions();
        configure(opts);

        var http     = new HttpClient();
        var notifier = new SlackNotifier(http, opts);

        builder.AddNotifier(notifier);
        return builder;
    }
}
