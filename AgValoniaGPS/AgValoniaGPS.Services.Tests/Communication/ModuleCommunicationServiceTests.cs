using System;
using AgValoniaGPS.Models.Communication;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Services.Communication;
using Moq;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.Communication;

/// <summary>
/// Tests for module communication services (AutoSteer, Machine, IMU).
/// Validates PGN message building, feedback parsing, event publishing, and thread safety.
/// </summary>
[TestFixture]
public class ModuleCommunicationServiceTests
{
    private Mock<IModuleCoordinatorService> _mockCoordinator = null!;
    private Mock<IPgnMessageBuilderService> _mockBuilder = null!;
    private Mock<IPgnMessageParserService> _mockParser = null!;
    private Mock<ITransportAbstractionService> _mockTransport = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCoordinator = new Mock<IModuleCoordinatorService>();
        _mockBuilder = new Mock<IPgnMessageBuilderService>();
        _mockParser = new Mock<IPgnMessageParserService>();
        _mockTransport = new Mock<ITransportAbstractionService>();

        // Default: Module is ready
        _mockCoordinator.Setup(x => x.IsModuleReady(It.IsAny<ModuleType>())).Returns(true);
    }

    #region AutoSteer Tests

    [Test]
    public void AutoSteerService_SendSteeringCommand_BuildsPgn254Correctly()
    {
        // Arrange
        var service = new AutoSteerCommunicationService(
            _mockCoordinator.Object,
            _mockBuilder.Object,
            _mockParser.Object,
            _mockTransport.Object);

        // Expected PGN 254 message with placeholder data
        byte[] expectedMessage = new byte[] { 0x80, 0x81, 0x7F, 254, 10, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF };
        _mockBuilder.Setup(x => x.BuildAutoSteerData(10.0, 1, 5.0, 100))
            .Returns(expectedMessage);

        // Act
        service.SendSteeringCommand(10.0, 5.0, 100, true);

        // Assert
        _mockBuilder.Verify(x => x.BuildAutoSteerData(10.0, 1, 5.0, 100), Times.Once);
        _mockTransport.Verify(x => x.SendMessage(ModuleType.AutoSteer, expectedMessage), Times.Once);
    }

    [Test]
    public void AutoSteerService_SendSettings_BuildsPgn252Correctly()
    {
        // Arrange
        var service = new AutoSteerCommunicationService(
            _mockCoordinator.Object,
            _mockBuilder.Object,
            _mockParser.Object,
            _mockTransport.Object);

        // Expected PGN 252 message with placeholder data
        byte[] expectedMessage = new byte[] { 0x80, 0x81, 0x7F, 252, 12, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF };
        _mockBuilder.Setup(x => x.BuildAutoSteerSettings(200, 50, 1.5f, 250, 0.8f))
            .Returns(expectedMessage);

        // Act
        service.SendSettings(200, 50, 1.5f, 250, 0.8f);

        // Assert
        _mockBuilder.Verify(x => x.BuildAutoSteerSettings(200, 50, 1.5f, 250, 0.8f), Times.Once);
        _mockTransport.Verify(x => x.SendMessage(ModuleType.AutoSteer, expectedMessage), Times.Once);
    }

    [Test]
    public void AutoSteerService_FeedbackReceived_ParsesAndPublishesEvent()
    {
        // Arrange
        var service = new AutoSteerCommunicationService(
            _mockCoordinator.Object,
            _mockBuilder.Object,
            _mockParser.Object,
            _mockTransport.Object);

        var feedback = new AutoSteerFeedback
        {
            ActualWheelAngle = 5.2,
            SwitchStates = new byte[] { 0x01 },
            StatusFlags = 0x00,
            Timestamp = DateTime.UtcNow
        };

        // PGN 253 feedback data
        byte[] feedbackData = new byte[] { 0x80, 0x81, 0x7E, 253, 8, 0x00, 0x00, 0x00, 0xFF };
        _mockParser.Setup(x => x.ParseAutoSteerData(feedbackData)).Returns(feedback);

        AutoSteerFeedbackEventArgs? receivedEvent = null;
        service.FeedbackReceived += (sender, e) => receivedEvent = e;

        // Act
        _mockTransport.Raise(x => x.DataReceived += null,
            new TransportDataReceivedEventArgs(ModuleType.AutoSteer, feedbackData));

        // Assert
        Assert.That(receivedEvent, Is.Not.Null);
        Assert.That(receivedEvent!.Feedback.ActualWheelAngle, Is.EqualTo(5.2));
        Assert.That(service.ActualWheelAngle, Is.EqualTo(5.2));
    }

    #endregion

    #region Machine Tests

    [Test]
    public void MachineService_SendSectionStates_BuildsPgn239Correctly()
    {
        // Arrange
        var service = new MachineCommunicationService(
            _mockCoordinator.Object,
            _mockBuilder.Object,
            _mockParser.Object,
            _mockTransport.Object);

        byte[] sectionStates = new byte[] { 1, 1, 0, 1 }; // on, on, off, on
        // Expected PGN 239 message with placeholder data
        byte[] expectedMessage = new byte[] { 0x80, 0x81, 0x7F, 239, 20, 0x00, 0x00, 0x00, 0x00, 0xFF };

        _mockBuilder.Setup(x => x.BuildMachineData(
            It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<byte>(), sectionStates))
            .Returns(expectedMessage);

        // Act
        service.SendSectionStates(sectionStates);

        // Assert
        _mockTransport.Verify(x => x.SendMessage(ModuleType.Machine, expectedMessage), Times.Once);
    }

    [Test]
    public void MachineService_WorkSwitchChanged_ParsesAndPublishesEvent()
    {
        // Arrange
        var service = new MachineCommunicationService(
            _mockCoordinator.Object,
            _mockBuilder.Object,
            _mockParser.Object,
            _mockTransport.Object);

        var feedback = new MachineFeedback
        {
            WorkSwitchActive = true,
            SectionSensors = new byte[] { 0x01, 0x01, 0x00 },
            StatusFlags = 0x00,
            Timestamp = DateTime.UtcNow
        };

        // PGN 234 work switch data
        byte[] feedbackData = new byte[] { 0x80, 0x81, 0x7B, 234, 2, 0x01, 0x00, 0xFF };
        _mockParser.Setup(x => x.ParseMachineData(feedbackData)).Returns(feedback);

        WorkSwitchChangedEventArgs? receivedEvent = null;
        service.WorkSwitchChanged += (sender, e) => receivedEvent = e;

        // Act
        _mockTransport.Raise(x => x.DataReceived += null,
            new TransportDataReceivedEventArgs(ModuleType.Machine, feedbackData));

        // Assert
        Assert.That(receivedEvent, Is.Not.Null);
        Assert.That(receivedEvent!.IsActive, Is.True);
        Assert.That(service.WorkSwitchActive, Is.True);
    }

    #endregion

    #region IMU Tests

    [Test]
    public void ImuService_SendConfiguration_BuildsPgn218Correctly()
    {
        // Arrange
        var service = new ImuCommunicationService(
            _mockCoordinator.Object,
            _mockBuilder.Object,
            _mockParser.Object,
            _mockTransport.Object);

        // Expected PGN 218 configuration message
        byte[] expectedMessage = new byte[] { 0x80, 0x81, 0x7F, 218, 4, 0x07, 0x00, 0x00, 0xFF };
        _mockBuilder.Setup(x => x.BuildImuConfig(0x07)).Returns(expectedMessage);

        // Act
        service.SendConfiguration(0x07);

        // Assert
        _mockBuilder.Verify(x => x.BuildImuConfig(0x07), Times.Once);
        _mockTransport.Verify(x => x.SendMessage(ModuleType.IMU, expectedMessage), Times.Once);
    }

    [Test]
    public void ImuService_DataReceived_ParsesAndPublishesEvent()
    {
        // Arrange
        var service = new ImuCommunicationService(
            _mockCoordinator.Object,
            _mockBuilder.Object,
            _mockParser.Object,
            _mockTransport.Object);

        var imuData = new ImuData
        {
            Roll = 2.5,
            Pitch = -1.2,
            Heading = 90.0,
            YawRate = 0.5,
            IsCalibrated = true,
            Timestamp = DateTime.UtcNow
        };

        // PGN 219 IMU data message
        byte[] imuDataBytes = new byte[] { 0x80, 0x81, 0x79, 219, 16, 0x00, 0x00, 0x00, 0x00, 0xFF };
        _mockParser.Setup(x => x.ParseImuData(imuDataBytes)).Returns(imuData);

        ImuDataReceivedEventArgs? receivedEvent = null;
        service.DataReceived += (sender, e) => receivedEvent = e;

        // Act
        _mockTransport.Raise(x => x.DataReceived += null,
            new TransportDataReceivedEventArgs(ModuleType.IMU, imuDataBytes));

        // Assert
        Assert.That(receivedEvent, Is.Not.Null);
        Assert.That(receivedEvent!.Data.Roll, Is.EqualTo(2.5));
        Assert.That(receivedEvent.Data.Pitch, Is.EqualTo(-1.2));
        Assert.That(receivedEvent.Data.Heading, Is.EqualTo(90.0));
        Assert.That(service.Roll, Is.EqualTo(2.5));
        Assert.That(service.Pitch, Is.EqualTo(-1.2));
        Assert.That(service.Heading, Is.EqualTo(90.0));
        Assert.That(service.IsCalibrated, Is.True);
    }

    #endregion

    #region Ready State Tests

    [Test]
    public void AutoSteerService_ModuleNotReady_DoesNotSendCommand()
    {
        // Arrange
        _mockCoordinator.Setup(x => x.IsModuleReady(ModuleType.AutoSteer)).Returns(false);

        var service = new AutoSteerCommunicationService(
            _mockCoordinator.Object,
            _mockBuilder.Object,
            _mockParser.Object,
            _mockTransport.Object);

        // Act
        service.SendSteeringCommand(10.0, 5.0, 100, true);

        // Assert
        _mockBuilder.Verify(x => x.BuildAutoSteerData(It.IsAny<double>(), It.IsAny<byte>(),
            It.IsAny<double>(), It.IsAny<int>()), Times.Never);
        _mockTransport.Verify(x => x.SendMessage(It.IsAny<ModuleType>(), It.IsAny<byte[]>()), Times.Never);
    }

    #endregion
}
