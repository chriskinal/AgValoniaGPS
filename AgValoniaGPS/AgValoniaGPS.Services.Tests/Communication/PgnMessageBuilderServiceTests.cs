using AgValoniaGPS.Models;
using AgValoniaGPS.Services.Communication;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace AgValoniaGPS.Services.Tests.Communication;

/// <summary>
/// Tests for PgnMessageBuilderService covering all message types and validation.
/// Verifies byte-perfect PGN message construction with correct CRC calculation.
/// </summary>
[TestFixture]
public class PgnMessageBuilderServiceTests
{
    private IPgnMessageBuilderService _builder = null!;

    [SetUp]
    public void Setup()
    {
        _builder = new PgnMessageBuilderService();
    }

    /// <summary>
    /// Test 1: AutoSteer Data (PGN 254) message format with CRC validation
    /// </summary>
    [Test]
    public void BuildAutoSteerData_ValidInputs_ProducesCorrectMessageFormat()
    {
        // Arrange
        double speedMph = 10.5; // Speed in MPH
        byte status = 1; // On
        double steerAngle = 5.25; // Degrees
        int crossTrackErrorMm = 150; // mm

        // Act
        byte[] message = _builder.BuildAutoSteerData(speedMph, status, steerAngle, crossTrackErrorMm);

        // Assert
        Assert.That(message, Is.Not.Null);
        Assert.That(message.Length, Is.EqualTo(15)); // Header(2) + Source(1) + PGN(1) + Length(1) + Data(9) + CRC(1)
        Assert.That(message[0], Is.EqualTo(0x80)); // Header1
        Assert.That(message[1], Is.EqualTo(0x81)); // Header2
        Assert.That(message[2], Is.EqualTo(0x7F)); // Source (from AgIO)
        Assert.That(message[3], Is.EqualTo(254)); // PGN 254
        Assert.That(message[4], Is.EqualTo(9)); // Data length

        // Speed: 10.5 * 10 = 105 (0x0069)
        ushort speedValue = (ushort)((message[5] << 8) | message[6]);
        Assert.That(speedValue, Is.EqualTo(105));

        // Status
        Assert.That(message[7], Is.EqualTo(1));

        // Steer angle: 5.25 * 100 = 525 (0x020D)
        short angleValue = (short)((message[8] << 8) | message[9]);
        Assert.That(angleValue, Is.EqualTo(525));

        // XTE: 150 mm
        short xteValue = (short)((message[10] << 8) | message[11]);
        Assert.That(xteValue, Is.EqualTo(150));

        // Verify CRC (sum of bytes 2 through 13)
        byte expectedCrc = 0;
        for (int i = 2; i < 14; i++)
        {
            expectedCrc += message[i];
        }
        Assert.That(message[14], Is.EqualTo(expectedCrc));
    }

    /// <summary>
    /// Test 2: AutoSteer Settings (PGN 252) message format
    /// </summary>
    [Test]
    public void BuildAutoSteerSettings_ValidInputs_ProducesCorrectMessageFormat()
    {
        // Arrange
        byte pwmDrive = 180;
        byte minPwm = 20;
        float proportionalGain = 1.5f;
        byte highPwm = 255;
        float lowSpeedPwm = 0.8f;

        // Act
        byte[] message = _builder.BuildAutoSteerSettings(pwmDrive, minPwm, proportionalGain, highPwm, lowSpeedPwm);

        // Assert
        Assert.That(message, Is.Not.Null);
        Assert.That(message.Length, Is.EqualTo(18)); // Header(2) + Source(1) + PGN(1) + Length(1) + Data(12) + CRC(1)
        Assert.That(message[0], Is.EqualTo(0x80));
        Assert.That(message[1], Is.EqualTo(0x81));
        Assert.That(message[2], Is.EqualTo(0x7F));
        Assert.That(message[3], Is.EqualTo(252)); // PGN 252
        Assert.That(message[4], Is.EqualTo(12)); // Data length

        Assert.That(message[5], Is.EqualTo(pwmDrive));
        Assert.That(message[6], Is.EqualTo(minPwm));

        // Proportional gain: 1.5 * 100 = 150
        ushort pgainValue = (ushort)((message[7] << 8) | message[8]);
        Assert.That(pgainValue, Is.EqualTo(150));

        Assert.That(message[9], Is.EqualTo(highPwm));

        // Low speed PWM: 0.8 * 100 = 80
        ushort lowSpeedValue = (ushort)((message[10] << 8) | message[11]);
        Assert.That(lowSpeedValue, Is.EqualTo(80));

        // Verify CRC
        byte expectedCrc = 0;
        for (int i = 2; i < 17; i++)
        {
            expectedCrc += message[i];
        }
        Assert.That(message[17], Is.EqualTo(expectedCrc));
    }

    /// <summary>
    /// Test 3: Machine Data (PGN 239) message format with variable section count
    /// </summary>
    [Test]
    public void BuildMachineData_VariableSections_ProducesCorrectMessageFormat()
    {
        // Arrange
        byte[] relayLo = new byte[8] { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
        byte[] relayHi = new byte[8] { 0xFF, 0xFE, 0xFD, 0xFC, 0xFB, 0xFA, 0xF9, 0xF8 };
        byte tramLine = 0x03;
        byte[] sectionState = new byte[5] { 1, 0, 1, 2, 1 }; // 5 sections

        // Act
        byte[] message = _builder.BuildMachineData(relayLo, relayHi, tramLine, sectionState);

        // Assert
        Assert.That(message, Is.Not.Null);
        Assert.That(message.Length, Is.EqualTo(28)); // Header(2) + Source(1) + PGN(1) + Length(1) + Data(22) + CRC(1)
        Assert.That(message[0], Is.EqualTo(0x80));
        Assert.That(message[1], Is.EqualTo(0x81));
        Assert.That(message[2], Is.EqualTo(0x7F));
        Assert.That(message[3], Is.EqualTo(239)); // PGN 239
        Assert.That(message[4], Is.EqualTo(22)); // Data length: 8 (relayLo) + 8 (relayHi) + 1 (tramLine) + 5 (sectionState)

        // Verify relay states
        for (int i = 0; i < 8; i++)
        {
            Assert.That(message[5 + i], Is.EqualTo(relayLo[i]));
            Assert.That(message[13 + i], Is.EqualTo(relayHi[i]));
        }

        // Verify tram line
        Assert.That(message[21], Is.EqualTo(tramLine));

        // Verify section states
        for (int i = 0; i < sectionState.Length; i++)
        {
            Assert.That(message[22 + i], Is.EqualTo(sectionState[i]));
        }

        // Verify CRC
        byte expectedCrc = 0;
        for (int i = 2; i < 27; i++)
        {
            expectedCrc += message[i];
        }
        Assert.That(message[27], Is.EqualTo(expectedCrc));
    }

    /// <summary>
    /// Test 4: Machine Config (PGN 238) message format
    /// </summary>
    [Test]
    public void BuildMachineConfig_ValidInputs_ProducesCorrectMessageFormat()
    {
        // Arrange
        ushort sections = 16;
        ushort zones = 4;
        ushort totalWidth = 1200; // cm (12 meters)

        // Act
        byte[] message = _builder.BuildMachineConfig(sections, zones, totalWidth);

        // Assert
        Assert.That(message, Is.Not.Null);
        Assert.That(message.Length, Is.EqualTo(12)); // Header(2) + Source(1) + PGN(1) + Length(1) + Data(6) + CRC(1)
        Assert.That(message[0], Is.EqualTo(0x80));
        Assert.That(message[1], Is.EqualTo(0x81));
        Assert.That(message[2], Is.EqualTo(0x7F));
        Assert.That(message[3], Is.EqualTo(238)); // PGN 238
        Assert.That(message[4], Is.EqualTo(6)); // Data length

        // Sections (uint16, big-endian)
        ushort sectionsValue = (ushort)((message[5] << 8) | message[6]);
        Assert.That(sectionsValue, Is.EqualTo(sections));

        // Zones (uint16, big-endian)
        ushort zonesValue = (ushort)((message[7] << 8) | message[8]);
        Assert.That(zonesValue, Is.EqualTo(zones));

        // Total width (uint16, big-endian)
        ushort widthValue = (ushort)((message[9] << 8) | message[10]);
        Assert.That(widthValue, Is.EqualTo(totalWidth));

        // Verify CRC
        byte expectedCrc = 0;
        for (int i = 2; i < 11; i++)
        {
            expectedCrc += message[i];
        }
        Assert.That(message[11], Is.EqualTo(expectedCrc));
    }

    /// <summary>
    /// Test 5: IMU Config (PGN 218) message format
    /// </summary>
    [Test]
    public void BuildImuConfig_ValidInput_ProducesCorrectMessageFormat()
    {
        // Arrange
        byte configFlags = 0x07; // Enable all features

        // Act
        byte[] message = _builder.BuildImuConfig(configFlags);

        // Assert
        Assert.That(message, Is.Not.Null);
        Assert.That(message.Length, Is.EqualTo(10)); // Header(2) + Source(1) + PGN(1) + Length(1) + Data(4) + CRC(1)
        Assert.That(message[0], Is.EqualTo(0x80));
        Assert.That(message[1], Is.EqualTo(0x81));
        Assert.That(message[2], Is.EqualTo(0x7F));
        Assert.That(message[3], Is.EqualTo(218)); // PGN 218
        Assert.That(message[4], Is.EqualTo(4)); // Data length

        Assert.That(message[5], Is.EqualTo(configFlags));
        Assert.That(message[6], Is.EqualTo(0)); // Reserved
        Assert.That(message[7], Is.EqualTo(0)); // Reserved
        Assert.That(message[8], Is.EqualTo(0)); // Reserved

        // Verify CRC
        byte expectedCrc = 0;
        for (int i = 2; i < 9; i++)
        {
            expectedCrc += message[i];
        }
        Assert.That(message[9], Is.EqualTo(expectedCrc));
    }

    /// <summary>
    /// Test 6: Hello Packet (PGN 200) format for backward compatibility
    /// </summary>
    [Test]
    public void BuildHelloPacket_ProducesBackwardCompatibleFormat()
    {
        // Act
        byte[] message = _builder.BuildHelloPacket();

        // Assert - Must match existing format: [0x80, 0x81, 0x7F, 200, 3, 56, 0, 0, CRC]
        Assert.That(message, Is.Not.Null);
        Assert.That(message.Length, Is.EqualTo(9));
        Assert.That(message[0], Is.EqualTo(0x80));
        Assert.That(message[1], Is.EqualTo(0x81));
        Assert.That(message[2], Is.EqualTo(0x7F));
        Assert.That(message[3], Is.EqualTo(200)); // PGN 200
        Assert.That(message[4], Is.EqualTo(3)); // Length
        Assert.That(message[5], Is.EqualTo(56)); // Specific value from existing implementation
        Assert.That(message[6], Is.EqualTo(0));
        Assert.That(message[7], Is.EqualTo(0));

        // Verify CRC
        byte expectedCrc = 0;
        for (int i = 2; i < 8; i++)
        {
            expectedCrc += message[i];
        }
        Assert.That(message[8], Is.EqualTo(expectedCrc));
    }

    /// <summary>
    /// Test 7: Scan Request message format
    /// </summary>
    [Test]
    public void BuildScanRequest_ProducesCorrectMessageFormat()
    {
        // Act
        byte[] message = _builder.BuildScanRequest();

        // Assert
        Assert.That(message, Is.Not.Null);
        Assert.That(message.Length, Is.EqualTo(8)); // Header(2) + Source(1) + PGN(1) + Length(1) + Data(2) + CRC(1)
        Assert.That(message[0], Is.EqualTo(0x80));
        Assert.That(message[1], Is.EqualTo(0x81));
        Assert.That(message[2], Is.EqualTo(0x7F));
        Assert.That(message[3], Is.EqualTo(202)); // PGN 202 (scan request)
        Assert.That(message[4], Is.EqualTo(2)); // Data length

        // Verify CRC
        byte expectedCrc = 0;
        for (int i = 2; i < 7; i++)
        {
            expectedCrc += message[i];
        }
        Assert.That(message[7], Is.EqualTo(expectedCrc));
    }

    /// <summary>
    /// Test 8: CRC calculation matches existing PgnMessage.CalculateCrc()
    /// </summary>
    [Test]
    public void BuildAutoSteerData_CrcCalculation_MatchesExistingImplementation()
    {
        // Arrange
        double speedMph = 15.0;
        byte status = 1;
        double steerAngle = -3.5;
        int crossTrackErrorMm = -200;

        // Act
        byte[] message = _builder.BuildAutoSteerData(speedMph, status, steerAngle, crossTrackErrorMm);

        // Assert - Manually calculate CRC using same algorithm as PgnMessage
        byte manualCrc = 0;
        for (int i = 2; i < message.Length - 1; i++)
        {
            manualCrc += message[i];
        }
        Assert.That(message[^1], Is.EqualTo(manualCrc), "CRC should match manual calculation");
    }

    /// <summary>
    /// Test for input validation - negative speed should throw
    /// </summary>
    [Test]
    public void BuildAutoSteerData_NegativeSpeed_ThrowsArgumentException()
    {
        // Arrange
        double speedMph = -5.0;
        byte status = 1;
        double steerAngle = 0.0;
        int crossTrackErrorMm = 0;

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _builder.BuildAutoSteerData(speedMph, status, steerAngle, crossTrackErrorMm));
    }

    /// <summary>
    /// Test for input validation - invalid relay array length should throw
    /// </summary>
    [Test]
    public void BuildMachineData_InvalidRelayLength_ThrowsArgumentException()
    {
        // Arrange
        byte[] relayLo = new byte[5]; // Should be 8
        byte[] relayHi = new byte[8];
        byte tramLine = 0;
        byte[] sectionState = new byte[4];

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _builder.BuildMachineData(relayLo, relayHi, tramLine, sectionState));
    }
}
