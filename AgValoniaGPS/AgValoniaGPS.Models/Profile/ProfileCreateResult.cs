namespace AgValoniaGPS.Models.Profile;

/// <summary>
/// Represents the result of a profile creation operation.
/// </summary>
public class ProfileCreateResult
{
    /// <summary>
    /// Gets or sets whether the profile creation succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the error message if creation failed.
    /// Empty if successful.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}
