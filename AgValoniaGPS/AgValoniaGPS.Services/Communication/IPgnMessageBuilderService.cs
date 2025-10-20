namespace AgValoniaGPS.Services.Communication;

/// <summary>
/// Service for building type-safe PGN (Parameter Group Number) messages.
/// Provides fluent API for constructing AutoSteer, Machine, IMU, and discovery messages
/// with proper format, byte ordering, and CRC validation.
/// </summary>
/// <remarks>
/// All methods return byte arrays in the standard PGN format:
/// [0x80, 0x81, source, PGN, length, ...data..., CRC]
///
/// CRC calculation matches existing PgnMessage.CalculateCrc() implementation:
/// Sum of all bytes from source through last data byte.
///
/// Performance requirement: &lt;5ms per message build operation.
/// </remarks>
public interface IPgnMessageBuilderService
{
    /// <summary>
    /// Build AutoSteer data message (PGN 254).
    /// Sends steering commands to AutoSteer module.
    /// </summary>
    /// <param name="speedMph">Vehicle speed in miles per hour (must be >= 0)</param>
    /// <param name="status">AutoSteer status: 0=off, 1=on, 2=error</param>
    /// <param name="steerAngle">Desired steering angle in degrees (positive = right)</param>
    /// <param name="crossTrackErrorMm">Cross-track error in millimeters</param>
    /// <returns>Byte array formatted as PGN 254 message</returns>
    /// <exception cref="ArgumentException">Thrown if speedMph is negative</exception>
    byte[] BuildAutoSteerData(double speedMph, byte status, double steerAngle, int crossTrackErrorMm);

    /// <summary>
    /// Build AutoSteer settings message (PGN 252).
    /// Configures PWM and PID parameters for AutoSteer module.
    /// </summary>
    /// <param name="pwmDrive">PWM drive level (0-255)</param>
    /// <param name="minPwm">Minimum PWM threshold (0-255)</param>
    /// <param name="proportionalGain">Proportional gain for PID control</param>
    /// <param name="highPwm">High PWM limit (0-255)</param>
    /// <param name="lowSpeedPwm">Low speed PWM multiplier</param>
    /// <returns>Byte array formatted as PGN 252 message</returns>
    byte[] BuildAutoSteerSettings(byte pwmDrive, byte minPwm, float proportionalGain, byte highPwm, float lowSpeedPwm);

    /// <summary>
    /// Build Machine data message (PGN 239).
    /// Sends relay states, tram line guidance, and section states to Machine module.
    /// </summary>
    /// <param name="relayLo">Low 8 relay states (must be length 8, 1 bit per relay)</param>
    /// <param name="relayHi">High 8 relay states (must be length 8, 1 bit per relay)</param>
    /// <param name="tramLine">Tram line guidance bits</param>
    /// <param name="sectionState">Section states (0=off, 1=on, 2=auto) - variable length</param>
    /// <returns>Byte array formatted as PGN 239 message</returns>
    /// <exception cref="ArgumentException">Thrown if relay arrays are not length 8</exception>
    /// <exception cref="ArgumentNullException">Thrown if any array parameter is null</exception>
    byte[] BuildMachineData(byte[] relayLo, byte[] relayHi, byte tramLine, byte[] sectionState);

    /// <summary>
    /// Build Machine configuration message (PGN 238).
    /// Configures section count, zones, and total implement width.
    /// </summary>
    /// <param name="sections">Number of sections (1-255)</param>
    /// <param name="zones">Number of zones (1-255)</param>
    /// <param name="totalWidth">Total implement width in centimeters</param>
    /// <returns>Byte array formatted as PGN 238 message</returns>
    byte[] BuildMachineConfig(ushort sections, ushort zones, ushort totalWidth);

    /// <summary>
    /// Build IMU configuration message (PGN 218).
    /// Configures IMU module settings and calibration.
    /// </summary>
    /// <param name="configFlags">Configuration bitmap for IMU settings</param>
    /// <returns>Byte array formatted as PGN 218 message</returns>
    byte[] BuildImuConfig(byte configFlags);

    /// <summary>
    /// Build scan request message (PGN 202).
    /// Requests all modules to respond with their capabilities.
    /// </summary>
    /// <returns>Byte array formatted as PGN 202 message</returns>
    byte[] BuildScanRequest();

    /// <summary>
    /// Build hello packet (PGN 200).
    /// Maintains backward compatibility with existing AgOpenGPS firmware.
    /// Format: [0x80, 0x81, 0x7F, 200, 3, 56, 0, 0, CRC]
    /// </summary>
    /// <returns>Byte array formatted as PGN 200 message</returns>
    byte[] BuildHelloPacket();
}
