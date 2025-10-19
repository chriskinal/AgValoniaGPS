namespace AgValoniaGPS.Models.Section;

/// <summary>
/// Represents types of analog switches in the system
/// </summary>
public enum AnalogSwitchType
{
    /// <summary>
    /// Controls whether implement is engaged/disengaged
    /// </summary>
    WorkSwitch,

    /// <summary>
    /// Controls whether auto-steer is enabled/disabled
    /// </summary>
    SteerSwitch,

    /// <summary>
    /// Controls whether section control is locked/unlocked
    /// </summary>
    LockSwitch
}
