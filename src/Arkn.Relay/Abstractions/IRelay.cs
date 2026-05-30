using Arkn.Core.Abstractions;

namespace Arkn.Relay.Abstractions;

/// <summary>
/// Defines the relay (mediator) for dispatching requests to their respective handlers.
/// </summary>
public interface IRelay
{
    /// <summary>
    /// Sends a request to its single handler and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result from the handler.</returns>
    Task<TResponse> SendAsync<TResponse>(
        IRequest<TResponse> request, 
        CancellationToken cancellationToken = default)
        where TResponse : IResult;
}
