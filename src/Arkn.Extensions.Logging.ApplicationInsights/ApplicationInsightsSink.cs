using Arkn.Logging.Abstractions;
using Arkn.Logging.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Arkn.Extensions.Logging.ApplicationInsights;

/// <summary>
/// An <see cref="IArknLogSink"/> that forwards log entries to Azure Application Insights.
/// </summary>
/// <remarks>
/// Exceptions are sent as <see cref="ExceptionTelemetry"/>; all other entries are sent
/// as <see cref="TraceTelemetry"/>. Context properties and scope are attached as custom
/// dimensions on every telemetry item.
/// </remarks>
public sealed class ApplicationInsightsSink : IArknLogSink, IDisposable
{
    private readonly TelemetryClient _client;
    private readonly ApplicationInsightsSinkOptions _options;

    /// <summary>
    /// Initializes the sink using the provided <paramref name="options"/>.
    /// </summary>
    public ApplicationInsightsSink(ApplicationInsightsSinkOptions options)
    {
        _options = options;

        var config = new TelemetryConfiguration();

        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
            config.ConnectionString = options.ConnectionString;
        // else the SDK will pick up APPLICATIONINSIGHTS_CONNECTION_STRING automatically

        _client = new TelemetryClient(config);
        _client.Context.Cloud.RoleName = options.RoleName;
    }

    /// <inheritdoc />
    public void Write(LogEntry entry)
    {
        if (entry.Level < _options.MinimumLevel)
            return;

        if (entry.Exception is not null)
            SendException(entry);
        else
            SendTrace(entry);
    }

    private void SendTrace(LogEntry entry)
    {
        var telemetry = new TraceTelemetry(entry.Message, MapSeverity(entry.Level))
        {
            Timestamp = entry.Timestamp,
        };

        AttachProperties(telemetry.Properties, entry);
        _client.TrackTrace(telemetry);
    }

    private void SendException(LogEntry entry)
    {
        var telemetry = new ExceptionTelemetry(entry.Exception!)
        {
            Message     = entry.Message,
            SeverityLevel = MapSeverity(entry.Level),
            Timestamp   = entry.Timestamp,
        };

        AttachProperties(telemetry.Properties, entry);
        _client.TrackException(telemetry);
    }

    private static void AttachProperties(IDictionary<string, string> properties, LogEntry entry)
    {
        if (entry.Scope is not null)
            properties["scope"] = entry.Scope;

        if (entry.Context is { Count: > 0 })
        {
            foreach (var (key, value) in entry.Context)
                properties[key] = value?.ToString() ?? string.Empty;
        }
    }

    private static SeverityLevel MapSeverity(ArknLogLevel level) => level switch
    {
        ArknLogLevel.Trace   => SeverityLevel.Verbose,
        ArknLogLevel.Debug   => SeverityLevel.Verbose,
        ArknLogLevel.Info    => SeverityLevel.Information,
        ArknLogLevel.Warning => SeverityLevel.Warning,
        ArknLogLevel.Error   => SeverityLevel.Error,
        ArknLogLevel.Fatal   => SeverityLevel.Critical,
        _                    => SeverityLevel.Information,
    };

    /// <summary>Flushes pending telemetry and disposes internal resources.</summary>
    public void Dispose()
    {
        _client.Flush();
    }
}
