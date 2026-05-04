namespace Arkn.Extensions.Notifications.Slack;

/// <summary>Configuration for the Slack Incoming Webhook notifier.</summary>
public sealed class SlackNotifierOptions
{
    /// <summary>Full Incoming Webhook URL from Slack App settings.</summary>
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>Override the default channel (e.g. "#alerts"). Leave empty to use the webhook default.</summary>
    public string? Channel { get; set; }

    /// <summary>Bot display name in Slack. Leave empty to use the webhook default.</summary>
    public string? Username { get; set; }

    /// <summary>Emoji for the bot icon (e.g. ":robot_face:"). Leave empty for webhook default.</summary>
    public string? IconEmoji { get; set; }

    /// <summary>Minimum level to send. Notifications below this level are silently dropped.</summary>
    public Arkn.Notifications.Models.NotificationLevel MinimumLevel { get; set; } =
        Arkn.Notifications.Models.NotificationLevel.Warning;

    /// <summary>Max log lines to include in the Slack message. Default: 5.</summary>
    public int MaxLogLines { get; set; } = 5;
}
