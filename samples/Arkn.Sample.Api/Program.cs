using Arkn.Http.Configuration;
using Arkn.Http.Extensions;
using Arkn.Jobs.Extensions;
using Arkn.Jobs.Models;
using Arkn.Logging.Extensions;
using Arkn.Logging.Models;
using Arkn.Logging.Sinks;
using Arkn.Notifications.Extensions;
using Arkn.Sample.Api.Features.HttpDemo;
using Arkn.Sample.Api.Features.Jobs;
using Arkn.Sample.Api.Features.Users;

var builder = WebApplication.CreateBuilder(args);

// ── Logging ────────────────────────────────────────────────────────────────────
// ANSI console in dev; add ApplicationInsights sink in prod via:
//   builder.Services.AddArknLogging(l => l.AddApplicationInsights(...));
builder.Services.AddArknLogging(logging =>
{
    logging.AddConsoleSink();

    logging.AddFileSink(new FileSinkOptions
    {
        Directory       = "logs",
        FileNamePattern = "arkn-sample-{date}.log",
        UseJsonFormat   = true,
    });
});

// ── HTTP client ────────────────────────────────────────────────────────────────
// ExternalUserClient talks to JSONPlaceholder with 3 retries and debug logging.
// In prod: swap .WithDebugLogging(...) preset and add .WithBearerAuth(...) or
// .WithClientCredentials(...) for authenticated APIs.
builder.Services
    .AddArknHttp<ExternalUserClient>("https://jsonplaceholder.typicode.com")
    .WithRetry(maxAttempts: 3, baseDelay: TimeSpan.FromMilliseconds(300))
    .WithTimeout(TimeSpan.FromSeconds(15))
    .WithDebugLogging(builder.Environment.IsDevelopment()
        ? DebugLoggingOptions.Development
        : DebugLoggingOptions.FailuresOnly);

// ── Notifications ──────────────────────────────────────────────────────────────
// Slack notifier — configure your webhook URL via user-secrets or environment:
//   dotnet user-secrets set "Slack:WebhookUrl" "https://hooks.slack.com/..."
builder.Services.AddArknNotifications(notifications =>
{
    var webhookUrl = builder.Configuration["Slack:WebhookUrl"];
    if (!string.IsNullOrWhiteSpace(webhookUrl))
    {
        // Uncomment to enable Slack notifications:
        // notifications.AddSlackNotifier(opts =>
        // {
        //     opts.WebhookUrl   = webhookUrl;
        //     opts.Channel      = "#alerts";
        //     opts.MinimumLevel = NotificationLevel.Warning;
        // });
    }
});

// ── Jobs ───────────────────────────────────────────────────────────────────────
builder.Services.AddArknJobs(jobs =>
{
    // Runs every day at 06:00 — adjust cron as needed
    jobs.Add<ReportGeneratorJob>("0 6 * * *")
        .WithName("report-generator")
        .WithDescription("Generates and sends the daily activity report.")
        .WithRetry(maxAttempts: 2)
        .WithTimeout(TimeSpan.FromMinutes(5))
        .NotifyOn(JobEvent.Failed | JobEvent.TimedOut);

    // Uncomment to route failure notifications to Slack:
    // jobs.OnFailure<SlackNotifier>();
});

// ── Domain services ────────────────────────────────────────────────────────────
builder.Services.AddSingleton<UserService>();

// ── API ────────────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapUsersEndpoints();
app.MapHttpDemoEndpoints();

app.MapGet("/", () => new
{
    name    = "Arkn Sample API",
    version = "0.1.4",
    docs    = "https://fernando-terra.github.io/arkn",
    endpoints = new[]
    {
        "GET  /users",
        "GET  /users/{id}",
        "POST /users",
        "DELETE /users/{id}",
        "GET  /external/users",
        "GET  /external/users/{id}",
        "POST /external/users",
        "DELETE /external/users/{id}",
    }
}).WithTags("Info");

app.Run();
