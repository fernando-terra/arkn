using Arkn.Notifications.Models;
using System.Text.Json;

namespace Arkn.Extensions.Notifications.Slack;

/// <summary>
/// Builds a Slack Block Kit JSON payload from an <see cref="ArknNotification"/>.
/// Zero external dependencies — uses System.Text.Json only.
/// </summary>
public static class SlackBlockBuilder
{
    private static readonly Dictionary<NotificationLevel, string> LevelEmoji = new()
    {
        [NotificationLevel.Info]     = "ℹ️",
        [NotificationLevel.Warning]  = "⚠️",
        [NotificationLevel.Error]    = "❌",
        [NotificationLevel.Critical] = "🚨",
    };

    private static readonly Dictionary<NotificationLevel, string> LevelColor = new()
    {
        [NotificationLevel.Info]     = "#36a64f",
        [NotificationLevel.Warning]  = "#ff9900",
        [NotificationLevel.Error]    = "#cc0000",
        [NotificationLevel.Critical] = "#7b0000",
    };

    /// <summary>
    /// Builds the Slack API payload as a JSON string ready to POST to an Incoming Webhook.
    /// </summary>
    public static string Build(ArknNotification notification, SlackNotifierOptions options)
    {
        var emoji = LevelEmoji.GetValueOrDefault(notification.Level, "🔔");
        var color = LevelColor.GetValueOrDefault(notification.Level, "#cccccc");

        var blocks = new List<object>
        {
            // Header
            new
            {
                type = "header",
                text = new { type = "plain_text", text = $"{emoji} {notification.Title}", emoji = true }
            },
            // Divider
            new { type = "divider" },
            // Body section
            new
            {
                type = "section",
                text = new { type = "mrkdwn", text = notification.Body }
            },
        };

        // Metadata fields
        if (notification.Metadata is { Count: > 0 })
        {
            var fields = notification.Metadata
                .Take(10)
                .Select(kv => new
                {
                    type = "mrkdwn",
                    text = $"*{kv.Key}*\n{kv.Value}"
                })
                .ToList<object>();

            blocks.Add(new { type = "section", fields });
        }

        // Log snippet
        if (notification.Metadata is not null &&
            notification.Metadata.TryGetValue("logs", out var logsObj) &&
            logsObj is string logs && !string.IsNullOrWhiteSpace(logs))
        {
            blocks.Add(new
            {
                type = "section",
                text = new { type = "mrkdwn", text = $"*Recent logs:*\n```{logs}```" }
            });
        }

        // Footer context
        blocks.Add(new
        {
            type = "context",
            elements = new object[]
            {
                new
                {
                    type = "mrkdwn",
                    text = $"*Source:* {notification.Source}  |  *Level:* {notification.Level}  |  {notification.Timestamp:yyyy-MM-dd HH:mm:ss} UTC"
                }
            }
        });

        // Compose payload
        var payload = new Dictionary<string, object>
        {
            ["blocks"] = blocks,
            ["attachments"] = new[]
            {
                new { color, fallback = $"{emoji} {notification.Title}: {notification.Body}" }
            }
        };

        if (!string.IsNullOrWhiteSpace(options.Channel))
            payload["channel"] = options.Channel;

        if (!string.IsNullOrWhiteSpace(options.Username))
            payload["username"] = options.Username;

        if (!string.IsNullOrWhiteSpace(options.IconEmoji))
            payload["icon_emoji"] = options.IconEmoji;

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented        = false,
        });
    }
}
