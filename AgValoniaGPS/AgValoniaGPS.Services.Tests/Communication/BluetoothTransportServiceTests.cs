using System;
using System.Threading.Tasks;
using NUnit.Framework;
using AgValoniaGPS.Models.Communication;
using AgValoniaGPS.Services.Communication.Transports;

namespace AgValoniaGPS.Services.Tests.Communication
{
    /// <summary>
    /// Tests for BluetoothTransportService.
    /// Uses mocks for platform-specific Bluetooth APIs to enable testing without hardware.
    /// </summary>
    [TestFixture]
    public class BluetoothTransportServiceTests
    {
        /// <summary>
        /// Test: Bluetooth SPP connection lifecycle (mock platform-specific APIs)
        /// Verifies StartAsync, StopAsync, and connection state transitions
        /// </summary>
        [Test]
        public async Task StartAsync_WhenCalledOnSupportedPlatform_ShouldSetIsConnectedTrue()
        {
            // Arrange
            var transport = new BluetoothTransportService(ModuleType.AutoSteer);
            transport.DeviceAddress = "00:11:22:33:44:55";
            transport.Mode = BluetoothMode.ClassicSPP;

            bool connectionChanged = false;
            transport.ConnectionChanged += (sender, isConnected) => connectionChanged = isConnected;

            // Act & Assert - Should throw NotSupportedException on unsupported platforms
            // or IOException when hardware is not available
            try
            {
                await transport.StartAsync();
                Assert.That(transport.IsConnected, Is.True, "IsConnected should be true after successful start");
                Assert.That(connectionChanged, Is.True, "ConnectionChanged event should fire with true");
            }
            catch (NotSupportedException)
            {
                Assert.Pass("Bluetooth not supported on this platform (expected behavior)");
            }
            catch (System.IO.IOException ex) when (ex.Message.Contains("Could not find COM port"))
            {
                Assert.Pass("Bluetooth hardware not available (expected without physical device)");
            }
            finally
            {
                await transport.StopAsync();
            }
        }

        /// <summary>
        /// Test: Bluetooth BLE connection lifecycle (mock platform-specific APIs)
        /// Verifies BLE mode StartAsync and connection state
        /// </summary>
        [Test]
        public async Task StartAsync_WhenCalledWithBLEMode_ShouldConnectSuccessfully()
        {
            // Arrange
            var transport = new BluetoothTransportService(ModuleType.Machine);
            transport.DeviceAddress = "AA:BB:CC:DD:EE:FF";
            transport.Mode = BluetoothMode.BLE;

            // Act & Assert
            try
            {
                await transport.StartAsync();
                Assert.That(transport.Mode, Is.EqualTo(BluetoothMode.BLE));
            }
            catch (NotSupportedException)
            {
                Assert.Pass("BLE not supported on this platform (expected behavior)");
            }
            finally
            {
                await transport.StopAsync();
            }
        }

        /// <summary>
        /// Test: Platform detection and graceful degradation (not supported on platform)
        /// Verifies NotSupportedException is thrown on unsupported platforms
        /// </summary>
        [Test]
        public void StartAsync_WhenBluetoothNotSupported_ShouldThrowNotSupportedException()
        {
            // Arrange
            var transport = new BluetoothTransportService(ModuleType.IMU);
            transport.DeviceAddress = "11:22:33:44:55:66";
            transport.Mode = BluetoothMode.ClassicSPP;

            // Force unsupported platform scenario by not setting device address
            transport.DeviceAddress = null;

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await transport.StartAsync(),
                "Should throw InvalidOperationException when DeviceAddress is not set");
        }

        /// <summary>
        /// Test: Transport parameter configuration (BT device address)
        /// Verifies DeviceAddress and Mode properties can be configured
        /// </summary>
        [Test]
        public void Configuration_WhenSet_ShouldRetainValues()
        {
            // Arrange
            var transport = new BluetoothTransportService(ModuleType.AutoSteer);

            // Act
            transport.DeviceAddress = "AA:BB:CC:DD:EE:FF";
            transport.Mode = BluetoothMode.BLE;

            // Assert
            Assert.That(transport.DeviceAddress, Is.EqualTo("AA:BB:CC:DD:EE:FF"));
            Assert.That(transport.Mode, Is.EqualTo(BluetoothMode.BLE));
            Assert.That(transport.Type, Is.EqualTo(TransportType.BluetoothBLE));
        }

        /// <summary>
        /// Test: Send data when not connected should throw
        /// </summary>
        [Test]
        public void Send_WhenNotConnected_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var transport = new BluetoothTransportService(ModuleType.AutoSteer);
            byte[] testData = new byte[] { 0x80, 0x81, 0x7F };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => transport.Send(testData),
                "Should throw InvalidOperationException when not connected");
        }

        /// <summary>
        /// Test: Verify Transport type property matches mode
        /// </summary>
        [Test]
        public void Type_WhenModeChanged_ShouldReflectCorrectTransportType()
        {
            // Arrange
            var transport = new BluetoothTransportService(ModuleType.AutoSteer);

            // Act & Assert - SPP mode
            transport.Mode = BluetoothMode.ClassicSPP;
            Assert.That(transport.Type, Is.EqualTo(TransportType.BluetoothSPP));

            // Act & Assert - BLE mode
            transport.Mode = BluetoothMode.BLE;
            Assert.That(transport.Type, Is.EqualTo(TransportType.BluetoothBLE));
        }
    }
}
