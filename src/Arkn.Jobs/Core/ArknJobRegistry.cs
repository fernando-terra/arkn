using Arkn.Jobs.Abstractions;
using Arkn.Jobs.Models;

namespace Arkn.Jobs.Core;

/// <summary>Holds the ordered list of jobs registered via <c>AddArknJobs()</c>.</summary>
public sealed class ArknJobRegistry : IArknJobRegistry
{
    private readonly List<ArknJobOptions> _jobs = [];

    public IReadOnlyList<ArknJobOptions> Jobs => _jobs.AsReadOnly();

    /// <summary>CLR type of the global failure notifier (set via OnFailure&lt;T&gt;()).</summary>
    public Type? GlobalFailureNotifierType { get; private set; }

    internal void Register(ArknJobOptions options) => _jobs.Add(options);

    internal void SetGlobalFailureNotifier(Type notifierType)
        => GlobalFailureNotifierType = notifierType;
}
