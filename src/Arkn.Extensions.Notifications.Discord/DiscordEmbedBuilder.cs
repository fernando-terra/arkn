using Arkn.Notifications.Models;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Arkn.Extensions.Notifications.Discord;

/// <summary>
/// Builds a Discord Webhook JSON payload with an embed from an <see cref="ArknNotification"/>.
/// Zero external dependencies — uses System.Text.Json only.
/// </summary>
public static class DiscordEmbedBuilder
{
    private static readonly Dictionary<NotificationLevel, string> LevelEmoji = new()
    {
        [NotificationLevel.Info]     = "ℹ️",
        [NotificationLevel.Warning]  = "⚠️",
        [NotificationLevel.Error]    = "❌",
        [NotificationLevel.Critical] = "⛔",
    };

    // Discord embed sidebar colours (decimal RGB)
    private static readonly Dictionary<NotificationLevel, int> LevelColor = new()
    {
        [NotificationLevel.Info]     = 0x36a64f,   // green
        [NotificationLevel.Warning]  = 0xff9900,   // amber
        [NotificationLevel.Error]    = 0xcc0000,   // red
        [NotificationLevel.Critical] = 0x7b0000,   // dark red
    };

    /// <summary>
    /// Builds the Discord Webhook payload as a JSON string ready to POST.
    /// </summary>
    public static string Build(ArknNotification notification, DiscordNotifierOptions options)
    {
        var emoji = LevelEmoji.GetValueOrDefault(notification.Level, "🔔");
        var color = LevelColor.GetValueOrDefault(notification.Level, 0x888888);

        // ── Embed fields (metadata) ────────────────────────────────────────
        var fields = new List<object>();

        if (notification.Metadata is { Count: > 0 })
        {
            foreach (var kv in notification.Metadata.Where(k => k.Key != "logs").Take(25))
            {
                fields.Add(new
                {
                    name   = kv.Key,
                    value  = kv.Value?.ToString() ?? "\u200B", // zero-width space keeps field visible
                    inline = true,
                });
            }
        }

        // Log snippet as a separate non-inline field
        if (notification.Metadata is not null &&
            notification.Metadata.TryGetValue("logs", out var logsObj) &&
            logsObj is string logs && !string.IsNullOrWhiteSpace(logs))
        {
            var trimmed = string.Join('\n',
                logs.Split('\n').TakeLast(options.MaxLogLines));

            // Discord code blocks are capped at 1024 chars per field value
            var block   = $"```\n{trimmed}\n```";
            if (block.Length > 1024) block = $"```\n{trimmed[..(1021 - 8)]}…\n```";

            fields.Add(new
            {
                name   = "Recent logs",
                value  = block,
                inline = false,
            });
        }

        // ── Embed ──────────────────────────────────────────────────────────
        var embed = new Dictionary<string, object?>
        {
            ["title"]       = $"{emoji} {notification.Title}",
            ["description"] = notification.Body,
            ["color"]       = color,
            ["fields"]      = fields,
            ["footer"]      = new { text = $"Source: {notification.Source}  ·  {notification.Level}" },
            ["timestamp"]   = notification.Timestamp.UtcDateTime.ToString("O"),
        };

        // ── Webhook payload ────────────────────────────────────────────────
        var payload = new Dictionary<string, object?> { ["embeds"] = new[] { embed } };

        if (!string.IsNullOrWhiteSpace(options.Username))
            payload["username"] = options.Username;

        if (!string.IsNullOrWhiteSpace(options.AvatarUrl))
            payload["avatar_url"] = options.AvatarUrl;

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented        = false,
            Encoder              = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        });
    }
}
