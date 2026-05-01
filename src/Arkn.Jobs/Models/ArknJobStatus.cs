namespace Arkn.Jobs.Models;

/// <summary>The lifecycle state of a job execution run.</summary>
public enum ArknJobStatus
{
    Pending   = 0,
    Running   = 1,
    Succeeded = 2,
    Failed    = 3,
    TimedOut  = 4,
}
