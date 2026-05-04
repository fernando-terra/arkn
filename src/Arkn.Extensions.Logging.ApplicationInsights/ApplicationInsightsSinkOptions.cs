using Arkn.Logging.Models;

namespace Arkn.Extensions.Logging.ApplicationInsights;

/// <summary>
/// Configuration options for the <see cref="ApplicationInsightsSink"/>.
/// </summary>
public record ApplicationInsightsSinkOptions
{
    /// <summary>
    /// Application Insights connection string.
    /// When <c>null</c>, the SDK will attempt to read from the
    /// <c>APPLICATIONINSIGHTS_CONNECTION_STRING</c> environment variable.
    /// </summary>
    public string? ConnectionString { get; init; }

    /// <summary>
    /// Minimum log level forwarded to Application Insights. Entries below this level are dropped.
    /// Defaults to <see cref="ArknLogLevel.Info"/>.
    /// </summary>
    public ArknLogLevel MinimumLevel { get; init; } = ArknLogLevel.Info;

    /// <summary>
    /// Cloud role name tag attached to all telemetry items.
    /// Defaults to <c>Arkn</c>.
    /// </summary>
    public string RoleName { get; init; } = "Arkn";
}
