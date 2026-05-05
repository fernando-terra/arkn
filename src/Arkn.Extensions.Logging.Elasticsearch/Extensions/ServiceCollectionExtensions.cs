using Arkn.Logging.Extensions;

namespace Arkn.Extensions.Logging.Elasticsearch.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Elasticsearch sink to Arkn logging.
    /// </summary>
    public static ArknLoggingBuilder AddElasticsearchSink(
        this ArknLoggingBuilder builder,
        Action<ElasticsearchSinkOptions> configure)
    {
        var opts = new ElasticsearchSinkOptions();
        configure(opts);
        builder.AddSink(new ElasticsearchSink(opts));
        return builder;
    }
}
