using AgValoniaGPS.Models.Communication;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Services.Communication;
using Moq;
using Xunit;

namespace AgValoniaGPS.Services.Tests.Communication;

/// <summary>
/// Unit tests for ModuleCoordinatorService.
/// Tests module state management, hello packet monitoring, data flow tracking,
/// timeout detection, and ready state management.
/// </summary>
public class ModuleCoordinatorServiceTests
{
    private readonly Mock<ITransportAbstractionService> _mockTransport;
    private readonly Mock<IPgnMessageParserService> _mockParser;
    private readonly Mock<IPgnMessageBuilderService> _mockBuilder;
    private readonly ModuleCoordinatorService _coordinator;

    public ModuleCoordinatorServiceTests()
    {
        _mockTransport = new Mock<ITransportAbstractionService>();
        _mockParser = new Mock<IPgnMessageParserService>();
        _mockBuilder = new Mock<IPgnMessageBuilderService>();

        _coordinator = new ModuleCoordinatorService(
            _mockTransport.Object,
            _mockParser.Object,
            _mockBuilder.Object);
    }

    [Fact]
    public void GetModuleState_InitialState_ReturnsDisconnected()
    {
        // Act
        var state = _coordinator.GetModuleState(ModuleType.AutoSteer);

        // Assert
        Assert.Equal(ModuleState.Disconnected, state);
    }

    [Fact]
    public void HelloPacketReceived_V1Module_TransitionsToReady()
    {
        // Arrange
        var helloData = new byte[] { 0x80, 0x81, 0x7E, 126, 3, 0, 0, 0, 0 };
        var helloResponse = new HelloResponse
        {
            ModuleType = ModuleType.AutoSteer,
            Version = 1, // V1 module - transitions directly to Ready
            ReceivedAt = DateTime.UtcNow
        };

        _mockParser.Setup(p => p.ParseHelloPacket(It.IsAny<byte[]>()))
            .Returns(helloResponse);

        // Act - Simulate transport receiving hello packet by raising event
        _mockTransport.Raise(
            t => t.DataReceived += null,
            this,
            new TransportDataReceivedEventArgs(ModuleType.AutoSteer, helloData));

        // Assert - V1 module should transition directly to Ready
        var state = _coordinator.GetModuleState(ModuleType.AutoSteer);
        Assert.Equal(ModuleState.Ready, state);
    }

    [Fact]
    public void HelloPacketReceived_UpdatesLastHelloTime()
    {
        // Arrange - Receive hello packet first
        var helloData = new byte[] { 0x80, 0x81, 0x7E, 126, 3, 0, 0, 0, 0 };
        var helloResponse = new HelloResponse
        {
            ModuleType = ModuleType.AutoSteer,
            Version = 1,
            ReceivedAt = DateTime.UtcNow
        };

        _mockParser.Setup(p => p.ParseHelloPacket(It.IsAny<byte[]>()))
            .Returns(helloResponse);

        _mockTransport.Raise(
            t => t.DataReceived += null,
            this,
            new TransportDataReceivedEventArgs(ModuleType.AutoSteer, helloData));

        // Act - Get last hello time
        var lastHello = _coordinator.GetLastHelloTime(ModuleType.AutoSteer);

        // Assert - Last hello time should be recent (within 1 second)
        Assert.True((DateTime.UtcNow - lastHello).TotalSeconds < 1);
    }

    [Fact]
    public void DataFlowMonitoring_AutoSteerDataReceived_UpdatesLastDataTime()
    {
        // Arrange - Non-hello data packet
        var dataMessage = new byte[] { 0x80, 0x81, 0x7E, 253, 8, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        // Parser returns null for non-hello packets
        _mockParser.Setup(p => p.ParseHelloPacket(It.IsAny<byte[]>()))
            .Returns((HelloResponse?)null);

        // Act - Simulate transport receiving data packet (non-hello)
        _mockTransport.Raise(
            t => t.DataReceived += null,
            this,
            new TransportDataReceivedEventArgs(ModuleType.AutoSteer, dataMessage));

        // Assert
        var lastData = _coordinator.GetLastDataTime(ModuleType.AutoSteer);
        Assert.True((DateTime.UtcNow - lastData).TotalMilliseconds < 200);
    }

    [Fact]
    public void ModuleReadyEvent_PublishedAfterHelloForV1Module()
    {
        // Arrange
        ModuleReadyEventArgs? readyEventArgs = null;
        _coordinator.ModuleReady += (sender, e) => readyEventArgs = e;

        var helloData = new byte[] { 0x80, 0x81, 0x7E, 126, 3, 0, 0, 0, 0 };
        var helloResponse = new HelloResponse
        {
            ModuleType = ModuleType.AutoSteer,
            Version = 1, // V1 module
            ReceivedAt = DateTime.UtcNow
        };

        _mockParser.Setup(p => p.ParseHelloPacket(It.IsAny<byte[]>()))
            .Returns(helloResponse);

        // Act - Simulate hello packet received
        _mockTransport.Raise(
            t => t.DataReceived += null,
            this,
            new TransportDataReceivedEventArgs(ModuleType.AutoSteer, helloData));

        // Assert - Module should transition to Ready for V1 modules
        Assert.NotNull(readyEventArgs);
        Assert.Equal(ModuleType.AutoSteer, readyEventArgs.ModuleType);
    }

    [Fact]
    public void IsModuleReady_ReadyState_ReturnsTrue()
    {
        // Arrange - Get module to Ready state via hello packet
        var helloData = new byte[] { 0x80, 0x81, 0x7E, 126, 3, 0, 0, 0, 0 };
        var helloResponse = new HelloResponse
        {
            ModuleType = ModuleType.AutoSteer,
            Version = 1,
            ReceivedAt = DateTime.UtcNow
        };

        _mockParser.Setup(p => p.ParseHelloPacket(It.IsAny<byte[]>()))
            .Returns(helloResponse);

        _mockTransport.Raise(
            t => t.DataReceived += null,
            this,
            new TransportDataReceivedEventArgs(ModuleType.AutoSteer, helloData));

        // Act
        var isReady = _coordinator.IsModuleReady(ModuleType.AutoSteer);

        // Assert
        Assert.True(isReady);
    }

    [Fact]
    public void ResetModule_ClearsStateAndIntegrals()
    {
        // Arrange - Get module to Ready state
        var helloData = new byte[] { 0x80, 0x81, 0x7E, 126, 3, 0, 0, 0, 0 };
        var helloResponse = new HelloResponse
        {
            ModuleType = ModuleType.AutoSteer,
            Version = 1,
            ReceivedAt = DateTime.UtcNow
        };

        _mockParser.Setup(p => p.ParseHelloPacket(It.IsAny<byte[]>()))
            .Returns(helloResponse);

        _mockTransport.Raise(
            t => t.DataReceived += null,
            this,
            new TransportDataReceivedEventArgs(ModuleType.AutoSteer, helloData));

        Assert.Equal(ModuleState.Ready, _coordinator.GetModuleState(ModuleType.AutoSteer));

        // Act
        _coordinator.ResetModule(ModuleType.AutoSteer);

        // Assert
        Assert.Equal(ModuleState.Disconnected, _coordinator.GetModuleState(ModuleType.AutoSteer));
        Assert.Equal(DateTime.MinValue, _coordinator.GetLastHelloTime(ModuleType.AutoSteer));
        Assert.Equal(DateTime.MinValue, _coordinator.GetLastDataTime(ModuleType.AutoSteer));
    }

    [Fact]
    public async Task InitializeModuleAsync_StartsTransportAndWaitsForReady()
    {
        // Arrange
        _mockTransport.Setup(t => t.StartTransportAsync(ModuleType.AutoSteer, TransportType.UDP))
            .Returns(Task.CompletedTask);

        _mockBuilder.Setup(b => b.BuildHelloPacket())
            .Returns(new byte[] { 0x80, 0x81, 0x7F, 200, 3, 56, 0, 0, 0 });

        var helloResponse = new HelloResponse
        {
            ModuleType = ModuleType.AutoSteer,
            Version = 1,
            ReceivedAt = DateTime.UtcNow
        };

        _mockParser.Setup(p => p.ParseHelloPacket(It.IsAny<byte[]>()))
            .Returns(helloResponse);

        // Act - Start initialization in background
        var initTask = Task.Run(() => _coordinator.InitializeModuleAsync(ModuleType.AutoSteer));

        // Simulate hello response after a short delay
        await Task.Delay(100);
        _mockTransport.Raise(
            t => t.DataReceived += null,
            this,
            new TransportDataReceivedEventArgs(ModuleType.AutoSteer, new byte[] { 0x80, 0x81, 0x7E, 126, 3, 0, 0, 0, 0 }));

        // Wait for initialization to complete
        await initTask;

        // Assert
        Assert.True(_coordinator.IsModuleReady(ModuleType.AutoSteer));
    }
}
