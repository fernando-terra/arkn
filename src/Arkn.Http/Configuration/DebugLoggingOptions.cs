using Arkn.Logging.Models;

namespace Arkn.Http.Configuration;

/// <summary>
/// Fine-grained control over what Arkn.Http logs in debug mode.
/// Configure via <c>.WithDebugLogging(opts => ...)</c>.
/// </summary>
public sealed class DebugLoggingOptions
{
    /// <summary>
    /// Log level for successful responses (2xx).
    /// Default: <see cref="ArknLogLevel.Debug"/> — silent in production when AppInsights
    /// minimum level is Warning or above.
    /// </summary>
    public ArknLogLevel SuccessLevel { get; set; } = ArknLogLevel.Debug;

    /// <summary>
    /// Log level for client error responses (4xx).
    /// Default: <see cref="ArknLogLevel.Warning"/>.
    /// </summary>
    public ArknLogLevel ClientErrorLevel { get; set; } = ArknLogLevel.Warning;

    /// <summary>
    /// Log level for server error responses (5xx).
    /// Default: <see cref="ArknLogLevel.Error"/>.
    /// </summary>
    public ArknLogLevel ServerErrorLevel { get; set; } = ArknLogLevel.Error;

    /// <summary>
    /// Whether to include the request body in log output.
    /// Default: <c>true</c>.
    /// </summary>
    public bool LogRequestBody { get; set; } = true;

    /// <summary>
    /// Whether to include the response body in log output.
    /// Default: <c>true</c>.
    /// </summary>
    public bool LogResponseBody { get; set; } = true;

    /// <summary>
    /// Whether to log request and response headers.
    /// Default: <c>true</c>.
    /// </summary>
    public bool LogHeaders { get; set; } = true;

    /// <summary>
    /// Preset for production: 2xx at Info (visible in AppInsights), 4xx Warning, 5xx Error.
    /// Full request tracing lands in AppInsights when its MinimumLevel is Info or lower.
    /// </summary>
    public static DebugLoggingOptions Production => new()
    {
        SuccessLevel      = ArknLogLevel.Info,
        ClientErrorLevel  = ArknLogLevel.Warning,
        ServerErrorLevel  = ArknLogLevel.Error,
    };

    /// <summary>
    /// Preset for development: everything at Debug, full bodies logged.
    /// </summary>
    public static DebugLoggingOptions Development => new()
    {
        SuccessLevel      = ArknLogLevel.Debug,
        ClientErrorLevel  = ArknLogLevel.Warning,
        ServerErrorLevel  = ArknLogLevel.Error,
    };

    /// <summary>
    /// Preset for failures only: 2xx are silent, 4xx Warning, 5xx Error.
    /// Useful for production tracing of problems without noise.
    /// </summary>
    public static DebugLoggingOptions FailuresOnly => new()
    {
        SuccessLevel      = ArknLogLevel.Trace, // below any typical minimum level
        ClientErrorLevel  = ArknLogLevel.Warning,
        ServerErrorLevel  = ArknLogLevel.Error,
    };
}
