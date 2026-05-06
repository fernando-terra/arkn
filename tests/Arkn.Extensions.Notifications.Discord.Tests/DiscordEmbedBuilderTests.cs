using Arkn.Extensions.Notifications.Discord;
using Arkn.Notifications.Models;
using System.Text.Json;

namespace Arkn.Extensions.Notifications.Discord.Tests;

public class DiscordEmbedBuilderTests
{
    private static DiscordNotifierOptions DefaultOptions() => new()
    {
        WebhookUrl   = "https://discord.com/api/webhooks/test/token",
        Username     = "Arkn",
        MinimumLevel = NotificationLevel.Info,
        MaxLogLines  = 5,
    };

    [Fact]
    public void Build_ShouldReturnValidJson()
    {
        var notification = ArknNotification.Error("Job Failed", "Invoice processor failed", "jobs/invoice");
        var json = DiscordEmbedBuilder.Build(notification, DefaultOptions());

        var doc = JsonDocument.Parse(json);
        Assert.NotNull(doc);
    }

    [Fact]
    public void Build_ShouldHaveEmbedsArray()
    {
        var notification = ArknNotification.Info("Info", "Body", "src");
        var json = DiscordEmbedBuilder.Build(notification, DefaultOptions());
        var doc  = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("embeds", out var embeds));
        Assert.Equal(1, embeds.GetArrayLength());
    }

    [Fact]
    public void Build_ShouldIncludeTitle()
    {
        var notification = ArknNotification.Error("My Error Title", "body", "source");
        var json = DiscordEmbedBuilder.Build(notification, DefaultOptions());
        var doc  = JsonDocument.Parse(json);

        var title = doc.RootElement
            .GetProperty("embeds")[0]
            .GetProperty("title")
            .GetString();

        Assert.Contains("My Error Title", title);
    }

    [Fact]
    public void Build_ShouldIncludeEmoji_ForErrorLevel()
    {
        var notification = ArknNotification.Error("E", "B", "s");
        var json = DiscordEmbedBuilder.Build(notification, DefaultOptions());

        Assert.Contains("❌", json);
    }

    [Fact]
    public void Build_ShouldIncludeEmoji_ForCriticalLevel()
    {
        var notification = ArknNotification.Critical("C", "B", "s");
        var json = DiscordEmbedBuilder.Build(notification, DefaultOptions());

        Assert.Contains("⛔", json);
    }

    [Fact]
    public void Build_ShouldIncludeEmoji_ForWarningLevel()
    {
        var notification = ArknNotification.Warning("W", "B", "s");
        var json = DiscordEmbedBuilder.Build(notification, DefaultOptions());

        Assert.Contains("⚠️", json);
    }

    [Fact]
    public void Build_ShouldSetColorForErrorLevel()
    {
        var notification = ArknNotification.Error("E", "B", "s");
        var json = DiscordEmbedBuilder.Build(notification, DefaultOptions());
        var doc  = JsonDocument.Parse(json);

        var color = doc.RootElement
            .GetProperty("embeds")[0]
            .GetProperty("color")
            .GetInt32();

        Assert.Equal(0xcc0000, color);
    }

    [Fact]
    public void Build_ShouldSetColorForWarningLevel()
    {
        var notification = ArknNotification.Warning("W", "B", "s");
        var json = DiscordEmbedBuilder.Build(notification, DefaultOptions());
        var doc  = JsonDocument.Parse(json);

        var color = doc.RootElement
            .GetProperty("embeds")[0]
            .GetProperty("color")
            .GetInt32();

        Assert.Equal(0xff9900, color);
    }

    [Fact]
    public void Build_WithMetadata_ShouldIncludeFieldsInEmbed()
    {
        var metadata = new Dictionary<string, object?>
        {
            ["RunId"]    = "abc-123",
            ["Duration"] = "04:32",
        };
        var notification = new ArknNotification("Title", "Body", NotificationLevel.Error, "source", metadata);
        var json = DiscordEmbedBuilder.Build(notification, DefaultOptions());

        Assert.Contains("RunId", json);
        Assert.Contains("abc-123", json);
        Assert.Contains("Duration", json);
    }

    [Fact]
    public void Build_WithLogs_ShouldIncludeCodeBlock()
    {
        var metadata = new Dictionary<string, object?> { ["logs"] = "line1\nline2\nline3" };
        var notification = new ArknNotification("T", "B", NotificationLevel.Error, "s", metadata);
        var json = DiscordEmbedBuilder.Build(notification, DefaultOptions());

        Assert.Contains("Recent logs", json);
        Assert.Contains("line1", json);
        Assert.Contains("```", json);
    }

    [Fact]
    public void Build_LogsShouldNotAppearAsInlineField()
    {
        var metadata = new Dictionary<string, object?> { ["logs"] = "log line" };
        var notification = new ArknNotification("T", "B", NotificationLevel.Error, "s", metadata);
        var json = DiscordEmbedBuilder.Build(notification, DefaultOptions());
        var doc  = JsonDocument.Parse(json);

        var fields = doc.RootElement
            .GetProperty("embeds")[0]
            .GetProperty("fields");

        // The "logs" key field should have inline=false (rendered as "Recent logs" non-inline)
        foreach (var f in fields.EnumerateArray())
        {
            if (f.TryGetProperty("name", out var name) && name.GetString() == "logs")
                Assert.Fail("'logs' key must not appear as a raw inline field");
        }
    }

    [Fact]
    public void Build_WithUsernameAndAvatarUrl_ShouldIncludeInPayload()
    {
        var opts = new DiscordNotifierOptions
        {
            WebhookUrl  = "https://discord.com/api/webhooks/test",
            Username    = "ArknBot",
            AvatarUrl   = "https://example.com/avatar.png",
        };
        var notification = ArknNotification.Warning("W", "B", "s");
        var json = DiscordEmbedBuilder.Build(notification, opts);

        Assert.Contains("ArknBot", json);
        Assert.Contains("https://example.com/avatar.png", json);
    }

    [Fact]
    public void Build_ShouldIncludeTimestamp()
    {
        var ts           = new DateTimeOffset(2026, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var notification = new ArknNotification("T", "B", NotificationLevel.Error, "s", OccurredOn: ts);
        var json         = DiscordEmbedBuilder.Build(notification, DefaultOptions());

        Assert.Contains("2026-01-15", json);
    }

    [Fact]
    public void Build_ShouldIncludeFooterWithSource()
    {
        var notification = ArknNotification.Error("E", "B", "Arkn.Jobs/InvoiceJob");
        var json = DiscordEmbedBuilder.Build(notification, DefaultOptions());

        Assert.Contains("Arkn.Jobs/InvoiceJob", json);
    }

    [Fact]
    public void Build_WithoutOptionalFields_ShouldNotIncludeUsernameKey()
    {
        var opts = new DiscordNotifierOptions
        {
            WebhookUrl = "https://discord.com/api/webhooks/test",
            // Username and AvatarUrl not set
        };
        var notification = ArknNotification.Info("I", "B", "s");
        var json = DiscordEmbedBuilder.Build(notification, opts);
        var doc  = JsonDocument.Parse(json);

        Assert.False(doc.RootElement.TryGetProperty("username", out _));
        Assert.False(doc.RootElement.TryGetProperty("avatarUrl", out _));
    }
}
