namespace AgValoniaGPS.Models.Configuration;

/// <summary>
/// Steering system configuration including wheel angle sensor calibration and PWM parameters.
/// </summary>
public class SteeringSettings
{
    /// <summary>
    /// Gets or sets the counts per degree (CPD) for the wheel angle sensor.
    /// Valid range: 1-1000.
    /// </summary>
    public int CountsPerDegree { get; set; } = 100;

    /// <summary>
    /// Gets or sets the Ackermann steering geometry compensation percentage.
    /// Valid range: 0-200 percent.
    /// </summary>
    public int Ackermann { get; set; } = 100;

    /// <summary>
    /// Gets or sets the wheel angle sensor offset calibration.
    /// Valid range: -10.0 to 10.0.
    /// </summary>
    public double WasOffset { get; set; } = 0.04;

    /// <summary>
    /// Gets or sets the high PWM value for steering motor control.
    /// Valid range: 0-255.
    /// </summary>
    public int HighPwm { get; set; } = 235;

    /// <summary>
    /// Gets or sets the low PWM value for steering motor control.
    /// Valid range: 0-255.
    /// </summary>
    public int LowPwm { get; set; } = 78;

    /// <summary>
    /// Gets or sets the minimum PWM value for steering motor control.
    /// Valid range: 0-255.
    /// </summary>
    public int MinPwm { get; set; } = 5;

    /// <summary>
    /// Gets or sets the maximum PWM value for steering motor control.
    /// Valid range: 0-255.
    /// </summary>
    public int MaxPwm { get; set; } = 10;

    /// <summary>
    /// Gets or sets the panic stop flag.
    /// 0 = disabled, 1 = enabled.
    /// </summary>
    public int PanicStop { get; set; } = 0;
}
