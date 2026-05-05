using Arkn.Logging.Models;

namespace Arkn.Extensions.Logging.Elasticsearch;

/// <summary>Configuration for the Elasticsearch log sink.</summary>
public sealed class ElasticsearchSinkOptions
{
    /// <summary>Elasticsearch node URL, e.g. "http://localhost:9200".</summary>
    public string NodeUrl { get; set; } = "http://localhost:9200";

    /// <summary>Index name pattern. Supports {yyyy-MM-dd} token for daily rotation.</summary>
    public string IndexPattern { get; set; } = "arkn-logs-{yyyy-MM-dd}";

    /// <summary>Optional Basic Auth username.</summary>
    public string? Username { get; set; }

    /// <summary>Optional Basic Auth password.</summary>
    public string? Password { get; set; }

    /// <summary>Optional Bearer token (overrides Basic Auth if both set).</summary>
    public string? ApiKey { get; set; }

    /// <summary>Minimum log level to send.</summary>
    public ArknLogLevel MinimumLevel { get; set; } = ArknLogLevel.Info;

    /// <summary>Max entries per bulk request.</summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>Flush interval in seconds.</summary>
    public int FlushIntervalSeconds { get; set; } = 5;

    /// <summary>Request timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 15;
}
