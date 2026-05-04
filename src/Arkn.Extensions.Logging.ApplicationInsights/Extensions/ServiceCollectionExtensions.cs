using Arkn.Logging.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Arkn.Extensions.Logging.ApplicationInsights.Extensions;

/// <summary>
/// Extension methods for registering the Application Insights sink with Arkn logging.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds an Application Insights sink to the Arkn logging pipeline.
    /// </summary>
    /// <param name="builder">The <see cref="ArknLoggingBuilder"/> to configure.</param>
    /// <param name="configure">A delegate to configure <see cref="ApplicationInsightsSinkOptions"/>.</param>
    /// <returns>The same <see cref="ArknLoggingBuilder"/> for chaining.</returns>
    public static ArknLoggingBuilder AddApplicationInsights(
        this ArknLoggingBuilder builder,
        Action<ApplicationInsightsSinkOptions> configure)
    {
        var options = new ApplicationInsightsSinkOptions();
        configure(options);
        builder.AddSink(new ApplicationInsightsSink(options));
        return builder;
    }
}
