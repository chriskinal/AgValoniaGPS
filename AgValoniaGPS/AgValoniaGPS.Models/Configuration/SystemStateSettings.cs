namespace AgValoniaGPS.Models.Configuration;

/// <summary>
/// System runtime state settings including steering algorithm selection and sensor readings.
/// </summary>
public class SystemStateSettings
{
    /// <summary>
    /// Gets or sets whether Stanley steering algorithm is in use.
    /// </summary>
    public bool StanleyUsed { get; set; } = false;

    /// <summary>
    /// Gets or sets whether steering in reverse is enabled.
    /// </summary>
    public bool SteerInReverse { get; set; } = false;

    /// <summary>
    /// Gets or sets whether reverse gear is engaged.
    /// </summary>
    public bool ReverseOn { get; set; } = true;

    /// <summary>
    /// Gets or sets the current heading in degrees.
    /// </summary>
    public double Heading { get; set; } = 20.6;

    /// <summary>
    /// Gets or sets the IMU-derived heading in degrees.
    /// </summary>
    public double ImuHeading { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the heading error value.
    /// </summary>
    public int HeadingError { get; set; } = 1;

    /// <summary>
    /// Gets or sets the distance error value.
    /// </summary>
    public int DistanceError { get; set; } = 1;

    /// <summary>
    /// Gets or sets the steering integral accumulator.
    /// </summary>
    public int SteerIntegral { get; set; } = 0;
}
