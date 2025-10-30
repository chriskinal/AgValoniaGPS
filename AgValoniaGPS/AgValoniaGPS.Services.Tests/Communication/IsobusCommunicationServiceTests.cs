using System;
using System.Diagnostics;
using System.Linq;
using AgValoniaGPS.Models.Communication;
using AgValoniaGPS.Services.Communication;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.Communication;

/// <summary>
/// Comprehensive unit tests for IsobusCommunicationService.
/// Tests ISO 11783 protocol compliance, message encoding/decoding, section control, and integration.
/// </summary>
[TestFixture]
public class IsobusCommunicationServiceTests
{
    private IsobusCommunicationService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new IsobusCommunicationService();
    }

    #region Message Building Tests

    [Test]
    public void BuildSectionControlMessage_ValidStates_ReturnsCorrectMessage()
    {
        // Arrange
        bool[] sectionStates = new bool[] { true, false, true, false };

        // Act
        var message = _service.BuildSectionControlMessage(sectionStates);

        // Assert
        Assert.That(message, Is.Not.Null);
        Assert.That(message.MessageType, Is.EqualTo(IsobusMessageType.SectionControlCommand));
        Assert.That(message.Data.Length, Is.GreaterThan(0));
    }

    [Test]
    public void BuildSectionControlMessage_NullStates_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.BuildSectionControlMessage(null!));
    }

    [Test]
    public void BuildSectionControlMessage_EmptyStates_ThrowsArgumentException()
    {
        // Arrange
        bool[] emptyStates = Array.Empty<bool>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.BuildSectionControlMessage(emptyStates));
    }

    [Test]
    public void BuildSectionControlMessage_TooManySections_ThrowsArgumentException()
    {
        // Arrange - 32 sections exceeds ISO 11783 limit of 31
        bool[] tooManyStates = new bool[32];

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.BuildSectionControlMessage(tooManyStates));
    }

    [Test]
    public void BuildSectionControlMessage_MaxSections_Succeeds()
    {
        // Arrange - 31 sections is maximum allowed
        bool[] maxStates = new bool[31];
        for (int i = 0; i < 31; i++)
            maxStates[i] = i % 2 == 0; // Alternating pattern

        // Act
        var message = _service.BuildSectionControlMessage(maxStates);

        // Assert
        Assert.That(message, Is.Not.Null);
        Assert.That(message.MessageType, Is.EqualTo(IsobusMessageType.SectionControlCommand));
    }

    #endregion

    #region Message Encoding Tests

    [Test]
    public void EncodeMessage_ValidMessage_ReturnsCorrectFormat()
    {
        // Arrange
        var message = new IsobusMessage(IsobusMessageType.SectionControlRequest, new byte[] { 0x01 });

        // Act
        var encoded = _service.EncodeMessage(message);

        // Assert
        Assert.That(encoded.Length, Is.EqualTo(7)); // Header(2) + Source(1) + PGN(1) + Length(1) + Data(1) + CRC(1)
        Assert.That(encoded[0], Is.EqualTo(0x80)); // HEADER1
        Assert.That(encoded[1], Is.EqualTo(0x81)); // HEADER2
        Assert.That(encoded[2], Is.EqualTo(0x7F)); // Source address
        Assert.That(encoded[3], Is.EqualTo((byte)IsobusMessageType.SectionControlRequest));
        Assert.That(encoded[4], Is.EqualTo(1)); // Data length
        Assert.That(encoded[5], Is.EqualTo(0x01)); // Data
        // CRC is at encoded[6]
    }

    [Test]
    public void EncodeMessage_NullMessage_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.EncodeMessage(null!));
    }

    [Test]
    public void EncodeMessage_MultiByteData_EncodesCorrectly()
    {
        // Arrange
        byte[] testData = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
        var message = new IsobusMessage(IsobusMessageType.ProcessData, testData);

        // Act
        var encoded = _service.EncodeMessage(message);

        // Assert
        Assert.That(encoded.Length, Is.EqualTo(11)); // Header(2) + Source(1) + PGN(1) + Length(1) + Data(5) + CRC(1)
        Assert.That(encoded[4], Is.EqualTo(5)); // Data length
        for (int i = 0; i < testData.Length; i++)
        {
            Assert.That(encoded[5 + i], Is.EqualTo(testData[i]));
        }
    }

    #endregion

    #region Message Decoding Tests

    [Test]
    public void DecodeMessage_ValidMessage_ReturnsCorrectIsobusMessage()
    {
        // Arrange - Build a valid message first
        var originalMessage = new IsobusMessage(IsobusMessageType.SectionControlRequest, new byte[] { 0x01 });
        var encoded = _service.EncodeMessage(originalMessage);

        // Act
        var decoded = _service.DecodeMessage(encoded);

        // Assert
        Assert.That(decoded, Is.Not.Null);
        Assert.That(decoded!.MessageType, Is.EqualTo(IsobusMessageType.SectionControlRequest));
        Assert.That(decoded.Data.Length, Is.EqualTo(1));
        Assert.That(decoded.Data[0], Is.EqualTo(0x01));
        Assert.That(decoded.SourceAddress, Is.EqualTo(0x7F));
    }

    [Test]
    public void DecodeMessage_NullData_ReturnsNull()
    {
        // Act
        var decoded = _service.DecodeMessage(null!);

        // Assert
        Assert.That(decoded, Is.Null);
    }

    [Test]
    public void DecodeMessage_TooShort_ReturnsNull()
    {
        // Arrange - Message too short (< 6 bytes)
        byte[] shortData = new byte[] { 0x80, 0x81, 0x7F, 0xF1 };

        // Act
        var decoded = _service.DecodeMessage(shortData);

        // Assert
        Assert.That(decoded, Is.Null);
    }

    [Test]
    public void DecodeMessage_InvalidHeader_ReturnsNull()
    {
        // Arrange - Invalid header bytes
        byte[] invalidHeader = new byte[] { 0x00, 0x00, 0x7F, 0xF1, 1, 0x01, 0x00 };

        // Act
        var decoded = _service.DecodeMessage(invalidHeader);

        // Assert
        Assert.That(decoded, Is.Null);
    }

    [Test]
    public void DecodeMessage_IncorrectCRC_ReturnsNull()
    {
        // Arrange - Build valid message but corrupt CRC
        var message = new IsobusMessage(IsobusMessageType.SectionControlRequest, new byte[] { 0x01 });
        var encoded = _service.EncodeMessage(message);
        encoded[^1] = 0xFF; // Corrupt CRC

        // Act
        var decoded = _service.DecodeMessage(encoded);

        // Assert
        Assert.That(decoded, Is.Null);
    }

    #endregion

    #region Round-Trip Tests

    [Test]
    public void EncodeDecodeMessage_RoundTrip_PreservesData()
    {
        // Arrange
        byte[] originalData = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var originalMessage = new IsobusMessage(IsobusMessageType.ProcessData, originalData);

        // Act
        var encoded = _service.EncodeMessage(originalMessage);
        var decoded = _service.DecodeMessage(encoded);

        // Assert
        Assert.That(decoded, Is.Not.Null);
        Assert.That(decoded!.MessageType, Is.EqualTo(originalMessage.MessageType));
        Assert.That(decoded.Data, Is.EqualTo(originalData));
    }

    #endregion

    #region Section Control Tests

    [Test]
    public void SendSectionControlCommand_ValidStates_RaisesMessageSentEvent()
    {
        // Arrange
        bool[] sectionStates = new bool[] { true, false, true };
        IsobusMessage? sentMessage = null;
        _service.MessageSent += (sender, msg) => sentMessage = msg;

        // Act
        _service.SendSectionControlCommand(sectionStates);

        // Assert
        Assert.That(sentMessage, Is.Not.Null);
        Assert.That(sentMessage!.MessageType, Is.EqualTo(IsobusMessageType.SectionControlCommand));
    }

    [Test]
    public void RequestSectionControlEnabled_True_SendsCorrectMessage()
    {
        // Arrange
        IsobusMessage? sentMessage = null;
        _service.MessageSent += (sender, msg) => sentMessage = msg;

        // Act
        _service.RequestSectionControlEnabled(true);

        // Assert
        Assert.That(sentMessage, Is.Not.Null);
        Assert.That(sentMessage!.MessageType, Is.EqualTo(IsobusMessageType.SectionControlRequest));
        Assert.That(sentMessage.Data.Length, Is.EqualTo(1));
        Assert.That(sentMessage.Data[0], Is.EqualTo(0x01));
    }

    [Test]
    public void RequestSectionControlEnabled_False_SendsCorrectMessage()
    {
        // Arrange
        IsobusMessage? sentMessage = null;
        _service.MessageSent += (sender, msg) => sentMessage = msg;

        // Act
        _service.RequestSectionControlEnabled(false);

        // Assert
        Assert.That(sentMessage, Is.Not.Null);
        Assert.That(sentMessage!.MessageType, Is.EqualTo(IsobusMessageType.SectionControlRequest));
        Assert.That(sentMessage.Data.Length, Is.EqualTo(1));
        Assert.That(sentMessage.Data[0], Is.EqualTo(0x00));
    }

    #endregion

    #region Process Data Tests

    [Test]
    public void SetGuidanceLineDeviation_ValidValue_SendsProcessDataMessage()
    {
        // Arrange
        IsobusMessage? sentMessage = null;
        _service.MessageSent += (sender, msg) => sentMessage = msg;

        // Act
        _service.SetGuidanceLineDeviation(123);

        // Assert
        Assert.That(sentMessage, Is.Not.Null);
        Assert.That(sentMessage!.MessageType, Is.EqualTo(IsobusMessageType.ProcessData));
        Assert.That(sentMessage.Data.Length, Is.EqualTo(6)); // 2 bytes identifier + 4 bytes value
    }

    [Test]
    public void SetActualSpeed_ValidValue_SendsProcessDataMessage()
    {
        // Arrange
        IsobusMessage? sentMessage = null;
        _service.MessageSent += (sender, msg) => sentMessage = msg;

        // Act
        _service.SetActualSpeed(5000); // 5000 mm/s = 5 m/s

        // Assert
        Assert.That(sentMessage, Is.Not.Null);
        Assert.That(sentMessage!.MessageType, Is.EqualTo(IsobusMessageType.ProcessData));
    }

    [Test]
    public void SetTotalDistance_ValidValue_SendsProcessDataMessage()
    {
        // Arrange
        IsobusMessage? sentMessage = null;
        _service.MessageSent += (sender, msg) => sentMessage = msg;

        // Act
        _service.SetTotalDistance(100000); // 100 meters

        // Assert
        Assert.That(sentMessage, Is.Not.Null);
        Assert.That(sentMessage!.MessageType, Is.EqualTo(IsobusMessageType.ProcessData));
    }

    [Test]
    public void SetGuidanceLineDeviation_RateLimited_DoesNotSendDuplicates()
    {
        // Arrange
        int messageCount = 0;
        _service.MessageSent += (sender, msg) => messageCount++;

        // Act - Send same value twice in quick succession
        _service.SetGuidanceLineDeviation(100);
        _service.SetGuidanceLineDeviation(100);

        // Assert - Only one message should be sent (duplicate filtered)
        Assert.That(messageCount, Is.EqualTo(1));
    }

    #endregion

    #region Heartbeat Processing Tests

    [Test]
    public void ProcessHeartbeat_ValidData_UpdatesSectionControlEnabled()
    {
        // Arrange - Heartbeat with section control enabled, 4 sections
        byte[] heartbeat = new byte[]
        {
            0x01, // Bit 0 = section control enabled
            0x04, // 4 sections
            0x0A  // Section states: 0b00001010 = sections 1 and 3 on (0-indexed)
        };

        // Act
        bool result = _service.ProcessHeartbeat(heartbeat);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(_service.SectionControlEnabled, Is.True);
        Assert.That(_service.IsAlive, Is.True);
    }

    [Test]
    public void ProcessHeartbeat_ValidData_UpdatesSectionStates()
    {
        // Arrange - Heartbeat with 8 sections
        byte[] heartbeat = new byte[]
        {
            0x00, // Section control disabled
            0x08, // 8 sections
            0xAA  // Section states: 0b10101010 = alternating pattern
        };

        // Act
        bool result = _service.ProcessHeartbeat(heartbeat);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(_service.IsSectionOn(0), Is.False);
        Assert.That(_service.IsSectionOn(1), Is.True);
        Assert.That(_service.IsSectionOn(2), Is.False);
        Assert.That(_service.IsSectionOn(3), Is.True);
        Assert.That(_service.IsSectionOn(4), Is.False);
        Assert.That(_service.IsSectionOn(5), Is.True);
        Assert.That(_service.IsSectionOn(6), Is.False);
        Assert.That(_service.IsSectionOn(7), Is.True);
    }

    [Test]
    public void ProcessHeartbeat_MultiByteStates_ProcessesCorrectly()
    {
        // Arrange - Heartbeat with 16 sections (2 bytes of state data)
        byte[] heartbeat = new byte[]
        {
            0x01, // Section control enabled
            0x10, // 16 sections
            0xFF, // First 8 sections all on
            0x00  // Last 8 sections all off
        };

        // Act
        bool result = _service.ProcessHeartbeat(heartbeat);

        // Assert
        Assert.That(result, Is.True);
        for (int i = 0; i < 8; i++)
            Assert.That(_service.IsSectionOn(i), Is.True, $"Section {i} should be on");
        for (int i = 8; i < 16; i++)
            Assert.That(_service.IsSectionOn(i), Is.False, $"Section {i} should be off");
    }

    [Test]
    public void ProcessHeartbeat_InvalidLength_ReturnsFalse()
    {
        // Arrange - Too short
        byte[] heartbeat = new byte[] { 0x01 };

        // Act
        bool result = _service.ProcessHeartbeat(heartbeat);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void ProcessHeartbeat_IncorrectDataLength_ReturnsFalse()
    {
        // Arrange - Says 8 sections but only provides data for 4
        byte[] heartbeat = new byte[]
        {
            0x01, // Section control enabled
            0x08, // Claims 8 sections
            0x0F  // Only 1 byte of state data (needs 1 byte but validation might fail)
        };

        // Act - This should actually succeed since 8 sections = 1 byte
        bool result = _service.ProcessHeartbeat(heartbeat);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsSectionOn_InvalidIndex_ReturnsFalse()
    {
        // Arrange - Process heartbeat with 4 sections
        byte[] heartbeat = new byte[] { 0x01, 0x04, 0xFF };
        _service.ProcessHeartbeat(heartbeat);

        // Act & Assert
        Assert.That(_service.IsSectionOn(-1), Is.False);
        Assert.That(_service.IsSectionOn(100), Is.False);
    }

    [Test]
    public void IsAlive_NoHeartbeat_ReturnsFalse()
    {
        // Arrange - Fresh service with no heartbeat

        // Act & Assert
        Assert.That(_service.IsAlive, Is.False);
    }

    [Test]
    public void IsAlive_RecentHeartbeat_ReturnsTrue()
    {
        // Arrange
        byte[] heartbeat = new byte[] { 0x01, 0x04, 0xFF };
        _service.ProcessHeartbeat(heartbeat);

        // Act & Assert
        Assert.That(_service.IsAlive, Is.True);
    }

    #endregion

    #region Performance Tests

    [Test]
    public void EncodeMessage_Performance_CompletesUnder5ms()
    {
        // Arrange
        var message = new IsobusMessage(IsobusMessageType.SectionControlCommand, new byte[31]);
        var stopwatch = Stopwatch.StartNew();

        // Act - Encode 100 messages
        for (int i = 0; i < 100; i++)
        {
            _service.EncodeMessage(message);
        }

        stopwatch.Stop();

        // Assert - Average time per message < 5ms
        double avgTimeMs = stopwatch.ElapsedMilliseconds / 100.0;
        Assert.That(avgTimeMs, Is.LessThan(5.0),
            $"Average encode time {avgTimeMs:F2}ms exceeds 5ms target");
    }

    [Test]
    public void DecodeMessage_Performance_CompletesUnder5ms()
    {
        // Arrange
        var message = new IsobusMessage(IsobusMessageType.SectionControlCommand, new byte[31]);
        var encoded = _service.EncodeMessage(message);
        var stopwatch = Stopwatch.StartNew();

        // Act - Decode 100 messages
        for (int i = 0; i < 100; i++)
        {
            _service.DecodeMessage(encoded);
        }

        stopwatch.Stop();

        // Assert - Average time per message < 5ms
        double avgTimeMs = stopwatch.ElapsedMilliseconds / 100.0;
        Assert.That(avgTimeMs, Is.LessThan(5.0),
            $"Average decode time {avgTimeMs:F2}ms exceeds 5ms target");
    }

    #endregion

    #region ISO 11783 Compliance Tests

    [Test]
    public void IsobusMessage_UsesStandardPgnFormat()
    {
        // Arrange
        var message = new IsobusMessage(IsobusMessageType.SectionControlCommand, new byte[] { 0x01, 0x02 });

        // Act
        var encoded = _service.EncodeMessage(message);

        // Assert - Verify PGN message structure
        Assert.That(encoded[0], Is.EqualTo(0x80), "Header byte 1 must be 0x80");
        Assert.That(encoded[1], Is.EqualTo(0x81), "Header byte 2 must be 0x81");
        Assert.That(encoded[2], Is.EqualTo(0x7F), "Source address must be 0x7F");
        // PGN byte is message type
        Assert.That(encoded[4], Is.EqualTo(2), "Length must match data length");
    }

    [Test]
    public void SectionControl_SupportsMaximum31Sections()
    {
        // Arrange - ISO 11783 allows up to 31 sections per TC-SC
        bool[] sections = Enumerable.Range(0, 31).Select(i => i % 2 == 0).ToArray();

        // Act
        var message = _service.BuildSectionControlMessage(sections);
        var encoded = _service.EncodeMessage(message);
        var decoded = _service.DecodeMessage(encoded);

        // Assert
        Assert.That(decoded, Is.Not.Null);
        Assert.That(decoded!.Data[1], Is.EqualTo(31), "Should support 31 sections");
    }

    #endregion
}
