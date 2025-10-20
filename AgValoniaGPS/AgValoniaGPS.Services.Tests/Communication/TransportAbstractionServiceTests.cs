using System;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Communication;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Services.Communication;
using AgValoniaGPS.Services.Communication.Transports;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.Communication
{
    /// <summary>
    /// Tests for TransportAbstractionService to verify transport selection, lifecycle management,
    /// message routing, and event forwarding across multiple module types.
    /// </summary>
    [TestFixture]
    public class TransportAbstractionServiceTests
    {
        private TransportAbstractionService _service;
        private MockTransport _mockUdpTransport;
        private MockTransport _mockBluetoothTransport;
        private MockTransport _mockCanTransport;

        [SetUp]
        public void SetUp()
        {
            _service = new TransportAbstractionService();
            _mockUdpTransport = new MockTransport(TransportType.UDP);
            _mockBluetoothTransport = new MockTransport(TransportType.BluetoothSPP);
            _mockCanTransport = new MockTransport(TransportType.CAN);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up any running transports
            _service.StopTransportAsync(ModuleType.AutoSteer).Wait();
            _service.StopTransportAsync(ModuleType.Machine).Wait();
            _service.StopTransportAsync(ModuleType.IMU).Wait();
        }

        /// <summary>
        /// Test 1: Verify per-module transport selection works correctly.
        /// AutoSteer uses Bluetooth, Machine uses UDP, IMU uses CAN.
        /// </summary>
        [Test]
        public async Task PerModuleTransportSelection_WorksCorrectly()
        {
            // Arrange - Register transport factories
            _service.RegisterTransportFactory(TransportType.BluetoothSPP, () => new MockTransport(TransportType.BluetoothSPP));
            _service.RegisterTransportFactory(TransportType.UDP, () => new MockTransport(TransportType.UDP));
            _service.RegisterTransportFactory(TransportType.CAN, () => new MockTransport(TransportType.CAN));

            // Act - Start different transports for each module
            await _service.StartTransportAsync(ModuleType.AutoSteer, TransportType.BluetoothSPP);
            await _service.StartTransportAsync(ModuleType.Machine, TransportType.UDP);
            await _service.StartTransportAsync(ModuleType.IMU, TransportType.CAN);

            // Assert - Verify each module has the correct transport type
            Assert.That(_service.GetActiveTransport(ModuleType.AutoSteer), Is.EqualTo(TransportType.BluetoothSPP),
                "AutoSteer should be using Bluetooth transport");
            Assert.That(_service.GetActiveTransport(ModuleType.Machine), Is.EqualTo(TransportType.UDP),
                "Machine should be using UDP transport");
            Assert.That(_service.GetActiveTransport(ModuleType.IMU), Is.EqualTo(TransportType.CAN),
                "IMU should be using CAN transport");
        }

        /// <summary>
        /// Test 2: Verify transport lifecycle (start/stop) with mock transport.
        /// </summary>
        [Test]
        public async Task TransportLifecycle_StartAndStop_WorksCorrectly()
        {
            // Arrange
            var mockTransport = new MockTransport(TransportType.UDP);
            _service.RegisterTransportFactory(TransportType.UDP, () => mockTransport);

            // Act - Start transport
            await _service.StartTransportAsync(ModuleType.AutoSteer, TransportType.UDP);

            // Assert - Transport should be started
            Assert.That(mockTransport.StartAsyncCallCount, Is.EqualTo(1), "StartAsync should be called once");
            Assert.That(_service.GetActiveTransport(ModuleType.AutoSteer), Is.EqualTo(TransportType.UDP));

            // Act - Stop transport
            await _service.StopTransportAsync(ModuleType.AutoSteer);

            // Assert - Transport should be stopped
            Assert.That(mockTransport.StopAsyncCallCount, Is.EqualTo(1), "StopAsync should be called once");
        }

        /// <summary>
        /// Test 3: Verify message routing sends to correct transport based on module.
        /// </summary>
        [Test]
        public async Task MessageRouting_SendsToCorrectTransport()
        {
            // Arrange - Set up mock transports for two modules
            var autoSteerTransport = new MockTransport(TransportType.BluetoothSPP);
            var machineTransport = new MockTransport(TransportType.UDP);

            _service.RegisterTransportFactory(TransportType.BluetoothSPP, () => autoSteerTransport);
            _service.RegisterTransportFactory(TransportType.UDP, () => machineTransport);

            await _service.StartTransportAsync(ModuleType.AutoSteer, TransportType.BluetoothSPP);
            await _service.StartTransportAsync(ModuleType.Machine, TransportType.UDP);

            byte[] autoSteerData = new byte[] { 0x80, 0x81, 0x7F, 254, 10 };
            byte[] machineData = new byte[] { 0x80, 0x81, 0x7F, 239, 8 };

            // Act - Send messages to different modules
            _service.SendMessage(ModuleType.AutoSteer, autoSteerData);
            _service.SendMessage(ModuleType.Machine, machineData);

            // Assert - Verify correct transport received correct data
            Assert.That(autoSteerTransport.LastSentData, Is.EqualTo(autoSteerData),
                "AutoSteer data should be sent to Bluetooth transport");
            Assert.That(machineTransport.LastSentData, Is.EqualTo(machineData),
                "Machine data should be sent to UDP transport");
        }

        /// <summary>
        /// Test 4: Verify DataReceived event forwarding from transport to abstraction layer.
        /// </summary>
        [Test]
        public async Task EventForwarding_DataReceived_ForwardsCorrectly()
        {
            // Arrange
            var mockTransport = new MockTransport(TransportType.UDP);
            _service.RegisterTransportFactory(TransportType.UDP, () => mockTransport);

            await _service.StartTransportAsync(ModuleType.AutoSteer, TransportType.UDP);

            TransportDataReceivedEventArgs? receivedEvent = null;
            _service.DataReceived += (sender, args) => receivedEvent = args;

            byte[] testData = new byte[] { 0x80, 0x81, 0x7E, 253, 8 };

            // Act - Simulate transport receiving data
            mockTransport.SimulateDataReceived(testData);

            // Assert - Verify event was forwarded
            Assert.That(receivedEvent, Is.Not.Null, "DataReceived event should be raised");
            Assert.That(receivedEvent!.Module, Is.EqualTo(ModuleType.AutoSteer),
                "Event should identify AutoSteer as source module");
            Assert.That(receivedEvent.Data, Is.EqualTo(testData),
                "Event should contain the received data");
            Assert.That(receivedEvent.ReceivedAt, Is.Not.EqualTo(default(DateTime)),
                "Event should have a timestamp");
        }

        /// <summary>
        /// Test 5: Verify TransportStateChanged event publishing when connection state changes.
        /// </summary>
        [Test]
        public async Task EventForwarding_TransportStateChanged_PublishesCorrectly()
        {
            // Arrange
            var mockTransport = new MockTransport(TransportType.BluetoothSPP);
            _service.RegisterTransportFactory(TransportType.BluetoothSPP, () => mockTransport);

            await _service.StartTransportAsync(ModuleType.Machine, TransportType.BluetoothSPP);

            TransportStateChangedEventArgs? receivedEvent = null;
            _service.TransportStateChanged += (sender, args) => receivedEvent = args;

            // Act - Simulate connection state change
            mockTransport.SimulateConnectionChanged(true);

            // Assert - Verify event was published
            Assert.That(receivedEvent, Is.Not.Null, "TransportStateChanged event should be raised");
            Assert.That(receivedEvent!.Module, Is.EqualTo(ModuleType.Machine),
                "Event should identify Machine as the module");
            Assert.That(receivedEvent.Transport, Is.EqualTo(TransportType.BluetoothSPP),
                "Event should identify Bluetooth as transport type");
            Assert.That(receivedEvent.IsConnected, Is.True,
                "Event should indicate connected state");
        }

        /// <summary>
        /// Test 6: Verify GetActiveTransport returns correct transport type for each module.
        /// </summary>
        [Test]
        public async Task GetActiveTransport_ReturnsCorrectTransportType()
        {
            // Arrange
            var udpTransport = new MockTransport(TransportType.UDP);
            var canTransport = new MockTransport(TransportType.CAN);

            _service.RegisterTransportFactory(TransportType.UDP, () => udpTransport);
            _service.RegisterTransportFactory(TransportType.CAN, () => canTransport);

            // Act
            await _service.StartTransportAsync(ModuleType.AutoSteer, TransportType.UDP);
            await _service.StartTransportAsync(ModuleType.IMU, TransportType.CAN);

            // Assert
            Assert.That(_service.GetActiveTransport(ModuleType.AutoSteer), Is.EqualTo(TransportType.UDP),
                "AutoSteer should return UDP transport");
            Assert.That(_service.GetActiveTransport(ModuleType.IMU), Is.EqualTo(TransportType.CAN),
                "IMU should return CAN transport");

            // Verify that Machine (not started) throws or returns default
            Assert.Throws<InvalidOperationException>(() => _service.GetActiveTransport(ModuleType.Machine),
                "Getting transport for module without active transport should throw");
        }
    }

    /// <summary>
    /// Mock transport implementation for testing the abstraction layer.
    /// Tracks method calls and simulates events.
    /// </summary>
    internal class MockTransport : ITransport
    {
        private bool _isConnected;

        public MockTransport(TransportType type)
        {
            Type = type;
        }

        public TransportType Type { get; }
        public bool IsConnected => _isConnected;

        public int StartAsyncCallCount { get; private set; }
        public int StopAsyncCallCount { get; private set; }
        public byte[]? LastSentData { get; private set; }

        public event EventHandler<byte[]>? DataReceived;
        public event EventHandler<bool>? ConnectionChanged;

        public Task StartAsync()
        {
            StartAsyncCallCount++;
            _isConnected = true;
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            StopAsyncCallCount++;
            _isConnected = false;
            return Task.CompletedTask;
        }

        public void Send(byte[] data)
        {
            LastSentData = data;
        }

        /// <summary>
        /// Simulates receiving data from hardware (for testing event forwarding).
        /// </summary>
        public void SimulateDataReceived(byte[] data)
        {
            DataReceived?.Invoke(this, data);
        }

        /// <summary>
        /// Simulates connection state change (for testing event forwarding).
        /// </summary>
        public void SimulateConnectionChanged(bool isConnected)
        {
            _isConnected = isConnected;
            ConnectionChanged?.Invoke(this, isConnected);
        }
    }
}
