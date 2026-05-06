using Arkn.Notifications.Models;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Arkn.Extensions.Notifications.Teams;

/// <summary>
/// Builds a Microsoft Teams Adaptive Card JSON payload from an <see cref="ArknNotification"/>.
/// Targets the Incoming Webhook format: a message with an Adaptive Card attachment.
/// Zero external dependencies — uses System.Text.Json only.
/// </summary>
public static class TeamsCardBuilder
{
    private static readonly Dictionary<NotificationLevel, string> LevelEmoji = new()
    {
        [NotificationLevel.Info]     = "ℹ️",
        [NotificationLevel.Warning]  = "⚠️",
        [NotificationLevel.Error]    = "❌",
        [NotificationLevel.Critical] = "⛔",
    };

    // Adaptive Card accent colors (hex, shown as header background via Container.style)
    private static readonly Dictionary<NotificationLevel, string> LevelColor = new()
    {
        [NotificationLevel.Info]     = "good",     // green
        [NotificationLevel.Warning]  = "warning",  // yellow
        [NotificationLevel.Error]    = "attention", // red
        [NotificationLevel.Critical] = "attention",
    };

    /// <summary>
    /// Builds the Teams Incoming Webhook payload as a JSON string ready to POST.
    /// Uses the modern <c>application/vnd.microsoft.card.adaptive</c> format.
    /// </summary>
    public static string Build(ArknNotification notification, TeamsNotifierOptions options)
    {
        var emoji = LevelEmoji.GetValueOrDefault(notification.Level, "🔔");
        var style = LevelColor.GetValueOrDefault(notification.Level, "default");

        // ── Adaptive Card body ─────────────────────────────────────────────
        var cardBody = new List<object>
        {
            // Coloured header container
            new
            {
                type  = "Container",
                style = style,
                items = new object[]
                {
                    new
                    {
                        type   = "TextBlock",
                        text   = $"{emoji} {notification.Title}",
                        weight = "Bolder",
                        size   = "Medium",
                        wrap   = true,
                        color  = "Light",
                    }
                }
            },
            // Body text
            new
            {
                type  = "TextBlock",
                text  = notification.Body,
                wrap  = true,
                spacing = "Medium",
            },
        };

        // Metadata fact set
        if (notification.Metadata is { Count: > 0 })
        {
            var facts = notification.Metadata
                .Where(kv => kv.Key != "logs")
                .Take(10)
                .Select(kv => new { title = kv.Key, value = kv.Value?.ToString() ?? string.Empty })
                .ToList<object>();

            if (facts.Count > 0)
            {
                cardBody.Add(new
                {
                    type   = "FactSet",
                    facts,
                    spacing = "Medium",
                });
            }
        }

        // Log snippet
        if (notification.Metadata is not null &&
            notification.Metadata.TryGetValue("logs", out var logsObj) &&
            logsObj is string logs && !string.IsNullOrWhiteSpace(logs))
        {
            var trimmed = string.Join('\n',
                logs.Split('\n').TakeLast(options.MaxLogLines));

            cardBody.Add(new
            {
                type    = "TextBlock",
                text    = "**Recent logs:**",
                wrap    = true,
                spacing = "Medium",
            });
            cardBody.Add(new
            {
                type      = "TextBlock",
                text      = trimmed,
                wrap      = true,
                fontType  = "Monospace",
                isSubtle  = true,
                spacing   = "Small",
            });
        }

        // Footer
        cardBody.Add(new
        {
            type     = "TextBlock",
            text     = $"Source: {notification.Source}  ·  {notification.Level}  ·  {notification.Timestamp:yyyy-MM-dd HH:mm:ss} UTC",
            isSubtle = true,
            size     = "Small",
            wrap     = true,
            spacing  = "Medium",
        });

        // ── Adaptive Card ──────────────────────────────────────────────────
        var adaptiveCard = new
        {
            type    = "AdaptiveCard",
            version = "1.4",
            schema  = "http://adaptivecards.io/schemas/adaptive-card.json",
            body    = cardBody,
        };

        // ── Webhook envelope ───────────────────────────────────────────────
        var payload = new
        {
            type        = "message",
            attachments = new[]
            {
                new
                {
                    contentType = "application/vnd.microsoft.card.adaptive",
                    contentUrl  = (string?)null,
                    content     = adaptiveCard,
                }
            }
        };

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented        = false,
            Encoder              = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        });
    }
}
