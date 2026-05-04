using Arkn.Http.Abstractions;
using Arkn.Http.Auth;
using Arkn.Http.Cache;
using Arkn.Http.Client;
using Arkn.Http.Configuration;
using Arkn.Http.Resilience;
using Arkn.Logging.Models;
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
    /// An <see cref="IArknHttpBuilder"/> for chaining <c>.WithRetry()</c>, <c>.WithTimeout()</c>,
    /// and auth methods.
    /// </returns>
    /// <example>
    /// <code>
    /// services.AddArknHttp&lt;UserClient&gt;("https://api.example.com")
    ///         .WithRetry(maxAttempts: 3, baseDelay: TimeSpan.FromMilliseconds(200))
    ///         .WithTimeout(TimeSpan.FromSeconds(30))
    ///         .WithBearerAuth(() => tokenProvider.GetTokenAsync());
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

            // Resolve IArknLogger for debug logging if enabled
            if (options.DebugOptions is not null)
                options.DebugLogger = sp.GetService<Arkn.Logging.Abstractions.IArknLogger>();

            IArknHttp http = new ArknHttp(httpClient, options);

            return Microsoft.Extensions.DependencyInjection.ActivatorUtilities
                .CreateInstance<TClient>(sp, http);
        });

        return new ArknHttpBuilder(services, options, clientName);
    }

    // ── Internal builder ───────────────────────────────────────────────────────

    private sealed class ArknHttpBuilder : IArknHttpBuilder
    {
        private readonly IServiceCollection _services;
        private readonly ArknHttpOptions _options;
        private readonly string _clientName;
        private bool _tokenStoreRegistered;

        internal ArknHttpBuilder(IServiceCollection services, ArknHttpOptions options, string clientName)
        {
            _services   = services;
            _options    = options;
            _clientName = clientName;
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

        public IArknHttpBuilder WithInterceptor(IArknAuthInterceptor interceptor)
        {
            _options.Interceptors.Add(interceptor);
            return this;
        }

        public IArknHttpBuilder WithBearerAuth(Func<Task<string>> tokenFactory)
        {
            EnsureTokenStore();
            var store    = new InMemoryTokenStore();
            var storeKey = _clientName;
            _options.Interceptors.Add(new BearerTokenInterceptor(tokenFactory, storeKey, store));
            return this;
        }

        public IArknHttpBuilder WithBearerAuth(Func<IServiceProvider, Task<string>> tokenFactory)
        {
            EnsureTokenStore();
            var store    = new InMemoryTokenStore();
            var storeKey = _clientName;
            _options.Interceptors.Add(new SpBearerTokenInterceptor(tokenFactory, storeKey, store, _services));
            return this;
        }

        public IArknHttpBuilder WithClientCredentials(Action<ClientCredentialsOptions> configure)
        {
            EnsureTokenStore();
            var ccOptions = new ClientCredentialsOptions();
            configure(ccOptions);
            var store = new InMemoryTokenStore();
            _options.Interceptors.Add(new ClientCredentialsInterceptor(ccOptions, store));
            return this;
        }

        public IArknHttpBuilder WithDebugLogging()
            => WithDebugLogging(DebugLoggingOptions.Development);

        public IArknHttpBuilder WithDebugLogging(Action<DebugLoggingOptions> configure)
        {
            var opts = new DebugLoggingOptions();
            configure(opts);
            return WithDebugLogging(opts);
        }

        public IArknHttpBuilder WithDebugLogging(DebugLoggingOptions options)
        {
            _options.DebugOptions = options;
            return this;
        }

        public IArknHttpBuilder WithApiKey(string name, string value)
            => WithApiKey(name, value, ApiKeyInterceptor.Placement.Header);

        public IArknHttpBuilder WithApiKey(string name, string value, ApiKeyInterceptor.Placement placement)
        {
            _options.Interceptors.Add(new ApiKeyInterceptor(name, value, placement));
            return this;
        }

        public IArknHttpBuilder WithRateLimitHandling(Action<RateLimitOptions>? configure = null)
        {
            var opts = new RateLimitOptions();
            configure?.Invoke(opts);
            _options.RateLimitOptions = opts;
            return this;
        }

        public IArknHttpBuilder WithResponseCaching(Action<ResponseCacheOptions>? configure = null)
        {
            var opts = new ResponseCacheOptions();
            configure?.Invoke(opts);
            _options.ResponseCacheOptions = opts;
            _options.ResponseCache = new InMemoryResponseCache();
            return this;
        }

        private void EnsureTokenStore()
        {
            if (_tokenStoreRegistered) return;
            _services.AddSingleton<IArknTokenStore, InMemoryTokenStore>();
            _tokenStoreRegistered = true;
        }
    }

    // SP-aware bearer interceptor: resolves service provider lazily per-request
    private sealed class SpBearerTokenInterceptor : IArknAuthInterceptor
    {
        private readonly Func<IServiceProvider, Task<string>> _factory;
        private readonly string _storeKey;
        private readonly IArknTokenStore _store;
        private readonly IServiceCollection _services;
        private IServiceProvider? _sp;

        public SpBearerTokenInterceptor(
            Func<IServiceProvider, Task<string>> factory,
            string storeKey,
            IArknTokenStore store,
            IServiceCollection services)
        {
            _factory  = factory;
            _storeKey = storeKey;
            _store    = store;
            _services = services;
        }

        public async Task ApplyAsync(HttpRequestMessage request, CancellationToken ct = default)
        {
            var token = await _store.GetAsync(_storeKey).ConfigureAwait(false);

            if (token is null)
            {
                _sp ??= _services.BuildServiceProvider();
                token = await _factory(_sp).ConfigureAwait(false);
                await _store.SetAsync(_storeKey, token, DateTimeOffset.UtcNow.AddMinutes(55))
                            .ConfigureAwait(false);
            }

            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
}
