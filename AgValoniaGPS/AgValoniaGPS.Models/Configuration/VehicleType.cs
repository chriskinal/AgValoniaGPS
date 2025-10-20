namespace AgValoniaGPS.Models.Configuration;

/// <summary>
/// Defines the type of agricultural vehicle.
/// </summary>
public enum VehicleType
{
    /// <summary>
    /// General purpose tractor.
    /// </summary>
    Tractor = 0,

    /// <summary>
    /// Combine harvester.
    /// </summary>
    Harvester = 1,

    /// <summary>
    /// Sprayer vehicle.
    /// </summary>
    Sprayer = 2,

    /// <summary>
    /// Planter or seeder.
    /// </summary>
    Planter = 3,

    /// <summary>
    /// Other vehicle type.
    /// </summary>
    Other = 99
}
