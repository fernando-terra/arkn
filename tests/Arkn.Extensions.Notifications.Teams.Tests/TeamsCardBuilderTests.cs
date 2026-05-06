using Arkn.Extensions.Notifications.Teams;
using Arkn.Notifications.Models;
using System.Text.Json;

namespace Arkn.Extensions.Notifications.Teams.Tests;

public class TeamsCardBuilderTests
{
    private static TeamsNotifierOptions DefaultOptions() => new()
    {
        WebhookUrl   = "https://your-org.webhook.office.com/test",
        MinimumLevel = NotificationLevel.Info,
        MaxLogLines  = 5,
    };

    [Fact]
    public void Build_ShouldReturnValidJson()
    {
        var notification = ArknNotification.Error("Job Failed", "Invoice processor failed", "jobs/invoice");
        var json = TeamsCardBuilder.Build(notification, DefaultOptions());

        var doc = JsonDocument.Parse(json);
        Assert.NotNull(doc);
    }

    [Fact]
    public void Build_ShouldHaveTypeMessage()
    {
        var notification = ArknNotification.Info("Test", "Body", "src");
        var json = TeamsCardBuilder.Build(notification, DefaultOptions());
        var doc  = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("type", out var type));
        Assert.Equal("message", type.GetString());
    }

    [Fact]
    public void Build_ShouldHaveAttachmentsArray()
    {
        var notification = ArknNotification.Warning("W", "Body", "src");
        var json = TeamsCardBuilder.Build(notification, DefaultOptions());
        var doc  = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("attachments", out var att));
        Assert.Equal(1, att.GetArrayLength());
    }

    [Fact]
    public void Build_AttachmentContentType_ShouldBeAdaptiveCard()
    {
        var notification = ArknNotification.Error("E", "B", "s");
        var json = TeamsCardBuilder.Build(notification, DefaultOptions());
        var doc  = JsonDocument.Parse(json);

        var contentType = doc.RootElement
            .GetProperty("attachments")[0]
            .GetProperty("contentType")
            .GetString();

        Assert.Equal("application/vnd.microsoft.card.adaptive", contentType);
    }

    [Fact]
    public void Build_ShouldIncludeTitleInCardBody()
    {
        var notification = ArknNotification.Error("My Error Title", "body", "source");
        var json = TeamsCardBuilder.Build(notification, DefaultOptions());

        Assert.Contains("My Error Title", json);
    }

    [Fact]
    public void Build_ShouldIncludeEmoji_ForErrorLevel()
    {
        var notification = ArknNotification.Error("E", "B", "s");
        var json = TeamsCardBuilder.Build(notification, DefaultOptions());

        Assert.Contains("❌", json);
    }

    [Fact]
    public void Build_ShouldIncludeEmoji_ForCriticalLevel()
    {
        var notification = ArknNotification.Critical("C", "B", "s");
        var json = TeamsCardBuilder.Build(notification, DefaultOptions());

        Assert.Contains("⛔", json);
    }

    [Fact]
    public void Build_ShouldIncludeEmoji_ForWarningLevel()
    {
        var notification = ArknNotification.Warning("W", "B", "s");
        var json = TeamsCardBuilder.Build(notification, DefaultOptions());

        Assert.Contains("⚠️", json);
    }

    [Fact]
    public void Build_WithMetadata_ShouldIncludeFactSet()
    {
        var metadata = new Dictionary<string, object?>
        {
            ["RunId"]    = "abc-123",
            ["Duration"] = "02:30",
        };
        var notification = new ArknNotification("Title", "Body", NotificationLevel.Error, "source", metadata);
        var json = TeamsCardBuilder.Build(notification, DefaultOptions());

        Assert.Contains("RunId", json);
        Assert.Contains("abc-123", json);
        Assert.Contains("Duration", json);
    }

    [Fact]
    public void Build_WithLogs_ShouldIncludeLogBlock()
    {
        var metadata = new Dictionary<string, object?> { ["logs"] = "line1\nline2\nline3" };
        var notification = new ArknNotification("T", "B", NotificationLevel.Error, "s", metadata);
        var json = TeamsCardBuilder.Build(notification, DefaultOptions());

        Assert.Contains("Recent logs", json);
        Assert.Contains("line1", json);
    }

    [Fact]
    public void Build_LogsShouldNotAppearInFactSet()
    {
        var metadata = new Dictionary<string, object?> { ["logs"] = "log line" };
        var notification = new ArknNotification("T", "B", NotificationLevel.Error, "s", metadata);
        var json = TeamsCardBuilder.Build(notification, DefaultOptions());
        var doc  = JsonDocument.Parse(json);

        // FactSet facts should NOT contain a "logs" title
        var card    = doc.RootElement.GetProperty("attachments")[0].GetProperty("content");
        var body    = card.GetProperty("body");
        var hasLogsInFactSet = false;
        foreach (var block in body.EnumerateArray())
        {
            if (block.TryGetProperty("type", out var t) && t.GetString() == "FactSet")
            {
                if (block.TryGetProperty("facts", out var facts))
                {
                    foreach (var f in facts.EnumerateArray())
                    {
                        if (f.TryGetProperty("title", out var title) && title.GetString() == "logs")
                            hasLogsInFactSet = true;
                    }
                }
            }
        }

        Assert.False(hasLogsInFactSet, "'logs' key should not appear as a FactSet entry");
    }

    [Fact]
    public void Build_ShouldIncludeSourceAndTimestamp()
    {
        var notification = ArknNotification.Error("E", "B", "Arkn.Jobs/InvoiceJob");
        var json = TeamsCardBuilder.Build(notification, DefaultOptions());

        Assert.Contains("Arkn.Jobs/InvoiceJob", json);
    }
}
