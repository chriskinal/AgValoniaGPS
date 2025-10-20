namespace AgValoniaGPS.Models.Communication;

/// <summary>
/// Specifies the type of hardware module in the precision agriculture system.
/// </summary>
public enum ModuleType
{
    /// <summary>
    /// AutoSteer module for automatic steering control
    /// </summary>
    AutoSteer,

    /// <summary>
    /// Machine module for section control and implement management
    /// </summary>
    Machine,

    /// <summary>
    /// IMU (Inertial Measurement Unit) module for roll, pitch, and heading data
    /// </summary>
    IMU
}
