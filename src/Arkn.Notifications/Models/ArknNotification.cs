namespace Arkn.Notifications.Models;

/// <summary>
/// A structured notification with a title, body, level, source tag and optional metadata.
/// </summary>
public sealed record ArknNotification(
    string Title,
    string Body,
    NotificationLevel Level,
    string Source,
    IReadOnlyDictionary<string, object?>? Metadata = null,
    DateTimeOffset? OccurredOn = null)
{
    /// <summary>UTC timestamp when this notification was created.</summary>
    public DateTimeOffset Timestamp { get; } = OccurredOn ?? DateTimeOffset.UtcNow;

    /// <summary>Creates an informational notification.</summary>
    public static ArknNotification Info(string title, string body, string source,
        IReadOnlyDictionary<string, object?>? metadata = null) =>
        new(title, body, NotificationLevel.Info, source, metadata);

    /// <summary>Creates a warning notification.</summary>
    public static ArknNotification Warning(string title, string body, string source,
        IReadOnlyDictionary<string, object?>? metadata = null) =>
        new(title, body, NotificationLevel.Warning, source, metadata);

    /// <summary>Creates an error notification.</summary>
    public static ArknNotification Error(string title, string body, string source,
        IReadOnlyDictionary<string, object?>? metadata = null) =>
        new(title, body, NotificationLevel.Error, source, metadata);

    /// <summary>Creates a critical notification.</summary>
    public static ArknNotification Critical(string title, string body, string source,
        IReadOnlyDictionary<string, object?>? metadata = null) =>
        new(title, body, NotificationLevel.Critical, source, metadata);
}
