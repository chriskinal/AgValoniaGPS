namespace AgValoniaGPS.Models.Configuration;

/// <summary>
/// IMU (Inertial Measurement Unit) configuration including sensor fusion and calibration parameters.
/// </summary>
public class ImuSettings
{
    /// <summary>
    /// Gets or sets whether dual antenna is used as IMU.
    /// </summary>
    public bool DualAsImu { get; set; } = false;

    /// <summary>
    /// Gets or sets the dual antenna heading offset in degrees.
    /// Valid range: 0-359 degrees.
    /// </summary>
    public int DualHeadingOffset { get; set; } = 90;

    /// <summary>
    /// Gets or sets the IMU fusion weight factor.
    /// Valid range: 0.0-1.0.
    /// </summary>
    public double ImuFusionWeight { get; set; } = 0.06;

    /// <summary>
    /// Gets or sets the minimum heading step threshold in degrees.
    /// Valid range: 0.01-5.0 degrees.
    /// </summary>
    public double MinHeadingStep { get; set; } = 0.5;

    /// <summary>
    /// Gets or sets the minimum step limit.
    /// Valid range: 0.01-1.0.
    /// </summary>
    public double MinStepLimit { get; set; } = 0.05;

    /// <summary>
    /// Gets or sets the roll zero calibration in degrees.
    /// </summary>
    public double RollZero { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets whether roll measurement is inverted.
    /// </summary>
    public bool InvertRoll { get; set; } = false;

    /// <summary>
    /// Gets or sets the roll filter coefficient.
    /// Valid range: 0.0-1.0.
    /// </summary>
    public double RollFilter { get; set; } = 0.15;
}
