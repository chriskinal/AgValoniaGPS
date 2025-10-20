using System;

namespace AgValoniaGPS.Models.Profile;

/// <summary>
/// Represents a user profile containing personal preferences and display settings.
/// Multiple users can share the same vehicle but maintain separate preferences.
/// </summary>
public class UserProfile
{
    /// <summary>
    /// Gets or sets the unique name of this user profile.
    /// Used as the profile identifier and filename.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when this user profile was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this user profile was last modified.
    /// Updated whenever preferences are saved.
    /// </summary>
    public DateTime LastModifiedDate { get; set; }

    /// <summary>
    /// Gets or sets the user's personal preferences including unit system,
    /// language, and display settings.
    /// </summary>
    public UserPreferences Preferences { get; set; } = new();
}
