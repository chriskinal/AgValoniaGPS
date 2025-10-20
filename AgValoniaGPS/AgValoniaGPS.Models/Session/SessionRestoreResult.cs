using System;

namespace AgValoniaGPS.Models.Session;

/// <summary>
/// Represents the result of attempting to restore a session from crash recovery.
/// </summary>
public class SessionRestoreResult
{
    /// <summary>
    /// Gets or sets whether the session restore succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the restored session state.
    /// Null if restore failed.
    /// </summary>
    public SessionState? RestoredSession { get; set; }

    /// <summary>
    /// Gets or sets the error message if restore failed.
    /// Empty if successful.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the application crashed (last snapshot time).
    /// Used to show the user how old the recovered session is.
    /// </summary>
    public DateTime CrashTime { get; set; }
}
