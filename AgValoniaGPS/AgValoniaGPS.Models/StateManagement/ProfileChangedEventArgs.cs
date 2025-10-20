using System;

namespace AgValoniaGPS.Models.StateManagement;

/// <summary>
/// Event arguments for profile change notifications.
/// Provides context about which profile type changed and session carry-over status.
/// </summary>
public class ProfileChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the type of profile that changed.
    /// </summary>
    public ProfileType ProfileType { get; set; }

    /// <summary>
    /// Gets or sets the name of the new profile.
    /// </summary>
    public string ProfileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the session state was carried over during profile switch.
    /// Only applicable for vehicle profile switches.
    /// </summary>
    public bool SessionCarriedOver { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the profile change occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Enumeration of profile types for profile switch notifications.
/// </summary>
public enum ProfileType
{
    /// <summary>
    /// Vehicle profile (contains vehicle-specific settings).
    /// </summary>
    Vehicle,

    /// <summary>
    /// User profile (contains user preferences).
    /// </summary>
    User
}
