using Arkn.Logging.Models;

namespace Arkn.Extensions.Logging.Seq;

/// <summary>Configuration for the Seq log sink.</summary>
public sealed class SeqSinkOptions
{
    /// <summary>Base URL of the Seq server, e.g. "http://localhost:5341".</summary>
    public string ServerUrl { get; set; } = "http://localhost:5341";

    /// <summary>Optional Seq API key.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Minimum log level to send. Entries below this are dropped.</summary>
    public ArknLogLevel MinimumLevel { get; set; } = ArknLogLevel.Info;

    /// <summary>
    /// Maximum entries to buffer before flushing.
    /// The sink flushes after this many entries or after <see cref="FlushIntervalSeconds"/>, whichever comes first.
    /// </summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>Flush interval in seconds even if batch is not full.</summary>
    public int FlushIntervalSeconds { get; set; } = 5;

    /// <summary>Request timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 10;
}
