using System;
namespace Arkn.SourceGen;

/// <summary>
/// Annotates a partial Error factory method inside an <see cref="ArknErrorsAttribute"/> class.
/// </summary>
/// <param name="errorType">The <c>ErrorType</c> to use (as a string constant matching the enum name).</param>
/// <param name="defaultMessage">The default human-readable message when no detail is supplied.</param>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ArknErrorCodeAttribute : Attribute
{
    public string ErrorType { get; }
    public string DefaultMessage { get; }

    public ArknErrorCodeAttribute(string errorType, string defaultMessage)
    {
        ErrorType      = errorType;
        DefaultMessage = defaultMessage;
    }
}
