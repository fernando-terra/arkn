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

        var handler = _serviceProvider.GetRequiredService(handlerType);

        // We use dynamic or reflection here for the alpha, but 
        // the plan is to move this to a Source Generated dispatcher.
        var method = handlerType.GetMethod("HandleAsync");
        
        if (method == null)
        {
            throw new InvalidOperationException($"HandleAsync method not found for {handlerType.Name}");
        }

        return await (Task<TResponse>)method.Invoke(handler, new object[] { request, cancellationToken })!;
    }
}
