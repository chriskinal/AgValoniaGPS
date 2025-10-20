using System;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Session;

namespace AgValoniaGPS.Services.Session;

/// <summary>
/// Handles crash recovery file I/O operations for session state persistence.
/// Provides atomic write operations and stale file detection.
/// </summary>
public interface ICrashRecoveryService
{
    /// <summary>
    /// Saves a session state snapshot to the crash recovery file.
    /// Uses atomic write (write to temp file, then rename) for data integrity.
    /// </summary>
    /// <param name="sessionState">The session state to save</param>
    /// <returns>A task that completes when the snapshot is saved</returns>
    Task SaveSnapshotAsync(SessionState sessionState);

    /// <summary>
    /// Restores session state from the crash recovery file.
    /// Checks file age and considers files older than 24 hours as stale.
    /// </summary>
    /// <returns>Session restore result with success status and recovered data</returns>
    Task<SessionRestoreResult> RestoreSnapshotAsync();

    /// <summary>
    /// Clears the crash recovery file.
    /// Called after successful restore or clean shutdown.
    /// </summary>
    /// <returns>A task that completes when the file is cleared</returns>
    Task ClearSnapshotAsync();

    /// <summary>
    /// Checks if a crash recovery file exists.
    /// </summary>
    /// <returns>True if crash recovery file exists, false otherwise</returns>
    bool HasCrashRecoveryFile();
}
