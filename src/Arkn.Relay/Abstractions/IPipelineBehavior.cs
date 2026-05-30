using Arkn.Core.Abstractions;

namespace Arkn.Relay.Abstractions;

/// <summary>
/// Represents a delegate that handles a request in a pipeline.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <returns>A task that represents the response.</returns>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Defines a behavior that can be executed as part of a request pipeline.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult
{
    /// <summary>
    /// Handles the request in the pipeline.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="next">The delegate to the next handler in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response from the pipeline.</returns>
    Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}
