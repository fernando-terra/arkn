namespace Arkn.Jobs.Models;

/// <summary>
/// Flags controlling which job lifecycle events trigger notifications.
/// </summary>
[Flags]
public enum JobEvent
{
    None      = 0,
    Started   = 1,
    Succeeded = 2,
    Failed    = 4,
    TimedOut  = 8,
    All       = Started | Succeeded | Failed | TimedOut,
}
