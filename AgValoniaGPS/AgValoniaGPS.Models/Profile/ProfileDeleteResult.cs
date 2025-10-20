namespace AgValoniaGPS.Models.Profile;

/// <summary>
/// Represents the result of a profile deletion operation.
/// </summary>
public class ProfileDeleteResult
{
    /// <summary>
    /// Gets or sets whether the profile deletion succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the error message if deletion failed.
    /// Empty if successful.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}
