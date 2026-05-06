# Arkn.Extensions.Logging.Elasticsearch

Elasticsearch sink for Arkn.Logging — sends logs via the **Bulk API** (`/_bulk`) using ndjson. **Zero NEST / Elastic.Clients.Elasticsearch dependency.**

```bash
dotnet add package Arkn.Extensions.Logging.Elasticsearch
```

## Setup

```csharp
builder.Services.AddArknLogging(logging =>
{
    logging.AddConsoleSink();

    logging.AddElasticsearchSink(es =>
    {
        es.NodeUrl              = "http://localhost:9200";
        es.IndexPattern         = "arkn-logs-{yyyy-MM-dd}";  // daily rotation
        es.MinimumLevel         = ArknLogLevel.Info;
        es.BatchSize            = 100;
        es.FlushIntervalSeconds = 5;
        es.TimeoutSeconds       = 15;

        // Authentication (choose one)
        es.ApiKey   = config["Elasticsearch:ApiKey"];     // Bearer ApiKey header
        // es.Username = config["Elasticsearch:Username"];
        // es.Password = config["Elasticsearch:Password"]; // Basic Auth
    });
});
```

## Index pattern tokens

| Token | Example output |
|---|---|
| `{yyyy-MM-dd}` | `arkn-logs-2026-05-06` |
| `{yyyy-MM}` | `arkn-logs-2026-05` |
| `{yyyy}` | `arkn-logs-2026` |

## Document schema

Each log entry is indexed as:

```json
{
  "@timestamp": "2026-05-06T00:35:00.000Z",
  "level": "Info",
  "message": "Job started",
  "scope": "job-run-abc123",
  "exception": null,
  "UserId": 42,
  "Action": "ProcessOrder"
}
```

`Context` key-value pairs are flattened as top-level fields for easy filtering in Kibana/Discover.

## Batching

Entries are buffered and flushed:
- When the buffer reaches `BatchSize` (default: 100)
- Every `FlushIntervalSeconds` seconds (default: 5)
- On `Dispose()` (final flush is synchronous)

Bulk API errors are swallowed — an Elasticsearch outage will not crash your application.
