using System;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Models.Session;

/// <summary>
/// Represents the current application session state for crash recovery and session persistence.
/// Contains all necessary information to restore a user's work after an unexpected shutdown.
/// </summary>
public class SessionState
{
    /// <summary>
    /// Gets or sets the timestamp when the session was started.
    /// </summary>
    public DateTime SessionStartTime { get; set; }

    /// <summary>
    /// Gets or sets the name of the currently active field.
    /// </summary>
    public string CurrentFieldName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of guidance line currently in use.
    /// </summary>
    public GuidanceLineType CurrentGuidanceLineType { get; set; } = GuidanceLineType.None;

    /// <summary>
    /// Gets or sets the current guidance line data (ABLine, CurveLine, or Contour object).
    /// Serialized as JSON object.
    /// </summary>
    public object? CurrentGuidanceLineData { get; set; }

    /// <summary>
    /// Gets or sets the current work progress data including area covered and distance traveled.
    /// </summary>
    public WorkProgressData WorkProgress { get; set; } = new();

    /// <summary>
    /// Gets or sets the name of the currently active vehicle profile.
    /// </summary>
    public string VehicleProfileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the currently active user profile.
    /// </summary>
    public string UserProfileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp of the last crash recovery snapshot.
    /// Updated every 30 seconds during normal operation.
    /// </summary>
    public DateTime LastSnapshotTime { get; set; }
}
