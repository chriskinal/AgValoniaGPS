namespace AgValoniaGPS.Models;

/// <summary>
/// Types of audio notifications available in the system.
/// Used by AudioNotificationService to play appropriate sounds for different events.
/// </summary>
public enum AudioNotificationType
{
    /// <summary>
    /// Boundary alarm - vehicle is approaching or has exited field boundary
    /// </summary>
    BoundaryAlarm = 0,

    /// <summary>
    /// U-turn too close warning
    /// </summary>
    UTurnTooClose = 1,

    /// <summary>
    /// Auto-steer engaged sound
    /// </summary>
    AutoSteerOn = 2,

    /// <summary>
    /// Auto-steer disengaged sound
    /// </summary>
    AutoSteerOff = 3,

    /// <summary>
    /// Hydraulic lift raised
    /// </summary>
    HydraulicLiftUp = 4,

    /// <summary>
    /// Hydraulic lift lowered
    /// </summary>
    HydraulicLiftDown = 5,

    /// <summary>
    /// RTK GPS fix lost alarm
    /// </summary>
    RTKAlarm = 6,

    /// <summary>
    /// RTK GPS fix recovered notification
    /// </summary>
    RTKRecovered = 7,

    /// <summary>
    /// Section turned on
    /// </summary>
    SectionOn = 8,

    /// <summary>
    /// Section turned off
    /// </summary>
    SectionOff = 9,

    /// <summary>
    /// Headland entry/exit notification
    /// </summary>
    Headland = 10
}
