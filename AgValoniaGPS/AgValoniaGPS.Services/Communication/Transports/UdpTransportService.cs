using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Communication;

namespace AgValoniaGPS.Services.Communication.Transports
{
    /// <summary>
    /// UDP transport implementation for hardware module communication.
    /// Provides broadcast-based UDP communication on module-specific ports.
    /// </summary>
    /// <remarks>
    /// Refactored from UdpCommunicationService to fit ITransport abstraction pattern.
    /// Supports multi-module port configuration:
    /// - AutoSteer: Port 8888
    /// - Machine: Port 9999
    /// - IMU: Port 7777
    /// Connection state becomes "connected" once socket is bound and receiving.
    /// Thread-safe for concurrent send/receive operations.
    /// </remarks>
    public class UdpTransportService : IUdpTransportService, IDisposable
    {
        /// <inheritdoc />
        public event EventHandler<byte[]>? DataReceived;

        /// <inheritdoc />
        public event EventHandler<bool>? ConnectionChanged;

        private Socket? _udpSocket;
        private readonly byte[] _receiveBuffer = new byte[1024];
        private EndPoint _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isDisposed;
        private bool _isConnected;

        private readonly ModuleType _moduleType;
        private IPEndPoint? _broadcastEndpoint;

        /// <inheritdoc />
        public TransportType Type => TransportType.UDP;

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
        public int LocalPort { get; private set; }

        /// <inheritdoc />
        public string BroadcastAddress { get; private set; } = "255.255.255.255";

        /// <summary>
        /// Initializes a new instance of the UdpTransportService class.
        /// </summary>
        /// <param name="moduleType">The module type to determine port configuration</param>
        public UdpTransportService(ModuleType moduleType)
        {
            _moduleType = moduleType;
            LocalPort = GetPortForModule(moduleType);
        }

        /// <summary>
        /// Gets the UDP port for a specific module type.
        /// </summary>
        /// <param name="moduleType">The module type</param>
        /// <returns>Port number for the module</returns>
        private static int GetPortForModule(ModuleType moduleType)
        {
            return moduleType switch
            {
                ModuleType.AutoSteer => 8888,
                ModuleType.Machine => 9999,
                ModuleType.IMU => 7777,
                _ => throw new ArgumentException($"Unknown module type: {moduleType}", nameof(moduleType))
            };
        }

        /// <inheritdoc />
        public async Task StartAsync()
        {
            if (IsConnected)
            {
                throw new InvalidOperationException("Transport is already started");
            }

            try
            {
                // Create UDP socket
                _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                _udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                // Reduce receive buffer to minimize packet buffering/delay
                _udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 8192);

                // Bind to module-specific port
                _udpSocket.Bind(new IPEndPoint(IPAddress.Any, LocalPort));

                // Set up broadcast endpoint
                _broadcastEndpoint = new IPEndPoint(IPAddress.Parse(BroadcastAddress), LocalPort);

                // Update connection state
                IsConnected = true;

                // Start receiving
                _cancellationTokenSource = new CancellationTokenSource();
                _ = Task.Run(() => ReceiveLoop(_cancellationTokenSource.Token));

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                throw new System.IO.IOException($"Failed to start UDP transport on port {LocalPort}: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public async Task StopAsync()
        {
            if (!IsConnected) return;

            try
            {
                _cancellationTokenSource?.Cancel();
                _udpSocket?.Close();
                _udpSocket?.Dispose();
                _udpSocket = null;
                IsConnected = false;
            }
            catch
            {
                // Suppress exceptions during cleanup
            }

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Send(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (!IsConnected || _udpSocket == null || _broadcastEndpoint == null)
            {
                throw new InvalidOperationException("Transport is not connected");
            }

            try
            {
                _udpSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, _broadcastEndpoint,
                    ar =>
                    {
                        try
                        {
                            _udpSocket?.EndSendTo(ar);
                        }
                        catch
                        {
                            // Suppress send errors
                        }
                    }, null);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send data: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Async receive loop that listens for incoming UDP data.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to stop the loop</param>
        private async Task ReceiveLoop(CancellationToken cancellationToken)
        {
            // Start the first receive operation
            if (_udpSocket != null)
            {
                try
                {
                    _udpSocket.BeginReceiveFrom(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None,
                        ref _remoteEndPoint, ReceiveCallback, null);
                }
                catch
                {
                    // Suppress initial receive errors
                }
            }

            // Keep the task alive until cancellation
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100, cancellationToken);
            }
        }

        /// <summary>
        /// Callback for UDP receive operations.
        /// Immediately starts the next receive to minimize latency.
        /// </summary>
        /// <param name="ar">Async result from receive operation</param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            if (_udpSocket == null) return;

            try
            {
                int bytesReceived = _udpSocket.EndReceiveFrom(ar, ref _remoteEndPoint);

                if (bytesReceived > 0)
                {
                    byte[] data = new byte[bytesReceived];
                    Array.Copy(_receiveBuffer, data, bytesReceived);

                    // Raise DataReceived event
                    DataReceived?.Invoke(this, data);
                }
            }
            catch
            {
                // Suppress receive errors
            }

            // Immediately start the next receive operation - minimizes latency
            if (_udpSocket != null)
            {
                try
                {
                    _udpSocket.BeginReceiveFrom(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None,
                        ref _remoteEndPoint, ReceiveCallback, null);
                }
                catch
                {
                    // Suppress receive start errors
                }
            }
        }

        /// <summary>
        /// Disposes resources used by the transport.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            StopAsync().Wait();
            _cancellationTokenSource?.Dispose();
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
