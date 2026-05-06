namespace Arkn.Extensions.Notifications.Teams;

/// <summary>Configuration for the Microsoft Teams Incoming Webhook notifier.</summary>
public sealed class TeamsNotifierOptions
{
    /// <summary>Full Incoming Webhook URL from the Teams channel connector settings.</summary>
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>Minimum level to send. Notifications below this level are silently dropped.</summary>
    public Arkn.Notifications.Models.NotificationLevel MinimumLevel { get; set; } =
        Arkn.Notifications.Models.NotificationLevel.Warning;

    /// <summary>Max log lines to include in the Teams card. Default: 5.</summary>
    public int MaxLogLines { get; set; } = 5;

    /// <summary>
    /// HTTP request timeout for the webhook call. Default: 10 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
}
