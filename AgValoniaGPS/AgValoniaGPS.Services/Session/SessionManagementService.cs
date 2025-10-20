using System;
using System.Threading;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Models.Session;
using AgValoniaGPS.Models.StateManagement;

namespace AgValoniaGPS.Services.Session;

/// <summary>
/// Manages application session state with crash recovery capabilities.
/// Provides periodic snapshots every 30 seconds for crash recovery.
/// </summary>
public class SessionManagementService : ISessionManagementService
{
    private const int SnapshotIntervalSeconds = 30;

    private readonly ICrashRecoveryService _crashRecoveryService;
    private readonly object _lock = new();

    private SessionState? _currentSessionState;
    private Timer? _snapshotTimer;

    /// <summary>
    /// Raised when session state changes.
    /// </summary>
    public event EventHandler<SessionStateChangedEventArgs>? SessionStateChanged;

    /// <summary>
    /// Initializes a new instance of the SessionManagementService class.
    /// </summary>
    /// <param name="crashRecoveryService">The crash recovery service for file I/O</param>
    public SessionManagementService(ICrashRecoveryService crashRecoveryService)
    {
        _crashRecoveryService = crashRecoveryService ?? throw new ArgumentNullException(nameof(crashRecoveryService));
    }

    /// <summary>
    /// Starts a new session and begins periodic crash recovery snapshots.
    /// </summary>
    public async Task StartSessionAsync()
    {
        lock (_lock)
        {
            // Create new session state
            _currentSessionState = new SessionState
            {
                SessionStartTime = DateTime.UtcNow,
                LastSnapshotTime = DateTime.UtcNow
            };

            // Start periodic snapshot timer (30 second interval)
            _snapshotTimer = new Timer(
                async _ => await SaveSessionSnapshotAsync(),
                null,
                TimeSpan.FromSeconds(SnapshotIntervalSeconds),
                TimeSpan.FromSeconds(SnapshotIntervalSeconds)
            );
        }

        // Raise session started event
        SessionStateChanged?.Invoke(this, new SessionStateChangedEventArgs(SessionStateChangeType.SessionStarted, _currentSessionState));

        await Task.CompletedTask;
    }

    /// <summary>
    /// Ends the current session and stops periodic snapshots.
    /// </summary>
    public async Task EndSessionAsync()
    {
        SessionState? endedSession;

        lock (_lock)
        {
            // Stop snapshot timer
            _snapshotTimer?.Dispose();
            _snapshotTimer = null;

            endedSession = _currentSessionState;
            _currentSessionState = null;
        }

        // Raise session ended event
        if (endedSession != null)
        {
            SessionStateChanged?.Invoke(this, new SessionStateChangedEventArgs(SessionStateChangeType.SessionEnded, endedSession));
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Saves current session state as a crash recovery snapshot.
    /// Must complete in less than 500ms per performance requirements.
    /// </summary>
    public async Task SaveSessionSnapshotAsync()
    {
        SessionState? snapshot;

        lock (_lock)
        {
            if (_currentSessionState == null)
            {
                return;
            }

            // Create a snapshot copy to avoid locking during I/O
            snapshot = new SessionState
            {
                SessionStartTime = _currentSessionState.SessionStartTime,
                CurrentFieldName = _currentSessionState.CurrentFieldName,
                CurrentGuidanceLineType = _currentSessionState.CurrentGuidanceLineType,
                CurrentGuidanceLineData = _currentSessionState.CurrentGuidanceLineData,
                WorkProgress = _currentSessionState.WorkProgress,
                VehicleProfileName = _currentSessionState.VehicleProfileName,
                UserProfileName = _currentSessionState.UserProfileName,
                LastSnapshotTime = DateTime.UtcNow
            };
        }

        try
        {
            // Save snapshot (non-blocking I/O)
            await _crashRecoveryService.SaveSnapshotAsync(snapshot);

            // Update last snapshot time
            lock (_lock)
            {
                if (_currentSessionState != null)
                {
                    _currentSessionState.LastSnapshotTime = snapshot.LastSnapshotTime;
                }
            }

            // Raise snapshot saved event
            SessionStateChanged?.Invoke(this, new SessionStateChangedEventArgs(SessionStateChangeType.SnapshotSaved, snapshot));
        }
        catch (Exception)
        {
            // Log error but don't throw - snapshot failures shouldn't crash the app
            // In production, this would log to a proper logging system
        }
    }

    /// <summary>
    /// Attempts to restore the last session from crash recovery file.
    /// </summary>
    public async Task<SessionRestoreResult> RestoreLastSessionAsync()
    {
        return await _crashRecoveryService.RestoreSnapshotAsync();
    }

    /// <summary>
    /// Gets the current session state.
    /// </summary>
    public SessionState? GetCurrentSessionState()
    {
        lock (_lock)
        {
            return _currentSessionState;
        }
    }

    /// <summary>
    /// Updates the current field name in the session state.
    /// </summary>
    public void UpdateCurrentField(string fieldName)
    {
        lock (_lock)
        {
            if (_currentSessionState != null)
            {
                _currentSessionState.CurrentFieldName = fieldName ?? string.Empty;
            }
        }

        SessionStateChanged?.Invoke(this, new SessionStateChangedEventArgs(SessionStateChangeType.FieldUpdated, fieldName));
    }

    /// <summary>
    /// Updates the current guidance line in the session state.
    /// </summary>
    public void UpdateCurrentGuidanceLine(GuidanceLineType lineType, object? lineData)
    {
        lock (_lock)
        {
            if (_currentSessionState != null)
            {
                _currentSessionState.CurrentGuidanceLineType = lineType;
                _currentSessionState.CurrentGuidanceLineData = lineData;
            }
        }

        SessionStateChanged?.Invoke(this, new SessionStateChangedEventArgs(SessionStateChangeType.GuidanceLineUpdated, lineData));
    }

    /// <summary>
    /// Updates the work progress data in the session state.
    /// </summary>
    public void UpdateWorkProgress(WorkProgressData progressData)
    {
        lock (_lock)
        {
            if (_currentSessionState != null)
            {
                _currentSessionState.WorkProgress = progressData ?? new WorkProgressData();
            }
        }

        SessionStateChanged?.Invoke(this, new SessionStateChangedEventArgs(SessionStateChangeType.WorkProgressUpdated, progressData));
    }

    /// <summary>
    /// Clears the crash recovery file after successful restore or clean shutdown.
    /// </summary>
    public async Task ClearCrashRecoveryAsync()
    {
        await _crashRecoveryService.ClearSnapshotAsync();
    }
}
