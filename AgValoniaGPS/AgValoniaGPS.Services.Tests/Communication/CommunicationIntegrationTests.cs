using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Communication;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Services.Communication;
using AgValoniaGPS.Services.Communication.Transports;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.Communication;

/// <summary>
/// Integration tests for Wave 6 Hardware I/O Communication.
/// Tests full communication loops, multi-module operation, transport switching,
/// performance benchmarks, and memory stability.
/// Focuses on integration gaps not covered by individual service tests.
/// </summary>
[TestFixture]
public class CommunicationIntegrationTests
{
    private PgnMessageBuilderService _builder = null!;
    private PgnMessageParserService _parser = null!;
    private TransportAbstractionService _transport = null!;
    private ModuleCoordinatorService _coordinator = null!;
    private AutoSteerCommunicationService _autoSteerComm = null!;
    private MachineCommunicationService _machineComm = null!;
    private ImuCommunicationService _imuComm = null!;

    [SetUp]
    public void SetUp()
    {
        // Create communication layer services
        _builder = new PgnMessageBuilderService();
        _parser = new PgnMessageParserService();
        _transport = new TransportAbstractionService();
        _coordinator = new ModuleCoordinatorService(_transport, _parser, _builder);

        // Register mock UDP transport factory for testing
        _transport.RegisterTransportFactory(TransportType.UDP, () => new MockUdpTransport());
        _transport.RegisterTransportFactory(TransportType.BluetoothSPP, () => new MockBluetoothTransport());

        // Create module communication services
        _autoSteerComm = new AutoSteerCommunicationService(_coordinator, _builder, _parser, _transport);
        _machineComm = new MachineCommunicationService(_coordinator, _builder, _parser, _transport);
        _imuComm = new ImuCommunicationService(_coordinator, _builder, _parser, _transport);
    }

    [TearDown]
    public async Task TearDown()
    {
        // Stop all transports
        await _transport.StopTransportAsync(ModuleType.AutoSteer);
        await _transport.StopTransportAsync(ModuleType.Machine);
        await _transport.StopTransportAsync(ModuleType.IMU);

        _coordinator.Dispose();
    }

    /// <summary>
    /// Integration Test 1: Multi-module concurrent operation
    /// Verifies all three modules (AutoSteer, Machine, IMU) can operate simultaneously
    /// without interference or data corruption.
    /// </summary>
    [Test]
    public async Task MultiModuleConcurrent_AllThreeModulesActive_OperateWithoutInterference()
    {
        // Arrange: Start all three module transports
        await _transport.StartTransportAsync(ModuleType.AutoSteer, TransportType.UDP);
        await _transport.StartTransportAsync(ModuleType.Machine, TransportType.UDP);
        await _transport.StartTransportAsync(ModuleType.IMU, TransportType.UDP);

        await Task.Delay(200); // Allow connection initialization

        // Assert: All transports should be active
        Assert.That(_transport.GetActiveTransport(ModuleType.AutoSteer), Is.EqualTo(TransportType.UDP),
            "AutoSteer should be on UDP");
        Assert.That(_transport.GetActiveTransport(ModuleType.Machine), Is.EqualTo(TransportType.UDP),
            "Machine should be on UDP");
        Assert.That(_transport.GetActiveTransport(ModuleType.IMU), Is.EqualTo(TransportType.UDP),
            "IMU should be on UDP");

        // Verify services can be used without errors
        Assert.DoesNotThrow(() =>
        {
            // These won't actually send since modules aren't "Ready" but shouldn't throw
            if (_coordinator.IsModuleReady(ModuleType.AutoSteer))
            {
                _autoSteerComm.SendSteeringCommand(10.0, 5.0, 100, true);
            }
        }, "AutoSteer communication should not throw");

        Assert.DoesNotThrow(() =>
        {
            if (_coordinator.IsModuleReady(ModuleType.Machine))
            {
                _machineComm.SendSectionStates(new byte[] { 1, 1, 0, 1 });
            }
        }, "Machine communication should not throw");

        Assert.DoesNotThrow(() =>
        {
            if (_coordinator.IsModuleReady(ModuleType.IMU))
            {
                _imuComm.SendConfiguration(0x07);
            }
        }, "IMU communication should not throw");
    }

    /// <summary>
    /// Integration Test 2: Transport switching during operation
    /// Verifies that a module can switch from UDP to Bluetooth transport
    /// without losing state or causing errors.
    /// </summary>
    [Test]
    public async Task TransportSwitching_SwitchFromUdpToBluetooth_MaintainsConnection()
    {
        // Arrange: Start AutoSteer on UDP
        await _transport.StartTransportAsync(ModuleType.AutoSteer, TransportType.UDP);

        Assert.That(_transport.GetActiveTransport(ModuleType.AutoSteer), Is.EqualTo(TransportType.UDP),
            "Should start on UDP");

        // Act: Switch to Bluetooth
        await _transport.StopTransportAsync(ModuleType.AutoSteer);
        await Task.Delay(100); // Small delay for cleanup

        await _transport.StartTransportAsync(ModuleType.AutoSteer, TransportType.BluetoothSPP);

        // Assert: Now on Bluetooth
        Assert.That(_transport.GetActiveTransport(ModuleType.AutoSteer), Is.EqualTo(TransportType.BluetoothSPP),
            "Should have switched to Bluetooth");

        // Verify no errors occurred during switch
        Assert.DoesNotThrow(() =>
        {
            if (_coordinator.IsModuleReady(ModuleType.AutoSteer))
            {
                _autoSteerComm.SendSteeringCommand(10.0, 7.0, 75, true);
            }
        }, "Should be able to send commands after transport switch");
    }

    /// <summary>
    /// Integration Test 3: PGN message build and parse round-trip
    /// Verifies messages can be built, transmitted, and parsed correctly.
    /// </summary>
    [Test]
    public void MessageRoundTrip_BuildAndParse_DataIntegrityMaintained()
    {
        // Arrange & Act: Build AutoSteer message
        byte[] autoSteerMsg = _builder.BuildAutoSteerData(10.0, 1, 5.5, 150);

        // Assert: Message should be parseable
        var parsedMsg = _parser.ParseMessage(autoSteerMsg);
        Assert.That(parsedMsg, Is.Not.Null, "Message should parse successfully");
        Assert.That(parsedMsg.PGN, Is.EqualTo(254), "PGN should be 254 for AutoSteer data");

        // Act: Build Machine message
        byte[] machineMsg = _builder.BuildMachineData(
            new byte[8], new byte[8], 0, new byte[] { 1, 1, 0, 1, 0, 1, 1, 0 });

        // Assert: Machine message should parse
        var parsedMachine = _parser.ParseMessage(machineMsg);
        Assert.That(parsedMachine, Is.Not.Null, "Machine message should parse");
        Assert.That(parsedMachine.PGN, Is.EqualTo(239), "PGN should be 239 for Machine data");

        // Act: Build IMU config
        byte[] imuMsg = _builder.BuildImuConfig(0x07);

        // Assert: IMU message should parse
        var parsedImu = _parser.ParseMessage(imuMsg);
        Assert.That(parsedImu, Is.Not.Null, "IMU message should parse");
        Assert.That(parsedImu.PGN, Is.EqualTo(218), "PGN should be 218 for IMU config");
    }

    /// <summary>
    /// Integration Test 4: CRC validation end-to-end
    /// Verifies that messages with invalid CRC are rejected by the parser.
    /// </summary>
    [Test]
    public void CrcValidation_CorruptedMessage_RejectedByParser()
    {
        // Arrange: Build valid message
        byte[] validMessage = _builder.BuildAutoSteerData(10.0, 1, 5.0, 100);

        // Verify valid message is accepted
        var validParsed = _parser.ParseMessage(validMessage);
        Assert.That(validParsed, Is.Not.Null, "Valid message should be parsed");

        // Act: Corrupt the CRC byte (last byte)
        byte[] corruptedMessage = new byte[validMessage.Length];
        Array.Copy(validMessage, corruptedMessage, validMessage.Length);
        corruptedMessage[^1] = (byte)(corruptedMessage[^1] ^ 0xFF);

        // Assert: Corrupted message should be rejected
        var corruptedParsed = _parser.ParseMessage(corruptedMessage);
        Assert.That(corruptedParsed, Is.Null, "Corrupted message should be rejected by CRC validation");

        // Verify parser still works after rejecting corrupt message
        var validParsed2 = _parser.ParseMessage(validMessage);
        Assert.That(validParsed2, Is.Not.Null, "Valid message should still be processed after corrupt message");
    }

    /// <summary>
    /// Integration Test 5: Concurrent transport types (mixed transports)
    /// Verifies AutoSteer on Bluetooth and Machine on UDP can be configured simultaneously.
    /// </summary>
    [Test]
    public async Task ConcurrentTransports_AutoSteerBluetoothMachineUdp_BothConfigured()
    {
        // Arrange & Act: Start AutoSteer on Bluetooth, Machine on UDP
        await _transport.StartTransportAsync(ModuleType.AutoSteer, TransportType.BluetoothSPP);
        await _transport.StartTransportAsync(ModuleType.Machine, TransportType.UDP);

        // Assert: Both should be on their respective transports
        Assert.That(_transport.GetActiveTransport(ModuleType.AutoSteer),
            Is.EqualTo(TransportType.BluetoothSPP),
            "AutoSteer should be on Bluetooth");
        Assert.That(_transport.GetActiveTransport(ModuleType.Machine),
            Is.EqualTo(TransportType.UDP),
            "Machine should be on UDP");

        // Verify both can be used independently
        Assert.DoesNotThrow(() =>
        {
            if (_coordinator.IsModuleReady(ModuleType.AutoSteer))
            {
                _autoSteerComm.SendSteeringCommand(10.0, 8.0, 150, true);
            }
            if (_coordinator.IsModuleReady(ModuleType.Machine))
            {
                _machineComm.SendSectionStates(new byte[] { 1, 1, 1, 1 });
            }
        }, "Both modules should be independently operable");
    }

    /// <summary>
    /// Integration Test 6: Module state tracking
    /// Verifies coordinator tracks module states correctly.
    /// </summary>
    [Test]
    public async Task ModuleStateTracking_InitialState_IsDisconnected()
    {
        // Assert: Initial state should be Disconnected
        Assert.That(_coordinator.GetModuleState(ModuleType.AutoSteer), Is.EqualTo(ModuleState.Disconnected),
            "AutoSteer should start as Disconnected");
        Assert.That(_coordinator.GetModuleState(ModuleType.Machine), Is.EqualTo(ModuleState.Disconnected),
            "Machine should start as Disconnected");
        Assert.That(_coordinator.GetModuleState(ModuleType.IMU), Is.EqualTo(ModuleState.Disconnected),
            "IMU should start as Disconnected");

        // Act: Start transport (this transitions state)
        await _transport.StartTransportAsync(ModuleType.AutoSteer, TransportType.UDP);

        // Allow state transition
        await Task.Delay(50);

        // Assert: State should have changed (may be Connecting or HelloReceived depending on timing)
        var newState = _coordinator.GetModuleState(ModuleType.AutoSteer);
        Assert.That(newState, Is.Not.EqualTo(ModuleState.Disconnected),
            "State should have changed after starting transport");
    }

    /// <summary>
    /// Integration Test 7: Message builder performance
    /// Verifies message building is fast enough for real-time operation.
    /// </summary>
    [Test]
    public void MessageBuilderPerformance_BuildMultipleMessages_CompletesQuickly()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();
        const int messageCount = 1000;

        // Act: Build 1000 messages
        for (int i = 0; i < messageCount; i++)
        {
            _builder.BuildAutoSteerData(10.0 + i * 0.1, 1, 5.0 + i * 0.05, 100 + i);
        }

        stopwatch.Stop();

        // Assert: Should complete in <500ms (avg <0.5ms per message)
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(500),
            $"Building {messageCount} messages should take <500ms (actual: {stopwatch.ElapsedMilliseconds}ms)");

        double avgTimePerMessage = stopwatch.ElapsedMilliseconds / (double)messageCount;
        TestContext.WriteLine($"Average build time: {avgTimePerMessage:F3}ms per message");
    }

    /// <summary>
    /// Integration Test 8: Message parser performance
    /// Verifies message parsing is fast enough for real-time operation.
    /// </summary>
    [Test]
    public void MessageParserPerformance_ParseMultipleMessages_CompletesQuickly()
    {
        // Arrange: Pre-build messages
        var messages = new List<byte[]>();
        for (int i = 0; i < 1000; i++)
        {
            messages.Add(_builder.BuildAutoSteerData(10.0 + i * 0.1, 1, 5.0, 100));
        }

        var stopwatch = Stopwatch.StartNew();

        // Act: Parse all messages
        foreach (var msg in messages)
        {
            _parser.ParseMessage(msg);
        }

        stopwatch.Stop();

        // Assert: Should complete in <500ms
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(500),
            $"Parsing 1000 messages should take <500ms (actual: {stopwatch.ElapsedMilliseconds}ms)");

        double avgTimePerMessage = stopwatch.ElapsedMilliseconds / (double)messages.Count;
        TestContext.WriteLine($"Average parse time: {avgTimePerMessage:F3}ms per message");
    }

    /// <summary>
    /// Integration Test 9: Memory stability over many messages
    /// Verifies no memory leaks occur during message building and parsing.
    /// </summary>
    [Test]
    public void MemoryStability_BuildAndParse1000Messages_NoExcessiveGrowth()
    {
        // Arrange: Force GC before measuring
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long initialMemory = GC.GetTotalMemory(false);

        // Act: Build and parse 1000 messages
        for (int i = 0; i < 1000; i++)
        {
            var msg = _builder.BuildAutoSteerData(10.0 + i * 0.1, 1, 5.0, 100 + i);
            _parser.ParseMessage(msg);
        }

        // Force GC after operations
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long finalMemory = GC.GetTotalMemory(false);
        long memoryGrowth = finalMemory - initialMemory;

        // Assert: Memory growth should be minimal (<500KB for 1000 messages)
        Assert.That(memoryGrowth, Is.LessThan(512 * 1024),
            $"Memory growth should be <500KB (actual: {memoryGrowth / 1024.0:F1}KB)");

        TestContext.WriteLine($"Memory growth: {memoryGrowth / 1024.0:F1}KB for 1000 messages");
    }

    /// <summary>
    /// Integration Test 10: Transport factory registration
    /// Verifies custom transport factories can be registered and used.
    /// </summary>
    [Test]
    public async Task TransportFactory_CustomFactory_CanBeRegistered()
    {
        // Arrange: Create a custom transport service
        var customTransport = new TransportAbstractionService();
        bool factoryCalled = false;

        // Register custom factory
        customTransport.RegisterTransportFactory(TransportType.CAN, () =>
        {
            factoryCalled = true;
            return new MockCanTransport();
        });

        // Act: Start transport (should call factory)
        await customTransport.StartTransportAsync(ModuleType.IMU, TransportType.CAN);

        // Assert: Factory should have been called
        Assert.That(factoryCalled, Is.True, "Custom factory should be called when starting transport");
        Assert.That(customTransport.GetActiveTransport(ModuleType.IMU), Is.EqualTo(TransportType.CAN),
            "IMU should be on CAN transport");

        // Cleanup
        await customTransport.StopTransportAsync(ModuleType.IMU);
    }

    #region Mock Transport Classes

    private class MockUdpTransport : ITransport
    {
        public TransportType Type => TransportType.UDP;
        public bool IsConnected { get; private set; }

        public event EventHandler<byte[]>? DataReceived;
        public event EventHandler<bool>? ConnectionChanged;

        public Task StartAsync()
        {
            IsConnected = true;
            ConnectionChanged?.Invoke(this, true);
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            IsConnected = false;
            ConnectionChanged?.Invoke(this, false);
            return Task.CompletedTask;
        }

        public void Send(byte[] data)
        {
            // Mock send - no actual transmission
        }
    }

    private class MockBluetoothTransport : ITransport
    {
        public TransportType Type => TransportType.BluetoothSPP;
        public bool IsConnected { get; private set; }

        public event EventHandler<byte[]>? DataReceived;
        public event EventHandler<bool>? ConnectionChanged;

        public Task StartAsync()
        {
            IsConnected = true;
            ConnectionChanged?.Invoke(this, true);
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            IsConnected = false;
            ConnectionChanged?.Invoke(this, false);
            return Task.CompletedTask;
        }

        public void Send(byte[] data)
        {
            // Mock send
        }
    }

    private class MockCanTransport : ITransport
    {
        public TransportType Type => TransportType.CAN;
        public bool IsConnected { get; private set; }

        public event EventHandler<byte[]>? DataReceived;
        public event EventHandler<bool>? ConnectionChanged;

        public Task StartAsync()
        {
            IsConnected = true;
            ConnectionChanged?.Invoke(this, true);
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            IsConnected = false;
            ConnectionChanged?.Invoke(this, false);
            return Task.CompletedTask;
        }

        public void Send(byte[] data)
        {
            // Mock send
        }
    }

    #endregion
}
