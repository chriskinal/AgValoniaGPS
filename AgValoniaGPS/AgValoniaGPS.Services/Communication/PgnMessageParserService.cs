using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Communication;

using ImuDataModel = AgValoniaGPS.Models.Communication.ImuData;
namespace AgValoniaGPS.Services.Communication;

/// <summary>
/// Service for parsing inbound PGN (Parameter Group Number) messages from hardware modules.
/// Validates message format, CRC checksums, and converts binary data to domain models.
/// Never throws exceptions for malformed data - returns null for invalid messages.
/// </summary>
public class PgnMessageParserService : IPgnMessageParserService
{
    /// <summary>
    /// Parse a generic PGN message from raw bytes.
    /// Validates header and CRC but doesn't parse type-specific data.
    /// </summary>
    public PgnMessage? ParseMessage(byte[] data)
    {
        if (!ValidateMessageStructure(data))
            return null;

        if (!ValidateCrc(data))
            return null;

        return PgnMessage.FromBytes(data);
    }

    /// <summary>
    /// Parse AutoSteer configuration response.
    /// Contains firmware version, capabilities, and steering angle limits.
    /// </summary>
    public AutoSteerConfigResponse? ParseAutoSteerConfig(byte[] data)
    {
        var message = ParseMessage(data);
        if (message == null || message.Length < 8)
            return null;

        try
        {
            return new AutoSteerConfigResponse
            {
                Version = message.Data[0],
                Capabilities = message.Data.Length > 1 ? new[] { message.Data[1] } : Array.Empty<byte>(),
                MaxSteerAngle = message.Data.Length >= 4 ? ConvertInt16(message.Data[2], message.Data[3]) / 100.0 : 0,
                MinSteerAngle = message.Data.Length >= 6 ? ConvertInt16(message.Data[4], message.Data[5]) / 100.0 : 0
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parse AutoSteer feedback data (PGN 253).
    /// Contains actual wheel angle, switch states, and status flags.
    /// </summary>
    public AutoSteerFeedback? ParseAutoSteerData(byte[] data)
    {
        var message = ParseMessage(data);
        if (message == null || message.PGN != 253 || message.Length < 4)
            return null;

        try
        {
            // Parse actual wheel angle from first two bytes (int16 * 100)
            short angleRaw = ConvertInt16(message.Data[0], message.Data[1]);
            double actualAngle = angleRaw / 100.0;

            // Parse switch states (byte 2)
            byte switchStates = message.Data.Length > 2 ? message.Data[2] : (byte)0;

            // Parse status flags (byte 3)
            byte statusFlags = message.Data.Length > 3 ? message.Data[3] : (byte)0;

            return new AutoSteerFeedback
            {
                ActualWheelAngle = actualAngle,
                SwitchStates = new[] { switchStates },
                StatusFlags = statusFlags,
                Timestamp = DateTime.UtcNow
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parse Machine configuration response.
    /// Contains firmware version, max sections, and max zones.
    /// </summary>
    public MachineConfigResponse? ParseMachineConfig(byte[] data)
    {
        var message = ParseMessage(data);
        if (message == null || message.Length < 5)
            return null;

        try
        {
            return new MachineConfigResponse
            {
                Version = message.Data[0],
                MaxSections = ConvertUInt16(message.Data[1], message.Data[2]),
                MaxZones = message.Data.Length >= 5 ? ConvertUInt16(message.Data[3], message.Data[4]) : (ushort)0
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parse Machine feedback data (PGN 234).
    /// Contains work switch state and section sensor feedback.
    /// </summary>
    public MachineFeedback? ParseMachineData(byte[] data)
    {
        var message = ParseMessage(data);
        if (message == null || message.PGN != 234 || message.Length < 1)
            return null;

        try
        {
            bool workSwitchActive = message.Data[0] == 0x01;
            byte statusFlags = message.Data.Length > 1 ? message.Data[1] : (byte)0;

            // Section sensors are in remaining bytes (if any)
            byte[] sectionSensors = Array.Empty<byte>();
            if (message.Data.Length > 2)
            {
                sectionSensors = new byte[message.Data.Length - 2];
                Array.Copy(message.Data, 2, sectionSensors, 0, sectionSensors.Length);
            }

            return new MachineFeedback
            {
                WorkSwitchActive = workSwitchActive,
                StatusFlags = statusFlags,
                SectionSensors = sectionSensors,
                Timestamp = DateTime.UtcNow
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parse IMU data (PGN 219).
    /// Contains roll, pitch, heading, yaw rate, and calibration status.
    /// </summary>
    public ImuDataModel? ParseImuData(byte[] data)
    {
        var message = ParseMessage(data);
        if (message == null || message.PGN != 219 || message.Length < 9)
            return null;

        try
        {
            // Parse roll (int16 * 100) - bytes 0-1
            double roll = ConvertInt16(message.Data[0], message.Data[1]) / 100.0;

            // Parse pitch (int16 * 100) - bytes 2-3
            double pitch = ConvertInt16(message.Data[2], message.Data[3]) / 100.0;

            // Parse heading (uint16 * 100) - bytes 4-5
            double heading = ConvertUInt16(message.Data[4], message.Data[5]) / 100.0;

            // Parse yaw rate (int16 * 100) - bytes 6-7
            double yawRate = ConvertInt16(message.Data[6], message.Data[7]) / 100.0;

            // Parse calibration flag - byte 8
            bool isCalibrated = message.Data[8] == 0x01;

            return new ImuDataModel
            {
                Roll = roll,
                Pitch = pitch,
                Heading = heading,
                YawRate = yawRate,
                IsCalibrated = isCalibrated,
                Timestamp = DateTime.UtcNow
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parse scan response from module discovery.
    /// Contains module type, version, and capabilities.
    /// </summary>
    public ScanResponse? ParseScanResponse(byte[] data)
    {
        var message = ParseMessage(data);
        if (message == null || message.PGN != PgnNumbers.SCAN_REPLY || message.Length < 2)
            return null;

        try
        {
            // Module type from first byte (0 = AutoSteer, 1 = Machine, 2 = IMU)
            ModuleType moduleType = message.Data[0] switch
            {
                0 => ModuleType.AutoSteer,
                1 => ModuleType.Machine,
                2 => ModuleType.IMU,
                _ => ModuleType.AutoSteer
            };

            byte version = message.Data[1];

            // Capabilities from remaining bytes (if any)
            byte[] capabilities = Array.Empty<byte>();
            if (message.Data.Length > 2)
            {
                capabilities = new byte[message.Data.Length - 2];
                Array.Copy(message.Data, 2, capabilities, 0, capabilities.Length);
            }

            return new ScanResponse
            {
                ModuleType = moduleType,
                Version = version,
                Capabilities = capabilities
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parse hello packet from a module (PGN 126/123/121).
    /// Identifies module type from PGN number and extracts version if available.
    /// </summary>
    public HelloResponse? ParseHelloPacket(byte[] data)
    {
        var message = ParseMessage(data);
        if (message == null)
            return null;

        // Identify module type from PGN number
        ModuleType moduleType = message.PGN switch
        {
            PgnNumbers.HELLO_FROM_AUTOSTEER => ModuleType.AutoSteer,
            PgnNumbers.HELLO_FROM_MACHINE => ModuleType.Machine,
            PgnNumbers.HELLO_FROM_IMU => ModuleType.IMU,
            _ => ModuleType.AutoSteer // Default fallback
        };

        // Don't validate for known hello PGNs - accept any hello-like message
        if (message.PGN != PgnNumbers.HELLO_FROM_AUTOSTEER &&
            message.PGN != PgnNumbers.HELLO_FROM_MACHINE &&
            message.PGN != PgnNumbers.HELLO_FROM_IMU)
        {
            return null;
        }

        try
        {
            // Version is in first data byte (if available)
            byte version = message.Data.Length > 0 ? message.Data[0] : (byte)0;

            return new HelloResponse
            {
                ModuleType = moduleType,
                Version = version,
                ReceivedAt = DateTime.UtcNow
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Validate CRC checksum of a PGN message.
    /// Uses simple sum algorithm matching PgnMessage.CalculateCrc().
    /// </summary>
    public bool ValidateCrc(byte[] data)
    {
        if (data.Length < 6)
            return false;

        byte expectedCrc = data[data.Length - 1];
        byte actualCrc = CalculateCrc(data);

        return expectedCrc == actualCrc;
    }

    /// <summary>
    /// Calculate CRC checksum for a PGN message.
    /// Sums all bytes from source (index 2) to end-1.
    /// Matches logic in PgnMessage.CalculateCrc().
    /// </summary>
    public byte CalculateCrc(byte[] data)
    {
        if (data.Length < 3)
            return 0;

        byte crc = 0;
        for (int i = 2; i < data.Length - 1; i++)
        {
            crc += data[i];
        }
        return crc;
    }

    /// <summary>
    /// Validate message structure (header bytes, minimum length).
    /// Does not validate CRC - use ValidateCrc() separately.
    /// </summary>
    private bool ValidateMessageStructure(byte[] data)
    {
        if (data == null || data.Length < 6)
            return false;

        // Validate header bytes
        if (data[0] != PgnMessage.HEADER1 || data[1] != PgnMessage.HEADER2)
            return false;

        // Validate declared length matches actual data
        byte declaredLength = data[4];
        int expectedTotalLength = 5 + declaredLength + 1; // header(2) + source + pgn + length + data + crc
        if (data.Length < expectedTotalLength)
            return false;

        return true;
    }

    /// <summary>
    /// Convert two bytes to signed 16-bit integer (big-endian).
    /// </summary>
    private short ConvertInt16(byte hi, byte lo)
    {
        return (short)((hi << 8) | lo);
    }

    /// <summary>
    /// Convert two bytes to unsigned 16-bit integer (big-endian).
    /// </summary>
    private ushort ConvertUInt16(byte hi, byte lo)
    {
        return (ushort)((hi << 8) | lo);
    }
}
