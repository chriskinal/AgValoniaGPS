namespace AgValoniaGPS.Models.Profile;

/// <summary>
/// Represents the result of a profile switch operation.
/// </summary>
public class ProfileSwitchResult
{
    /// <summary>
    /// Gets or sets whether the profile switch succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the error message if the switch failed.
    /// Empty if successful.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the session data was carried over to the new profile.
    /// Only applicable for vehicle profile switches.
    /// </summary>
    public bool SessionCarriedOver { get; set; }
}
