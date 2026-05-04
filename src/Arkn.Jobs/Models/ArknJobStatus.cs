namespace Arkn.Jobs.Models;

/// <summary>The lifecycle state of a job execution run.</summary>
public enum ArknJobStatus
{
    /// <summary>The job is queued and has not started yet.</summary>
    Pending   = 0,
    /// <summary>The job is currently executing.</summary>
    Running   = 1,
    /// <summary>The job completed successfully.</summary>
    Succeeded = 2,
    /// <summary>The job completed with a failure.</summary>
    Failed    = 3,
    /// <summary>The job was cancelled because it exceeded its configured timeout.</summary>
    TimedOut  = 4,
}
