using System;
using AgValoniaGPS.Models.StateManagement;

namespace AgValoniaGPS.Services.StateManagement;

/// <summary>
/// Represents a state change notification with metadata.
/// Common payload structure for all state change events.
/// </summary>
public class StateChangeNotification
{
    /// <summary>
    /// Gets or sets the type of state change.
    /// </summary>
    public StateChangeType ChangeType { get; set; }

    /// <summary>
    /// Gets or sets the settings category (for settings changes).
    /// Null for non-settings changes.
    /// </summary>
    public SettingsCategory? Category { get; set; }

    /// <summary>
    /// Gets or sets the profile type (for profile switches).
    /// Null for non-profile changes.
    /// </summary>
    public ProfileType? ProfileType { get; set; }

    /// <summary>
    /// Gets or sets the session state change type (for session state changes).
    /// Null for non-session changes.
    /// </summary>
    public SessionStateChangeType? SessionChangeType { get; set; }

    /// <summary>
    /// Gets or sets the data associated with the state change.
    /// Type depends on the change type:
    /// - Settings change: new settings object
    /// - Profile switch: profile name (string)
    /// - Session change: session state data
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the change occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a notification for a settings change.
    /// </summary>
    /// <param name="category">The settings category that changed</param>
    /// <param name="newSettings">The new settings object</param>
    /// <returns>A configured StateChangeNotification</returns>
    public static StateChangeNotification ForSettingsChange(SettingsCategory category, object newSettings)
    {
        return new StateChangeNotification
        {
            ChangeType = StateChangeType.Settings,
            Category = category,
            Data = newSettings
        };
    }

    /// <summary>
    /// Creates a notification for a profile switch.
    /// </summary>
    /// <param name="profileType">The type of profile that switched</param>
    /// <param name="profileName">The name of the new profile</param>
    /// <returns>A configured StateChangeNotification</returns>
    public static StateChangeNotification ForProfileSwitch(ProfileType profileType, string profileName)
    {
        return new StateChangeNotification
        {
            ChangeType = StateChangeType.Profile,
            ProfileType = profileType,
            Data = profileName
        };
    }

    /// <summary>
    /// Creates a notification for a session state change.
    /// </summary>
    /// <param name="sessionChangeType">The type of session state change</param>
    /// <param name="stateData">Optional state data associated with the change</param>
    /// <returns>A configured StateChangeNotification</returns>
    public static StateChangeNotification ForSessionStateChange(
        SessionStateChangeType sessionChangeType,
        object? stateData = null)
    {
        return new StateChangeNotification
        {
            ChangeType = StateChangeType.Session,
            SessionChangeType = sessionChangeType,
            Data = stateData
        };
    }
}

/// <summary>
/// High-level categorization of state changes.
/// </summary>
public enum StateChangeType
{
    /// <summary>
    /// Application settings changed.
    /// </summary>
    Settings,

    /// <summary>
    /// Active profile switched.
    /// </summary>
    Profile,

    /// <summary>
    /// Session state changed.
    /// </summary>
    Session
}
