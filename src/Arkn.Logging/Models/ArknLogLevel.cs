namespace Arkn.Logging.Models;

/// <summary>Severity levels for Arkn log entries.</summary>
public enum ArknLogLevel
{
    /// <summary>Extremely detailed diagnostic messages, typically only enabled during development.</summary>
    Trace   = 0,
    /// <summary>Detailed messages useful for debugging.</summary>
    Debug   = 1,
    /// <summary>General informational messages about application flow.</summary>
    Info    = 2,
    /// <summary>Messages indicating a potentially harmful situation that is recoverable.</summary>
    Warning = 3,
    /// <summary>Error events that might still allow the application to continue running.</summary>
    Error   = 4,
    /// <summary>Very severe error events that will presumably lead to application abort.</summary>
    Fatal   = 5,
}
