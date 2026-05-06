namespace Arkn.Extensions.Notifications.Discord;

/// <summary>Configuration for the Discord Webhook notifier.</summary>
public sealed class DiscordNotifierOptions
{
    /// <summary>Full Discord Webhook URL from channel settings.</summary>
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>Override the webhook's default bot display name.</summary>
    public string? Username { get; set; }

    /// <summary>URL of an avatar image to override the webhook's default icon.</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Minimum level to send. Notifications below this level are silently dropped.</summary>
    public Arkn.Notifications.Models.NotificationLevel MinimumLevel { get; set; } =
        Arkn.Notifications.Models.NotificationLevel.Warning;

    /// <summary>Max log lines to include in the embed. Default: 5.</summary>
    public int MaxLogLines { get; set; } = 5;

    /// <summary>HTTP request timeout for the webhook call. Default: 10 seconds.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
}
