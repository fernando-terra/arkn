using Arkn.Http.Abstractions;
using Arkn.Http.Client;
using Arkn.Http.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arkn.Http.Extensions;

/// <summary>
/// Extension methods for registering Arkn typed HTTP clients with the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a typed Arkn HTTP client <typeparamref name="TClient"/> with its own
    /// <see cref="HttpClient"/>, options, and retry/timeout policy.
    /// </summary>
    /// <typeparam name="TClient">
    /// A concrete class inheriting <see cref="ArknHttpClient"/> with a constructor
    /// that accepts <see cref="IArknHttp"/> as its first parameter.
    /// </typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="baseUrl">Base URL for all requests made by this client.</param>
    /// <returns>
    /// An <see cref="IArknHttpBuilder"/> for chaining <c>.WithRetry()</c> and <c>.WithTimeout()</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// services.AddArknHttp&lt;UserClient&gt;("https://api.example.com")
    ///         .WithRetry(maxAttempts: 3, baseDelay: TimeSpan.FromMilliseconds(200))
    ///         .WithTimeout(TimeSpan.FromSeconds(30));
    /// </code>
    /// </example>
    public static IArknHttpBuilder AddArknHttp<TClient>(
        this IServiceCollection services,
        string baseUrl)
        where TClient : ArknHttpClient
    {
        var options    = new ArknHttpOptions { BaseUrl = baseUrl };
        var clientName = typeof(TClient).Name;

        // Register the named HttpClient
        services.AddHttpClient(clientName);

        // Register TClient as a transient service using ActivatorUtilities
        // so additional constructor parameters (beyond IArknHttp) are resolved from DI.
        services.AddTransient<TClient>(sp =>
        {
            var factory    = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient(clientName);
            IArknHttp http = new ArknHttp(httpClient, options);

            return Microsoft.Extensions.DependencyInjection.ActivatorUtilities
                .CreateInstance<TClient>(sp, http);
        });

        return new ArknHttpBuilder(services, options);
    }

    // ── Internal builder ───────────────────────────────────────────────────────

    private sealed class ArknHttpBuilder : IArknHttpBuilder
    {
        private readonly ArknHttpOptions _options;

        internal ArknHttpBuilder(IServiceCollection _, ArknHttpOptions options)
        {
            _options = options;
        }

        public IArknHttpBuilder WithRetry(int maxAttempts, TimeSpan baseDelay)
        {
            _options.MaxRetryAttempts = maxAttempts;
            _options.BaseRetryDelay   = baseDelay;
            return this;
        }

        public IArknHttpBuilder WithTimeout(TimeSpan timeout)
        {
            _options.Timeout = timeout;
            return this;
        }
    }
}
