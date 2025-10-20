using System;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Communication;

namespace AgValoniaGPS.Services.Communication.Transports
{
    /// <summary>
    /// Radio transport implementation for LoRa, 900MHz, and WiFi radio modules.
    /// Provides long-range wireless communication (1+ mile for LoRa).
    /// </summary>
    /// <remarks>
    /// Supported radio types:
    /// - LoRa (SX1276/SX1278): Configured for 1-mile range with spread factor 7-12
    /// - 900MHz (RFM69): Frequency hopping spread spectrum
    /// - WiFi (ESP32/ESP8266): UDP bridge mode
    ///
    /// LoRa configuration for 1-mile range:
    /// - Transmit power: 17-20 dBm
    /// - Spread factor: 9-12
    /// - Bandwidth: 125 kHz
    /// - Coding rate: 4/5
    ///
    /// Radio modules typically connect via UART (Serial) or SPI.
    /// This implementation uses UART for simplicity.
    /// </remarks>
    public class RadioTransportService : IRadioTransportService, IDisposable
    {
        /// <inheritdoc />
        public event EventHandler<byte[]>? DataReceived;

        /// <inheritdoc />
        public event EventHandler<bool>? ConnectionChanged;

        private const int LORA_DEFAULT_TX_POWER = 20; // 20 dBm for 1-mile range
        private const int LORA_DEFAULT_FREQUENCY_US = 915000000; // 915 MHz US ISM band
        private const int SPREAD_900MHZ_DEFAULT_FREQUENCY = 902000000; // 902 MHz

        private SerialPort? _serialPort;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _receiveTask;
        private bool _isDisposed;
        private bool _isConnected;
        private readonly ModuleType _moduleType;

        /// <inheritdoc />
        public TransportType Type => TransportType.Radio;

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
        public RadioType RadioType { get; set; } = RadioType.LoRa;

        /// <inheritdoc />
        public int TransmitPower { get; set; } = LORA_DEFAULT_TX_POWER;

        /// <inheritdoc />
        public int Frequency { get; set; } = LORA_DEFAULT_FREQUENCY_US;

        /// <summary>
        /// Gets or sets the serial port name for UART-connected radio modules.
        /// Examples: "COM3" (Windows), "/dev/ttyUSB0" (Linux), "/dev/cu.usbserial-0001" (macOS).
        /// </summary>
        public string? SerialPortName { get; set; }

        /// <summary>
        /// Initializes a new instance of the RadioTransportService class.
        /// </summary>
        /// <param name="moduleType">The module type for this transport</param>
        public RadioTransportService(ModuleType moduleType)
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

            if (Frequency <= 0)
            {
                throw new InvalidOperationException("Frequency must be set before starting transport");
            }

            try
            {
                // Detect radio module
                string? detectedPort = await DetectRadioModuleAsync();
                if (detectedPort == null)
                {
                    throw new System.IO.IOException("Radio module not detected on any serial port");
                }

                SerialPortName = detectedPort;

                // Initialize radio module via serial port
                await InitializeRadioModuleAsync();

                IsConnected = true;
            }
            catch (Exception ex) when (ex is not NotSupportedException && ex is not InvalidOperationException)
            {
                throw new System.IO.IOException($"Failed to start radio transport: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public async Task StopAsync()
        {
            if (!IsConnected)
            {
                return;
            }

            _cancellationTokenSource?.Cancel();

            if (_receiveTask != null)
            {
                await _receiveTask;
            }

            if (_serialPort != null)
            {
                _serialPort.DataReceived -= OnSerialDataReceived;
                _serialPort.Close();
                _serialPort.Dispose();
                _serialPort = null;
            }

            IsConnected = false;
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

            if (_serialPort == null || !_serialPort.IsOpen)
            {
                throw new InvalidOperationException("Radio module is not connected");
            }

            // Wrap data in radio-specific framing
            byte[] framedData = FrameDataForRadio(data);

            // Send via serial port to radio module
            _serialPort.Write(framedData, 0, framedData.Length);
        }

        /// <summary>
        /// Detects radio module on available serial ports.
        /// </summary>
        private async Task<string?> DetectRadioModuleAsync()
        {
            // If SerialPortName is already set, use it
            if (!string.IsNullOrWhiteSpace(SerialPortName))
            {
                return SerialPortName;
            }

            // Platform-specific serial port enumeration
            string[] availablePorts = SerialPort.GetPortNames();

            if (availablePorts.Length == 0)
            {
                return null;
            }

            // Try each port to detect radio module
            foreach (string port in availablePorts)
            {
                try
                {
                    using var testPort = new SerialPort(port, 115200);
                    testPort.Open();
                    await Task.Delay(100);

                    // Send AT command to detect module (LoRa and 900MHz modules often use AT commands)
                    testPort.WriteLine("AT");
                    await Task.Delay(100);

                    if (testPort.BytesToRead > 0)
                    {
                        string response = testPort.ReadExisting();
                        if (response.Contains("OK") || response.Contains("LoRa"))
                        {
                            testPort.Close();
                            return port;
                        }
                    }

                    testPort.Close();
                }
                catch
                {
                    // Ignore errors and try next port
                }
            }

            return null;
        }

        /// <summary>
        /// Initializes the radio module with configuration parameters.
        /// </summary>
        private async Task InitializeRadioModuleAsync()
        {
            if (string.IsNullOrWhiteSpace(SerialPortName))
            {
                throw new InvalidOperationException("SerialPortName must be set");
            }

            // Create serial port connection
            _serialPort = new SerialPort(SerialPortName)
            {
                BaudRate = 115200, // Standard baud rate for radio modules
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                ReadTimeout = 1000,
                WriteTimeout = 1000
            };

            _serialPort.DataReceived += OnSerialDataReceived;
            _serialPort.Open();

            // Configure radio module based on type
            await ConfigureRadioModuleAsync();

            // Start receive loop
            _cancellationTokenSource = new CancellationTokenSource();
            _receiveTask = Task.Run(() => ReceiveLoopAsync(_cancellationTokenSource.Token));
        }

        /// <summary>
        /// Configures the radio module with type-specific settings.
        /// </summary>
        private async Task ConfigureRadioModuleAsync()
        {
            if (_serialPort == null || !_serialPort.IsOpen)
            {
                return;
            }

            switch (RadioType)
            {
                case RadioType.LoRa:
                    await ConfigureLoRaModuleAsync();
                    break;

                case RadioType.Spread900MHz:
                    await Configure900MHzModuleAsync();
                    break;

                case RadioType.WiFi:
                    await ConfigureWiFiModuleAsync();
                    break;
            }
        }

        /// <summary>
        /// Configures LoRa module for 1-mile range.
        /// </summary>
        private async Task ConfigureLoRaModuleAsync()
        {
            if (_serialPort == null)
            {
                return;
            }

            // Configure LoRa module via AT commands (typical for SX1276/SX1278 modules)
            // These are example commands - actual commands depend on module firmware

            // Set frequency
            _serialPort.WriteLine($"AT+FREQ={Frequency / 1000}"); // Frequency in kHz
            await Task.Delay(100);

            // Set transmit power
            _serialPort.WriteLine($"AT+POWER={TransmitPower}"); // Power in dBm
            await Task.Delay(100);

            // Set spread factor for 1-mile range (SF9-12 for long range)
            _serialPort.WriteLine("AT+SF=10"); // Spread factor 10
            await Task.Delay(100);

            // Set bandwidth (125 kHz for long range)
            _serialPort.WriteLine("AT+BW=125"); // Bandwidth in kHz
            await Task.Delay(100);

            // Set coding rate (4/5 for error correction)
            _serialPort.WriteLine("AT+CR=5"); // Coding rate 4/5
            await Task.Delay(100);
        }

        /// <summary>
        /// Configures 900MHz spread spectrum module.
        /// </summary>
        private async Task Configure900MHzModuleAsync()
        {
            if (_serialPort == null)
            {
                return;
            }

            // Configure 900MHz module
            _serialPort.WriteLine($"AT+FREQ={Frequency / 1000}");
            await Task.Delay(100);

            _serialPort.WriteLine($"AT+POWER={TransmitPower}");
            await Task.Delay(100);
        }

        /// <summary>
        /// Configures WiFi module (ESP32/ESP8266) in UDP bridge mode.
        /// </summary>
        private async Task ConfigureWiFiModuleAsync()
        {
            if (_serialPort == null)
            {
                return;
            }

            // Configure WiFi module in transparent UDP mode
            // This is simplified - actual WiFi configuration would require SSID, password, etc.
            _serialPort.WriteLine("AT+CWMODE=1"); // Station mode
            await Task.Delay(100);

            // WiFi connection would happen here in real implementation
        }

        /// <summary>
        /// Receive loop for radio data.
        /// </summary>
        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            // Serial port data received event handles incoming data
            // This loop just keeps the task alive
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100, cancellationToken);
            }
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

                    // Unframe data from radio-specific framing
                    byte[] unframedData = UnframeDataFromRadio(buffer);

                    DataReceived?.Invoke(this, unframedData);
                }
            }
            catch (Exception)
            {
                // Suppress read errors
            }
        }

        /// <summary>
        /// Frames data for radio transmission (adds radio-specific header/footer).
        /// </summary>
        private byte[] FrameDataForRadio(byte[] data)
        {
            // Simple framing: [START_BYTE] [LENGTH] [DATA...] [END_BYTE]
            // Real implementation would use radio-specific framing

            byte[] framedData = new byte[data.Length + 3];
            framedData[0] = 0x02; // STX (Start of Text)
            framedData[1] = (byte)data.Length;
            Array.Copy(data, 0, framedData, 2, data.Length);
            framedData[framedData.Length - 1] = 0x03; // ETX (End of Text)

            return framedData;
        }

        /// <summary>
        /// Unframes data from radio reception (removes radio-specific header/footer).
        /// </summary>
        private byte[] UnframeDataFromRadio(byte[] framedData)
        {
            // Remove framing bytes
            if (framedData.Length < 3)
            {
                return framedData;
            }

            if (framedData[0] == 0x02 && framedData[framedData.Length - 1] == 0x03)
            {
                int dataLength = framedData[1];
                byte[] data = new byte[dataLength];
                Array.Copy(framedData, 2, data, 0, Math.Min(dataLength, framedData.Length - 3));
                return data;
            }

            return framedData;
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
