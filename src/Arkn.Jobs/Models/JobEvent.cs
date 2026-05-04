namespace Arkn.Jobs.Models;

/// <summary>
/// Flags controlling which job lifecycle events trigger notifications.
/// </summary>
[Flags]
public enum JobEvent
{
    /// <summary>No lifecycle events selected.</summary>
    None      = 0,
    /// <summary>Fired when a job execution begins.</summary>
    Started   = 1,
    /// <summary>Fired when a job execution completes successfully.</summary>
    Succeeded = 2,
    /// <summary>Fired when a job execution fails.</summary>
    Failed    = 4,
    /// <summary>Fired when a job execution is cancelled due to a timeout.</summary>
    TimedOut  = 8,
    /// <summary>Combination of all lifecycle events.</summary>
    All       = Started | Succeeded | Failed | TimedOut,
}
