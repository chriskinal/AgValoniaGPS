using System;

namespace AgValoniaGPS.Services.Communication;

/// <summary>
/// Implementation of PGN message builder service.
/// Constructs byte-perfect PGN messages with correct format and CRC validation.
/// Reuses CRC calculation logic from existing PgnMessage class.
/// </summary>
/// <remarks>
/// Message format: [0x80, 0x81, source, PGN, length, ...data..., CRC]
/// - Header: Always 0x80, 0x81
/// - Source: 0x7F (from AgIO/AgOpenGPS)
/// - PGN: Parameter Group Number identifying message type
/// - Length: Number of data bytes
/// - Data: Variable length payload
/// - CRC: Sum of all bytes from source through last data byte
///
/// Thread-safe: All methods are stateless and can be called concurrently.
/// Performance: All message builds complete in &lt;5ms.
/// </remarks>
public class PgnMessageBuilderService : IPgnMessageBuilderService
{
    // Message header constants
    private const byte HEADER1 = 0x80;
    private const byte HEADER2 = 0x81;
    private const byte SOURCE_AGIO = 0x7F;

    // PGN numbers
    private const byte PGN_HELLO = 200;
    private const byte PGN_SCAN_REQUEST = 202;
    private const byte PGN_IMU_CONFIG = 218;
    private const byte PGN_MACHINE_CONFIG = 238;
    private const byte PGN_MACHINE_DATA = 239;
    private const byte PGN_STEER_SETTINGS = 252;
    private const byte PGN_AUTOSTEER_DATA = 254;

    public byte[] BuildAutoSteerData(double speedMph, byte status, double steerAngle, int crossTrackErrorMm)
    {
        // Validate inputs
        if (speedMph < 0)
            throw new ArgumentException("Speed cannot be negative", nameof(speedMph));

        // Data layout for PGN 254:
        // [0-1]: speedHi/speedLo - Speed * 10 (uint16, big-endian)
        // [2]: status - 0=off, 1=on, 2=error
        // [3-4]: steerAngleHi/steerAngleLo - Angle * 100 (int16, big-endian, degrees)
        // [5-6]: distanceHi/distanceLo - XTE in mm (int16, big-endian)
        // [7-8]: Reserved
        const int dataLength = 9;

        // Convert values to wire format
        ushort speedValue = (ushort)(speedMph * 10.0);
        short angleValue = (short)(steerAngle * 100.0);
        short xteValue = (short)crossTrackErrorMm;

        // Build message
        byte[] message = new byte[5 + dataLength + 1]; // Header(2) + Source(1) + PGN(1) + Length(1) + Data(9) + CRC(1)

        message[0] = HEADER1;
        message[1] = HEADER2;
        message[2] = SOURCE_AGIO;
        message[3] = PGN_AUTOSTEER_DATA;
        message[4] = dataLength;

        // Speed (uint16, big-endian)
        message[5] = (byte)(speedValue >> 8);
        message[6] = (byte)(speedValue & 0xFF);

        // Status
        message[7] = status;

        // Steer angle (int16, big-endian)
        message[8] = (byte)(angleValue >> 8);
        message[9] = (byte)(angleValue & 0xFF);

        // XTE (int16, big-endian)
        message[10] = (byte)(xteValue >> 8);
        message[11] = (byte)(xteValue & 0xFF);

        // Reserved bytes
        message[12] = 0;
        message[13] = 0;

        // Calculate and append CRC
        message[14] = CalculateCrc(message);

        return message;
    }

    public byte[] BuildAutoSteerSettings(byte pwmDrive, byte minPwm, float proportionalGain, byte highPwm, float lowSpeedPwm)
    {
        // Data layout for PGN 252:
        // [0]: pwmDrive - PWM drive level (0-255)
        // [1]: minPwm - Minimum PWM threshold
        // [2-3]: proportionalGain * 100 (uint16)
        // [4]: highPwm - High PWM limit
        // [5-6]: lowSpeedPwm * 100 (uint16)
        // [7-11]: Reserved for additional PID parameters
        const int dataLength = 12;

        // Convert values to wire format
        ushort pGainValue = (ushort)(proportionalGain * 100.0f);
        ushort lowSpeedValue = (ushort)(lowSpeedPwm * 100.0f);

        // Build message
        byte[] message = new byte[5 + dataLength + 1];

        message[0] = HEADER1;
        message[1] = HEADER2;
        message[2] = SOURCE_AGIO;
        message[3] = PGN_STEER_SETTINGS;
        message[4] = dataLength;

        message[5] = pwmDrive;
        message[6] = minPwm;

        // Proportional gain (uint16, big-endian)
        message[7] = (byte)(pGainValue >> 8);
        message[8] = (byte)(pGainValue & 0xFF);

        message[9] = highPwm;

        // Low speed PWM (uint16, big-endian)
        message[10] = (byte)(lowSpeedValue >> 8);
        message[11] = (byte)(lowSpeedValue & 0xFF);

        // Reserved bytes
        for (int i = 12; i <= 16; i++)
        {
            message[i] = 0;
        }

        // Calculate and append CRC
        message[17] = CalculateCrc(message);

        return message;
    }

    public byte[] BuildMachineData(byte[] relayLo, byte[] relayHi, byte tramLine, byte[] sectionState)
    {
        // Validate inputs
        if (relayLo == null)
            throw new ArgumentNullException(nameof(relayLo));
        if (relayHi == null)
            throw new ArgumentNullException(nameof(relayHi));
        if (sectionState == null)
            throw new ArgumentNullException(nameof(sectionState));
        if (relayLo.Length != 8)
            throw new ArgumentException("Relay Lo array must have length 8", nameof(relayLo));
        if (relayHi.Length != 8)
            throw new ArgumentException("Relay Hi array must have length 8", nameof(relayHi));

        // Data layout for PGN 239:
        // [0-7]: relayLo - Low 8 relay states (1 bit per relay)
        // [8-15]: relayHi - High 8 relay states
        // [16]: tramLine - Tram line guidance bits
        // [17-N]: sectionState - 1 byte per section (0=off, 1=on, 2=auto)
        int dataLength = 8 + 8 + 1 + sectionState.Length;

        // Build message
        byte[] message = new byte[5 + dataLength + 1];

        message[0] = HEADER1;
        message[1] = HEADER2;
        message[2] = SOURCE_AGIO;
        message[3] = PGN_MACHINE_DATA;
        message[4] = (byte)dataLength;

        // Copy relay states
        Array.Copy(relayLo, 0, message, 5, 8);
        Array.Copy(relayHi, 0, message, 13, 8);

        // Tram line
        message[21] = tramLine;

        // Section states
        Array.Copy(sectionState, 0, message, 22, sectionState.Length);

        // Calculate and append CRC
        message[^1] = CalculateCrc(message);

        return message;
    }

    public byte[] BuildMachineConfig(ushort sections, ushort zones, ushort totalWidth)
    {
        // Data layout for PGN 238:
        // [0-1]: sections - Number of sections (uint16)
        // [2-3]: zones - Number of zones (uint16)
        // [4-5]: totalWidth - Total implement width in cm (uint16)
        const int dataLength = 6;

        // Build message
        byte[] message = new byte[5 + dataLength + 1];

        message[0] = HEADER1;
        message[1] = HEADER2;
        message[2] = SOURCE_AGIO;
        message[3] = PGN_MACHINE_CONFIG;
        message[4] = dataLength;

        // Sections (uint16, big-endian)
        message[5] = (byte)(sections >> 8);
        message[6] = (byte)(sections & 0xFF);

        // Zones (uint16, big-endian)
        message[7] = (byte)(zones >> 8);
        message[8] = (byte)(zones & 0xFF);

        // Total width (uint16, big-endian)
        message[9] = (byte)(totalWidth >> 8);
        message[10] = (byte)(totalWidth & 0xFF);

        // Calculate and append CRC
        message[11] = CalculateCrc(message);

        return message;
    }

    public byte[] BuildImuConfig(byte configFlags)
    {
        // Data layout for PGN 218:
        // [0]: configFlags - Configuration bitmap
        // [1-3]: Reserved
        const int dataLength = 4;

        // Build message
        byte[] message = new byte[5 + dataLength + 1];

        message[0] = HEADER1;
        message[1] = HEADER2;
        message[2] = SOURCE_AGIO;
        message[3] = PGN_IMU_CONFIG;
        message[4] = dataLength;

        message[5] = configFlags;
        message[6] = 0; // Reserved
        message[7] = 0; // Reserved
        message[8] = 0; // Reserved

        // Calculate and append CRC
        message[9] = CalculateCrc(message);

        return message;
    }

    public byte[] BuildScanRequest()
    {
        // Data layout for PGN 202:
        // [0-1]: Request type and reserved
        const int dataLength = 2;

        // Build message
        byte[] message = new byte[5 + dataLength + 1];

        message[0] = HEADER1;
        message[1] = HEADER2;
        message[2] = SOURCE_AGIO;
        message[3] = PGN_SCAN_REQUEST;
        message[4] = dataLength;

        message[5] = 0; // Request type
        message[6] = 0; // Reserved

        // Calculate and append CRC
        message[7] = CalculateCrc(message);

        return message;
    }

    public byte[] BuildHelloPacket()
    {
        // Maintain backward compatibility with existing format:
        // [0x80, 0x81, 0x7F, 200, 3, 56, 0, 0, CRC]
        const int dataLength = 3;

        // Build message
        byte[] message = new byte[5 + dataLength + 1];

        message[0] = HEADER1;
        message[1] = HEADER2;
        message[2] = SOURCE_AGIO;
        message[3] = PGN_HELLO;
        message[4] = dataLength;

        // Specific values from existing implementation
        message[5] = 56;
        message[6] = 0;
        message[7] = 0;

        // Calculate and append CRC
        message[8] = CalculateCrc(message);

        return message;
    }

    /// <summary>
    /// Calculate CRC checksum using same algorithm as PgnMessage.CalculateCrc().
    /// CRC is the sum of all bytes from source (index 2) through last data byte.
    /// </summary>
    /// <param name="message">The message array (must include space for CRC at end)</param>
    /// <returns>CRC checksum byte</returns>
    private byte CalculateCrc(byte[] message)
    {
        byte crc = 0;
        // Sum from source byte (index 2) through last data byte (length - 2, excluding CRC position)
        for (int i = 2; i < message.Length - 1; i++)
        {
            crc += message[i];
        }
        return crc;
    }
}
