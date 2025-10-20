using AgValoniaGPS.Models.Communication;

namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for IMU data reception events.
/// Raised when inertial measurement data is received from the IMU module.
/// </summary>
public class ImuDataReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the IMU data.
    /// </summary>
    public readonly Communication.ImuData Data;

    /// <summary>
    /// Creates a new instance of ImuDataReceivedEventArgs.
    /// </summary>
    /// <param name="data">IMU data</param>
    public ImuDataReceivedEventArgs(Communication.ImuData data)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }
}
