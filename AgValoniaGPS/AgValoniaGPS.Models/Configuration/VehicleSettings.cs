namespace AgValoniaGPS.Models.Configuration;

/// <summary>
/// Vehicle physical configuration and dimensions.
/// Contains wheelbase, track, antenna positions, and steering limits.
/// </summary>
public class VehicleSettings
{
    /// <summary>
    /// Gets or sets the wheelbase in centimeters.
    /// Valid range: 50-500 cm.
    /// </summary>
    public double Wheelbase { get; set; } = 180.0;

    /// <summary>
    /// Gets or sets the track width in centimeters.
    /// Valid range: 10-400 cm.
    /// </summary>
    public double Track { get; set; } = 30.0;

    /// <summary>
    /// Gets or sets the maximum steering angle in degrees.
    /// Valid range: 10-90 degrees.
    /// </summary>
    public double MaxSteerAngle { get; set; } = 45.0;

    /// <summary>
    /// Gets or sets the maximum angular velocity in degrees per second.
    /// Valid range: 10-200 degrees/sec.
    /// </summary>
    public double MaxAngularVelocity { get; set; } = 100.0;

    /// <summary>
    /// Gets or sets the antenna pivot distance in centimeters.
    /// Distance from vehicle pivot point to antenna.
    /// </summary>
    public double AntennaPivot { get; set; } = 25.0;

    /// <summary>
    /// Gets or sets the antenna height in centimeters.
    /// Height of antenna above ground.
    /// </summary>
    public double AntennaHeight { get; set; } = 50.0;

    /// <summary>
    /// Gets or sets the antenna lateral offset in centimeters.
    /// Lateral distance from vehicle centerline.
    /// </summary>
    public double AntennaOffset { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the distance from pivot to antenna rear in centimeters.
    /// </summary>
    public double PivotBehindAnt { get; set; } = 30.0;

    /// <summary>
    /// Gets or sets the distance from steer axle ahead in centimeters.
    /// </summary>
    public double SteerAxleAhead { get; set; } = 110.0;

    /// <summary>
    /// Gets or sets the vehicle type.
    /// </summary>
    public VehicleType VehicleType { get; set; } = VehicleType.Tractor;

    /// <summary>
    /// Gets or sets the vehicle hitch length in centimeters.
    /// </summary>
    public double VehicleHitchLength { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the minimum U-turn radius in meters.
    /// Valid range: 1-20 meters.
    /// </summary>
    public double MinUturnRadius { get; set; } = 3.0;
}
