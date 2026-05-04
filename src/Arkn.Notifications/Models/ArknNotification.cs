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
    public DateTimeOffset Timestamp { get; } = OccurredOn ?? DateTimeOffset.UtcNow;

    public static ArknNotification Info(string title, string body, string source,
        IReadOnlyDictionary<string, object?>? metadata = null) =>
        new(title, body, NotificationLevel.Info, source, metadata);

    public static ArknNotification Warning(string title, string body, string source,
        IReadOnlyDictionary<string, object?>? metadata = null) =>
        new(title, body, NotificationLevel.Warning, source, metadata);

    public static ArknNotification Error(string title, string body, string source,
        IReadOnlyDictionary<string, object?>? metadata = null) =>
        new(title, body, NotificationLevel.Error, source, metadata);

    public static ArknNotification Critical(string title, string body, string source,
        IReadOnlyDictionary<string, object?>? metadata = null) =>
        new(title, body, NotificationLevel.Critical, source, metadata);
}
