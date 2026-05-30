using Arkn.Core.Abstractions;

namespace Arkn.Relay.Abstractions;

/// <summary>
/// Represents a request that returns a response.
/// </summary>
/// <typeparam name="TResponse">The type of the response. Should typically be a Result or IResult.</typeparam>
public interface IRequest<out TResponse>
{
}

/// <summary>
/// Represents a request that does not return a value.
/// In Arkn.Relay, this usually returns a <see cref="Arkn.Results.Result"/>.
/// </summary>
public interface IRequest : IRequest<Arkn.Results.Result>
{
}
