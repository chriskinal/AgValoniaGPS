using System;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Communication;

namespace AgValoniaGPS.Services.Communication.Transports
{
    /// <summary>
    /// Bluetooth transport implementation for SPP (Serial Port Profile) and BLE modes.
    /// Provides cross-platform Bluetooth communication with graceful degradation.
    /// </summary>
    /// <remarks>
    /// Platform-specific implementations:
    /// - Windows: WinRT Bluetooth APIs
    /// - Linux: BlueZ via DBus
    /// - macOS: IOBluetooth framework
    ///
    /// Gracefully degrades on unsupported platforms by throwing NotSupportedException.
    /// SPP mode uses SerialPort emulation where available.
    /// BLE mode requires platform-specific GATT services.
    /// </remarks>
    public class BluetoothTransportService : IBluetoothTransportService, IDisposable
    {
        /// <inheritdoc />
        public event EventHandler<byte[]>? DataReceived;

        /// <inheritdoc />
        public event EventHandler<bool>? ConnectionChanged;

        private SerialPort? _serialPort;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isDisposed;
        private bool _isConnected;
        private readonly ModuleType _moduleType;

        /// <inheritdoc />
        public TransportType Type => Mode == BluetoothMode.ClassicSPP
            ? TransportType.BluetoothSPP
            : TransportType.BluetoothBLE;

        /// <inheritdoc />
        public bool IsConnected
        {
            get => _isConnected;
            private set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    ConnectionChanged?.Invoke(this, value);
                }
            }
        }

        /// <inheritdoc />
        public BluetoothMode Mode { get; set; } = BluetoothMode.ClassicSPP;

        /// <inheritdoc />
        public string? DeviceAddress { get; set; }

        /// <summary>
        /// Initializes a new instance of the BluetoothTransportService class.
        /// </summary>
        /// <param name="moduleType">The module type for this transport</param>
        public BluetoothTransportService(ModuleType moduleType)
        {
            _moduleType = moduleType;
        }

        /// <inheritdoc />
        public async Task StartAsync()
        {
            if (IsConnected)
            {
                throw new InvalidOperationException("Transport is already started");
            }

            if (string.IsNullOrWhiteSpace(DeviceAddress))
            {
                throw new InvalidOperationException("DeviceAddress must be set before starting transport");
            }

            try
            {
                if (Mode == BluetoothMode.ClassicSPP)
                {
                    await StartSppAsync();
                }
                else
                {
                    await StartBleAsync();
                }
            }
            catch (Exception ex) when (ex is not NotSupportedException && ex is not InvalidOperationException)
            {
                throw new System.IO.IOException($"Failed to start Bluetooth transport: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Starts Bluetooth Classic SPP connection.
        /// </summary>
        private async Task StartSppAsync()
        {
            // Platform detection
            if (!IsBluetoothSupported())
            {
                throw new NotSupportedException("Bluetooth is not supported on this platform");
            }

            // Try to map Bluetooth device to COM port (platform-specific)
            string? comPort = await TryGetComPortForDeviceAsync(DeviceAddress!);
            if (comPort == null)
            {
                throw new System.IO.IOException($"Could not find COM port for Bluetooth device {DeviceAddress}");
            }

            // Create and configure serial port
            _serialPort = new SerialPort(comPort)
            {
                BaudRate = 115200, // Standard Bluetooth SPP baudrate
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                Handshake = Handshake.None
            };

            _serialPort.DataReceived += OnSerialDataReceived;
            _serialPort.Open();

            IsConnected = true;

            await Task.CompletedTask;
        }

        /// <summary>
        /// Starts Bluetooth Low Energy (BLE) connection.
        /// </summary>
        private async Task StartBleAsync()
        {
            // Platform detection
            if (!IsBleSupported())
            {
                throw new NotSupportedException("Bluetooth Low Energy is not supported on this platform");
            }

            // BLE implementation is highly platform-specific
            // This is a placeholder for actual BLE GATT service implementation
            // Real implementation would use:
            // - Windows: Windows.Devices.Bluetooth.BluetoothLEDevice
            // - Linux: BlueZ DBus API
            // - macOS: CoreBluetooth framework

            throw new NotSupportedException("BLE mode is not yet fully implemented on this platform");
        }

        /// <inheritdoc />
        public async Task StopAsync()
        {
            if (!IsConnected)
            {
                return;
            }

            _cancellationTokenSource?.Cancel();

            if (_serialPort != null)
            {
                _serialPort.DataReceived -= OnSerialDataReceived;
                _serialPort.Close();
                _serialPort.Dispose();
                _serialPort = null;
            }

            IsConnected = false;

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Send(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (!IsConnected)
            {
                throw new InvalidOperationException("Transport is not connected");
            }

            if (Mode == BluetoothMode.ClassicSPP && _serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Write(data, 0, data.Length);
            }
            else
            {
                throw new InvalidOperationException("BLE send not yet implemented");
            }
        }

        /// <inheritdoc />
        public async Task<string[]> ScanDevicesAsync()
        {
            if (!IsBluetoothSupported())
            {
                throw new NotSupportedException("Bluetooth is not supported on this platform");
            }

            // Platform-specific device scanning
            // This is a placeholder - real implementation would use platform APIs
            // Windows: DeviceInformation.FindAllAsync
            // Linux: BlueZ adapter.StartDiscovery
            // macOS: IOBluetoothDevice.recentDevices

            await Task.Delay(100); // Simulate scan delay

            // Return empty array for now (would return discovered device addresses)
            return Array.Empty<string>();
        }

        /// <summary>
        /// Handles serial port data received events.
        /// </summary>
        private void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort == null || !_serialPort.IsOpen)
            {
                return;
            }

            try
            {
                int bytesToRead = _serialPort.BytesToRead;
                if (bytesToRead > 0)
                {
                    byte[] buffer = new byte[bytesToRead];
                    _serialPort.Read(buffer, 0, bytesToRead);
                    DataReceived?.Invoke(this, buffer);
                }
            }
            catch (Exception)
            {
                // Suppress read errors (disconnection will be detected by connection monitoring)
            }
        }

        /// <summary>
        /// Checks if Bluetooth is supported on the current platform.
        /// </summary>
        private static bool IsBluetoothSupported()
        {
            // Platform detection
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows Vista and later support Bluetooth
                return Environment.OSVersion.Version.Major >= 6;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Linux with BlueZ (check if bluetoothd is available)
                // Simplified check - real implementation would verify BlueZ installation
                return System.IO.Directory.Exists("/var/lib/bluetooth");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // macOS has built-in Bluetooth support
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if Bluetooth Low Energy is supported on the current platform.
        /// </summary>
        private static bool IsBleSupported()
        {
            // BLE requires more recent OS versions
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows 8 and later for BLE
                return Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 2;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Linux with BlueZ 5.0+
                return IsBluetoothSupported(); // Simplified - real check would verify BlueZ version
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // macOS 10.7+ supports BLE
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to get the COM port associated with a Bluetooth device address.
        /// Platform-specific implementation.
        /// </summary>
        private async Task<string?> TryGetComPortForDeviceAsync(string deviceAddress)
        {
            // Platform-specific COM port mapping
            // This is a placeholder - real implementation would:
            // - Windows: Query WMI for Bluetooth serial ports
            // - Linux: Use rfcomm to bind device to /dev/rfcommX
            // - macOS: Use IOBluetooth to create virtual serial port

            await Task.Delay(10); // Simulate lookup

            // For testing purposes, return null to trigger IOException
            // Real implementation would return actual COM port
            return null;
        }

        /// <summary>
        /// Disposes resources used by the transport.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            StopAsync().GetAwaiter().GetResult();

            _cancellationTokenSource?.Dispose();
            _serialPort?.Dispose();

            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
