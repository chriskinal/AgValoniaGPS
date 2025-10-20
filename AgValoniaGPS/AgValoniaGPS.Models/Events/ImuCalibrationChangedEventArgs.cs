namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for IMU calibration state change events.
/// Raised when the IMU calibration status changes.
/// </summary>
public class ImuCalibrationChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets whether the IMU is calibrated.
    /// </summary>
    public readonly bool IsCalibrated;

    /// <summary>
    /// Gets the timestamp when the calibration state changed.
    /// </summary>
    public readonly DateTime ChangedAt;

    /// <summary>
    /// Creates a new instance of ImuCalibrationChangedEventArgs.
    /// </summary>
    /// <param name="isCalibrated">IMU calibration state</param>
    public ImuCalibrationChangedEventArgs(bool isCalibrated)
    {
        IsCalibrated = isCalibrated;
        ChangedAt = DateTime.UtcNow;
    }
}
