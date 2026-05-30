using Arkn.Relay.Abstractions;
using Arkn.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Arkn.Relay.Core;

/// <summary>
/// Default implementation of the <see cref="IRelay"/> mediator.
/// </summary>
public sealed class ArknRelay : IRelay
{
    private readonly IServiceProvider _serviceProvider;

    public ArknRelay(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> SendAsync<TResponse>(
        IRequest<TResponse> request, 
        CancellationToken cancellationToken = default)
        where TResponse : IResult
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = _serviceProvider.GetRequiredService(handlerType);
        var behaviors = _serviceProvider.GetServices(behaviorType).Cast<object>().ToList();

        RequestHandlerDelegate<TResponse> handlerDelegate = () =>
        {
            var method = handlerType.GetMethod("HandleAsync");
            if (method == null)
            {
                throw new InvalidOperationException($"HandleAsync method not found for {handlerType.Name}");
            }
            return (Task<TResponse>)method.Invoke(handler, new object[] { request, cancellationToken })!;
        };

        if (behaviors.Count == 0)
        {
            return await handlerDelegate();
        }

        return await behaviors
            .Reverse<object>()
            .Aggregate(handlerDelegate, (next, behavior) =>
            {
                return () =>
                {
                    var behaviorInterface = behavior.GetType().GetInterface(behaviorType.Name) 
                                           ?? behavior.GetType();
                    var handleMethod = behaviorInterface.GetMethod("HandleAsync");
                    if (handleMethod == null)
                    {
                        throw new InvalidOperationException($"HandleAsync method not found for {behavior.GetType().Name}");
                    }
                    return (Task<TResponse>)handleMethod.Invoke(behavior, new object[] { request, next, cancellationToken })!;
                };
            })();
    }
}
