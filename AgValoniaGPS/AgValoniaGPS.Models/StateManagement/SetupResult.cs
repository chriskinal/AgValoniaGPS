namespace AgValoniaGPS.Models.StateManagement;

/// <summary>
/// Represents the result of completing the entire setup wizard.
/// </summary>
public class SetupResult
{
    /// <summary>
    /// Gets or sets whether the setup was completed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the error message if setup failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the name of the vehicle profile created during setup.
    /// </summary>
    public string? VehicleProfileName { get; set; }

    /// <summary>
    /// Gets or sets the name of the user profile created during setup.
    /// </summary>
    public string? UserProfileName { get; set; }
}
