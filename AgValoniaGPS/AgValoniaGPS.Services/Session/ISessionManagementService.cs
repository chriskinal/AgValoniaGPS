using System;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Models.Session;
using AgValoniaGPS.Models.StateManagement;

namespace AgValoniaGPS.Services.Session;

/// <summary>
/// Manages application session state with crash recovery capabilities.
/// Provides periodic snapshots for crash recovery, session start/end tracking, and state restoration.
/// </summary>
public interface ISessionManagementService
{
    /// <summary>
    /// Raised when session state changes (session start, end, or state update).
    /// </summary>
    event EventHandler<SessionStateChangedEventArgs>? SessionStateChanged;

    /// <summary>
    /// Starts a new session and begins periodic crash recovery snapshots.
    /// </summary>
    /// <returns>A task that completes when the session is started</returns>
    Task StartSessionAsync();

    /// <summary>
    /// Ends the current session and saves final state.
    /// Stops periodic crash recovery snapshots.
    /// </summary>
    /// <returns>A task that completes when the session is ended</returns>
    Task EndSessionAsync();

    /// <summary>
    /// Saves current session state as a crash recovery snapshot.
    /// Called periodically (every 30 seconds) during active session.
    /// Must complete in less than 500ms.
    /// </summary>
    /// <returns>A task that completes when the snapshot is saved</returns>
    Task SaveSessionSnapshotAsync();

    /// <summary>
    /// Attempts to restore the last session from crash recovery file.
    /// Checks for CrashRecovery.json and returns restoration result.
    /// </summary>
    /// <returns>Session restore result with success status and recovered data</returns>
    Task<SessionRestoreResult> RestoreLastSessionAsync();

    /// <summary>
    /// Gets the current session state.
    /// </summary>
    /// <returns>The current session state or null if no active session</returns>
    SessionState? GetCurrentSessionState();

    /// <summary>
    /// Updates the current field name in the session state.
    /// </summary>
    /// <param name="fieldName">The name of the current field</param>
    void UpdateCurrentField(string fieldName);

    /// <summary>
    /// Updates the current guidance line in the session state.
    /// </summary>
    /// <param name="lineType">The type of guidance line (ABLine, Curve, Contour)</param>
    /// <param name="lineData">The guidance line data object</param>
    void UpdateCurrentGuidanceLine(GuidanceLineType lineType, object? lineData);

    /// <summary>
    /// Updates the work progress data in the session state.
    /// </summary>
    /// <param name="progressData">The work progress data to update</param>
    void UpdateWorkProgress(WorkProgressData progressData);

    /// <summary>
    /// Clears the crash recovery file after successful session restore or clean shutdown.
    /// </summary>
    /// <returns>A task that completes when the crash recovery file is cleared</returns>
    Task ClearCrashRecoveryAsync();
}
