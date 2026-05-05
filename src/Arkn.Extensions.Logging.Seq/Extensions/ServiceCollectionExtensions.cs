using Arkn.Logging.Extensions;

namespace Arkn.Extensions.Logging.Seq.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Seq sink to Arkn logging.
    /// </summary>
    public static ArknLoggingBuilder AddSeqSink(
        this ArknLoggingBuilder builder,
        Action<SeqSinkOptions> configure)
    {
        var opts = new SeqSinkOptions();
        configure(opts);
        builder.AddSink(new SeqSink(opts));
        return builder;
    }
}
