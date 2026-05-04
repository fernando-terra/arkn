using Arkn.Notifications.Models;

namespace Arkn.Notifications.Abstractions;

/// <summary>Marker interface for typed notification messages.</summary>
public interface IArknNotification
{
    string Title { get; }
    string Body { get; }
    NotificationLevel Level { get; }
    string Source { get; }
    DateTimeOffset Timestamp { get; }
}
