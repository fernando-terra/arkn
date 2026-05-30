using Arkn.Relay.Abstractions;
using Arkn.Relay.Core;
using Arkn.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arkn.Relay.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Arkn.Relay to the service collection.
    /// </summary>
    public static IServiceCollection AddArknRelay(this IServiceCollection services)
    {
        services.TryAddScoped<IRelay, ArknRelay>();
        return services;
    }

    /// <summary>
    /// Registers a handler for a specific request.
    /// </summary>
    public static IServiceCollection AddArknHandler<TRequest, TResponse, THandler>(this IServiceCollection services)
        where TRequest : IRequest<TResponse>
        where TResponse : IResult
        where THandler : class, IRequestHandler<TRequest, TResponse>
    {
        services.AddScoped<IRequestHandler<TRequest, TResponse>, THandler>();
        return services;
    }

    /// <summary>
    /// Registers a pipeline behavior for all requests or specific requests.
    /// </summary>
    public static IServiceCollection AddArknBehavior(this IServiceCollection services, Type behaviorType)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), behaviorType);
        return services;
    }

    /// <summary>
    /// Registers a pipeline behavior for a specific request.
    /// </summary>
    public static IServiceCollection AddArknBehavior<TRequest, TResponse, TBehavior>(this IServiceCollection services)
        where TRequest : IRequest<TResponse>
        where TResponse : IResult
        where TBehavior : class, IPipelineBehavior<TRequest, TResponse>
    {
        services.AddScoped<IPipelineBehavior<TRequest, TResponse>, TBehavior>();
        return services;
    }
}
