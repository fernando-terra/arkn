using Arkn.Notifications.Models;

namespace Arkn.Notifications.Abstractions;

/// <summary>Marker interface for typed notification messages.</summary>
public interface IArknNotification
{
    /// <summary>The notification title displayed to the recipient.</summary>
    string Title { get; }
    /// <summary>The main body text of the notification.</summary>
    string Body { get; }
    /// <summary>The severity level of this notification.</summary>
    NotificationLevel Level { get; }
    /// <summary>The source identifier indicating which component raised this notification.</summary>
    string Source { get; }
    /// <summary>UTC timestamp when this notification was created.</summary>
    DateTimeOffset Timestamp { get; }
}
