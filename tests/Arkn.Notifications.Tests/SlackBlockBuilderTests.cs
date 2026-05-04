using Arkn.Extensions.Notifications.Slack;
using Arkn.Notifications.Models;
using System.Text.Json;

namespace Arkn.Notifications.Tests;

public class SlackBlockBuilderTests
{
    private static SlackNotifierOptions DefaultOptions() => new()
    {
        WebhookUrl = "https://hooks.slack.com/test",
        Username   = "Arkn",
        IconEmoji  = ":robot_face:",
    };

    [Fact]
    public void Build_ShouldReturnValidJson()
    {
        var notification = ArknNotification.Error("Job Failed", "Invoice processor failed", "jobs/invoice");
        var json = SlackBlockBuilder.Build(notification, DefaultOptions());

        var doc = JsonDocument.Parse(json);
        Assert.NotNull(doc);
    }

    [Fact]
    public void Build_ShouldIncludeBlocksArray()
    {
        var notification = ArknNotification.Error("Title", "Body", "source");
        var json = SlackBlockBuilder.Build(notification, DefaultOptions());
        var doc  = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("blocks", out var blocks));
        Assert.True(blocks.GetArrayLength() >= 3); // header + divider + body minimum
    }

    [Fact]
    public void Build_ShouldIncludeEmoji_ForErrorLevel()
    {
        var notification = ArknNotification.Error("Error Title", "body", "source");
        var json = SlackBlockBuilder.Build(notification, DefaultOptions());

        Assert.Contains("❌", json);
    }

    [Fact]
    public void Build_ShouldIncludeEmoji_ForCriticalLevel()
    {
        var notification = ArknNotification.Critical("Critical", "body", "source");
        var json = SlackBlockBuilder.Build(notification, DefaultOptions());

        Assert.Contains("⛔", json);
    }

    [Fact]
    public void Build_WithMetadata_ShouldIncludeFields()
    {
        var metadata = new Dictionary<string, object?>
        {
            ["RunId"]    = "abc-123",
            ["Duration"] = "04:32",
        };
        var notification = new ArknNotification("Title", "Body", NotificationLevel.Error, "source", metadata);
        var json = SlackBlockBuilder.Build(notification, DefaultOptions());

        Assert.Contains("RunId", json);
        Assert.Contains("abc-123", json);
    }

    [Fact]
    public void Build_WithLogs_ShouldIncludeCodeBlock()
    {
        var metadata = new Dictionary<string, object?> { ["logs"] = "line1\nline2\nline3" };
        var notification = new ArknNotification("Title", "Body", NotificationLevel.Error, "source", metadata);
        var json = SlackBlockBuilder.Build(notification, DefaultOptions());

        Assert.Contains("Recent logs", json);
        Assert.Contains("line1", json);
    }

    [Fact]
    public void Build_WithChannelAndUsername_ShouldIncludeInPayload()
    {
        var opts = new SlackNotifierOptions
        {
            WebhookUrl = "https://hooks.slack.com/test",
            Channel    = "#alerts",
            Username   = "Arkn Bot",
            IconEmoji  = ":bell:",
        };
        var notification = ArknNotification.Warning("W", "B", "s");
        var json = SlackBlockBuilder.Build(notification, opts);

        Assert.Contains("#alerts", json);
        Assert.Contains("Arkn Bot", json);
    }
}
