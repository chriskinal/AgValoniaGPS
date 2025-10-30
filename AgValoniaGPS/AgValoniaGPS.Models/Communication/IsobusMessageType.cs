namespace AgValoniaGPS.Models.Communication;

/// <summary>
/// ISO 11783 (ISOBUS) message types used in agricultural equipment communication.
/// </summary>
public enum IsobusMessageType
{
    /// <summary>
    /// Heartbeat message from ISOBUS device (status and section states)
    /// </summary>
    Heartbeat = 0x00,

    /// <summary>
    /// Section control enable/disable request
    /// </summary>
    SectionControlRequest = 0xF1,

    /// <summary>
    /// Process data (guidance line deviation, speed, distance)
    /// </summary>
    ProcessData = 0xF2,

    /// <summary>
    /// Work state information
    /// </summary>
    WorkState = 0xF3,

    /// <summary>
    /// Section control command (turn sections on/off)
    /// </summary>
    SectionControlCommand = 0xEF,

    /// <summary>
    /// Unknown or unsupported message type
    /// </summary>
    Unknown = 0xFF
}
