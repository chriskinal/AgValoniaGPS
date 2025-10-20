using System;
using AgValoniaGPS.Models.Configuration;

namespace AgValoniaGPS.Models.Profile;

/// <summary>
/// Represents a complete vehicle profile containing all configuration settings
/// for a specific agricultural vehicle.
/// </summary>
public class VehicleProfile
{
    /// <summary>
    /// Gets or sets the unique name of this vehicle profile.
    /// Used as the profile identifier and filename.
    /// </summary>
    public string VehicleName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when this vehicle profile was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this vehicle profile was last modified.
    /// Updated whenever settings are saved.
    /// </summary>
    public DateTime LastModifiedDate { get; set; }

    /// <summary>
    /// Gets or sets all application settings for this vehicle.
    /// Contains vehicle dimensions, steering parameters, GPS settings, and all other configuration.
    /// </summary>
    public ApplicationSettings Settings { get; set; } = new();
}
