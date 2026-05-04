namespace Arkn.Notifications.Models;

/// <summary>Severity level of a notification.</summary>
public enum NotificationLevel
{
    /// <summary>A general informational notification.</summary>
    Info     = 0,
    /// <summary>A notification indicating a potential issue that is not yet an error.</summary>
    Warning  = 1,
    /// <summary>A notification indicating an error condition.</summary>
    Error    = 2,
    /// <summary>A notification indicating a critical failure requiring immediate attention.</summary>
    Critical = 3,
}
