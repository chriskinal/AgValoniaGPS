using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Communication;
using AgValoniaGPS.Services.Communication;
using ImuDataModel = AgValoniaGPS.Models.Communication.ImuData;
using Xunit;

namespace AgValoniaGPS.Services.Tests.Communication;

/// <summary>
/// Unit tests for PgnMessageParserService.
/// Tests parsing of all inbound PGN message types with CRC validation.
/// </summary>
public class PgnMessageParserServiceTests
{
    private readonly PgnMessageParserService _parser;

    public PgnMessageParserServiceTests()
    {
        _parser = new PgnMessageParserService();
    }

    [Fact]
    public void ParseAutoSteerData_ValidMessage_ParsesCorrectly()
    {
        // Arrange - PGN 253 AutoSteer feedback: actual angle = 5.25 degrees, switch states, status flags
        // Format: [header1, header2, source, pgn, length, actualAngleHi, actualAngleLo, switchStates, statusFlags, reserved..., crc]
        // Angle = 5.25 * 100 = 525 = 0x020D (big-endian: 0x02, 0x0D)
        var message = new byte[]
        {
            0x80, 0x81,  // Header
            0x7E,        // Source (AutoSteer module)
            253,         // PGN
            8,           // Length
            0x02, 0x0D,  // Actual angle (525 = 5.25 degrees)
            0x05,        // Switch states bitmap
            0x01,        // Status flags
            0x00, 0x00, 0x00, 0x00, // Reserved
            0x00         // CRC (will be calculated)
        };
        message[message.Length - 1] = CalculateTestCrc(message);

        // Act
        var result = _parser.ParseAutoSteerData(message);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5.25, result.ActualWheelAngle, 0.01);
        Assert.Single(result.SwitchStates);
        Assert.Equal(0x05, result.SwitchStates[0]);
        Assert.Equal(0x01, result.StatusFlags);
    }

    [Fact]
    public void ParseMachineData_WorkSwitchActive_ParsesCorrectly()
    {
        // Arrange - PGN 234 Work Switch State: active = true, status flags
        var message = new byte[]
        {
            0x80, 0x81,  // Header
            0x7B,        // Source (Machine module)
            234,         // PGN
            2,           // Length
            0x01,        // Work switch active
            0x00,        // Status flags
            0x00         // CRC
        };
        message[message.Length - 1] = CalculateTestCrc(message);

        // Act
        var result = _parser.ParseMachineData(message);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.WorkSwitchActive);
        Assert.Equal(0x00, result.StatusFlags);
    }

    [Fact]
    public void ParseImuData_ValidMessage_ParsesRollPitchHeading()
    {
        // Arrange - PGN 219 IMU Data: roll = 2.5°, pitch = -1.3°, heading = 90.0°, yawRate = 0.5°/s, calibrated
        // Format: [header, source, pgn, length, rollHi, rollLo, pitchHi, pitchLo, headingHi, headingLo, yawRateHi, yawRateLo, isCalibrated, reserved..., crc]
        // Roll = 2.5 * 100 = 250 = 0x00FA
        // Pitch = -1.3 * 100 = -130 = 0xFF7E (two's complement)
        // Heading = 90.0 * 100 = 9000 = 0x2328
        // YawRate = 0.5 * 100 = 50 = 0x0032
        var message = new byte[]
        {
            0x80, 0x81,  // Header
            0x79,        // Source (IMU module)
            219,         // PGN
            16,          // Length
            0x00, 0xFA,  // Roll (250 = 2.5°)
            0xFF, 0x7E,  // Pitch (-130 = -1.3°)
            0x23, 0x28,  // Heading (9000 = 90.0°)
            0x00, 0x32,  // Yaw rate (50 = 0.5°/s)
            0x01,        // Is calibrated
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Reserved
            0x00         // CRC
        };
        message[message.Length - 1] = CalculateTestCrc(message);

        // Act
        var result = _parser.ParseImuData(message);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2.5, result.Roll, 0.01);
        Assert.Equal(-1.3, result.Pitch, 0.01);
        Assert.Equal(90.0, result.Heading, 0.01);
        Assert.Equal(0.5, result.YawRate, 0.01);
        Assert.True(result.IsCalibrated);
    }

    [Fact]
    public void ParseHelloPacket_AutoSteer_IdentifiesModuleType()
    {
        // Arrange - PGN 126 Hello from AutoSteer, version 2
        var message = new byte[]
        {
            0x80, 0x81,  // Header
            0x7E,        // Source (AutoSteer module)
            126,         // PGN
            3,           // Length
            0x02,        // Version
            0x00, 0x00,  // Reserved
            0x00         // CRC
        };
        message[message.Length - 1] = CalculateTestCrc(message);

        // Act
        var result = _parser.ParseHelloPacket(message);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ModuleType.AutoSteer, result.ModuleType);
        Assert.Equal(2, result.Version);
    }

    [Fact]
    public void ParseHelloPacket_Machine_IdentifiesModuleType()
    {
        // Arrange - PGN 123 Hello from Machine
        var message = new byte[]
        {
            0x80, 0x81,  // Header
            0x7B,        // Source (Machine module)
            123,         // PGN
            3,           // Length
            0x01,        // Version
            0x00, 0x00,  // Reserved
            0x00         // CRC
        };
        message[message.Length - 1] = CalculateTestCrc(message);

        // Act
        var result = _parser.ParseHelloPacket(message);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ModuleType.Machine, result.ModuleType);
        Assert.Equal(1, result.Version);
    }

    [Fact]
    public void ParseHelloPacket_IMU_IdentifiesModuleType()
    {
        // Arrange - PGN 121 Hello from IMU
        var message = new byte[]
        {
            0x80, 0x81,  // Header
            0x79,        // Source (IMU module)
            121,         // PGN
            3,           // Length
            0x01,        // Version
            0x00, 0x00,  // Reserved
            0x00         // CRC
        };
        message[message.Length - 1] = CalculateTestCrc(message);

        // Act
        var result = _parser.ParseHelloPacket(message);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ModuleType.IMU, result.ModuleType);
        Assert.Equal(1, result.Version);
    }

    [Fact]
    public void ValidateCrc_InvalidChecksum_RejectsMessage()
    {
        // Arrange - Valid message structure but wrong CRC
        var message = new byte[]
        {
            0x80, 0x81,  // Header
            0x7E,        // Source
            126,         // PGN
            3,           // Length
            0x02,        // Data
            0x00, 0x00,  // Data
            0xFF         // Invalid CRC
        };

        // Act
        var isValid = _parser.ValidateCrc(message);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ParseMessage_UnknownPgn_ReturnsNull()
    {
        // Arrange - Unknown PGN 199
        var message = new byte[]
        {
            0x80, 0x81,  // Header
            0x7F,        // Source
            199,         // Unknown PGN
            3,           // Length
            0x01, 0x02, 0x03, // Data
            0x00         // CRC
        };
        message[message.Length - 1] = CalculateTestCrc(message);

        // Act
        var result = _parser.ParseMessage(message);

        // Assert - Should return generic PgnMessage but not throw
        Assert.NotNull(result);
        Assert.Equal(199, result.PGN);
    }

    /// <summary>
    /// Helper method to calculate CRC for test messages.
    /// Matches PgnMessage.CalculateCrc() logic.
    /// </summary>
    private byte CalculateTestCrc(byte[] message)
    {
        byte crc = 0;
        for (int i = 2; i < message.Length - 1; i++)
        {
            crc += message[i];
        }
        return crc;
    }
}
