using Arkn.Notifications.Models;

namespace Arkn.Notifications.Abstractions;

/// <summary>
/// Contract for all notification destinations.
/// Implement to send notifications to Slack, email, PagerDuty, etc.
/// </summary>
public interface IArknNotifier
{
    /// <summary>Sends a notification. Should not throw — swallow and log internally.</summary>
    Task NotifyAsync(ArknNotification notification, CancellationToken cancellationToken = default);
}
