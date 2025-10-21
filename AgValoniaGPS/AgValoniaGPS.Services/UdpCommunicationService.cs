using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AgValoniaGPS.Models;
using AgValoniaGPS.Services.Interfaces;

namespace AgValoniaGPS.Services;

/// <summary>
/// UDP communication service for Teensy modules
/// Eliminates unreliable USB serial connections
/// Port 9999 for module communication
/// </summary>
public class UdpCommunicationService : IUdpCommunicationService, IDisposable
{
    public event EventHandler<UdpDataReceivedEventArgs>? DataReceived;
    public event EventHandler<ModuleConnectionEventArgs>? ModuleConnectionChanged;

    private Socket? _udpSocket;
    private readonly byte[] _receiveBuffer = new byte[1024];
    private EndPoint _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isDisposed;

    // Module broadcast endpoint (e.g., 192.168.5.255:8888)
    private IPEndPoint? _moduleEndpoint;

    // Hello packet: [0x80, 0x81, 0x7F, 200, 3, 56, 0, 0, CRC]
    private readonly byte[] _helloPacket = { 0x80, 0x81, 0x7F, 200, 3, 56, 0, 0, 0x47 };

    // Module connection tracking - Hello responses (2 second timeout)
    private DateTime _lastHelloFromAutoSteer = DateTime.MinValue;
    private DateTime _lastHelloFromMachine = DateTime.MinValue;
    private DateTime _lastHelloFromIMU = DateTime.MinValue;

    // Module data tracking - Data flow (50ms for 50Hz modules, 250ms for 10Hz GPS/IMU)
    private DateTime _lastDataFromAutoSteer = DateTime.MinValue;
    private DateTime _lastDataFromMachine = DateTime.MinValue;
    private DateTime _lastDataFromIMU = DateTime.MinValue;

    private const int HELLO_TIMEOUT_MS = 2000; // 2 seconds for hello response
    private const int DATA_TIMEOUT_STEER_MACHINE_MS = 100; // 50Hz data = 20ms cycle, allow 100ms
    private const int DATA_TIMEOUT_IMU_MS = 300; // 10Hz data = 100ms cycle, allow 300ms

    public bool IsConnected { get; private set; }
    public string? LocalIPAddress { get; private set; }

    public async Task StartAsync()
    {
        if (IsConnected) return;

        try
        {
            // Get local IP address (prioritizes 192.168.5.x AgIO board subnet)
            LocalIPAddress = GetLocalIPAddress();

            // Log the selected IP for debugging
            System.Diagnostics.Debug.WriteLine($"[UDP] Selected IP address: {LocalIPAddress}");

            // Create UDP socket on port 9999
            _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            _udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // Reduce receive buffer to minimize packet buffering/delay
            _udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 8192);

            _udpSocket.Bind(new IPEndPoint(IPAddress.Any, 9999));

            // Set up module broadcast endpoint (default: 192.168.5.255:8888)
            _moduleEndpoint = new IPEndPoint(IPAddress.Parse("192.168.5.255"), 8888);

            IsConnected = true;

            // Start receiving
            _cancellationTokenSource = new CancellationTokenSource();
            _ = Task.Run(() => ReceiveLoop(_cancellationTokenSource.Token));

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            IsConnected = false;
            throw new Exception($"Failed to start UDP communication: {ex.Message}", ex);
        }
    }

    public async Task StopAsync()
    {
        if (!IsConnected) return;

        _cancellationTokenSource?.Cancel();
        _udpSocket?.Close();
        _udpSocket?.Dispose();
        _udpSocket = null;
        IsConnected = false;

        await Task.CompletedTask;
    }

    public void SendToModules(byte[] data)
    {
        if (!IsConnected || _udpSocket == null || _moduleEndpoint == null) return;

        try
        {
            _udpSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, _moduleEndpoint,
                ar =>
                {
                    try
                    {
                        _udpSocket?.EndSendTo(ar);
                    }
                    catch { }
                }, null);
        }
        catch { }
    }

    public void SendHelloPacket()
    {
        SendToModules(_helloPacket);
    }

    public bool IsModuleHelloOk(ModuleType moduleType)
    {
        var now = DateTime.Now;
        var timeout = TimeSpan.FromMilliseconds(HELLO_TIMEOUT_MS);

        return moduleType switch
        {
            ModuleType.AutoSteer => (now - _lastHelloFromAutoSteer) < timeout,
            ModuleType.Machine => (now - _lastHelloFromMachine) < timeout,
            ModuleType.IMU => (now - _lastHelloFromIMU) < timeout,
            _ => false
        };
    }

    public bool IsModuleDataOk(ModuleType moduleType)
    {
        var now = DateTime.Now;

        return moduleType switch
        {
            ModuleType.AutoSteer => (now - _lastDataFromAutoSteer).TotalMilliseconds < DATA_TIMEOUT_STEER_MACHINE_MS,
            ModuleType.Machine => (now - _lastDataFromMachine).TotalMilliseconds < DATA_TIMEOUT_STEER_MACHINE_MS,
            ModuleType.IMU => (now - _lastDataFromIMU).TotalMilliseconds < DATA_TIMEOUT_IMU_MS,
            _ => false
        };
    }

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
            catch { }
        }

        // Keep the task alive until cancellation
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(100, cancellationToken);
        }
    }

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

                ProcessReceivedData(data, (IPEndPoint)_remoteEndPoint);
            }
        }
        catch { }

        // IMMEDIATELY start the next receive operation - this is the key fix!
        if (_udpSocket != null)
        {
            try
            {
                _udpSocket.BeginReceiveFrom(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None,
                    ref _remoteEndPoint, ReceiveCallback, null);
            }
            catch { }
        }
    }

    private void ProcessReceivedData(byte[] data, IPEndPoint remoteEndPoint)
    {
        // Check if this is a binary PGN message or text NMEA sentence
        if (data.Length >= 2 && data[0] == PgnMessage.HEADER1 && data[1] == PgnMessage.HEADER2)
        {
            // Binary PGN message
            if (data.Length < 6) return;

            byte pgn = data[3];

            // Track module connections based on hello messages
            UpdateModuleConnection(pgn);

            // Fire event
            DataReceived?.Invoke(this, new UdpDataReceivedEventArgs
            {
                Data = data,
                RemoteEndPoint = remoteEndPoint,
                PGN = pgn,
                Timestamp = DateTime.Now
            });
        }
        else if (data.Length > 0 && data[0] == (byte)'$')
        {
            // Text NMEA sentence (starts with $)
            // Fire event with PGN 0 to indicate NMEA text
            DataReceived?.Invoke(this, new UdpDataReceivedEventArgs
            {
                Data = data,
                RemoteEndPoint = remoteEndPoint,
                PGN = 0, // Special PGN for NMEA text
                Timestamp = DateTime.Now
            });
        }
    }

    private void UpdateModuleConnection(byte pgn)
    {
        var now = DateTime.Now;

        // Track ALL PGNs as data - if we're getting any PGN from a module, it's sending data
        switch (pgn)
        {
            // AutoSteer PGNs
            case PgnNumbers.HELLO_FROM_AUTOSTEER: // 126
                _lastHelloFromAutoSteer = now;
                System.Diagnostics.Debug.WriteLine($"AutoSteer HELLO received at {now:HH:mm:ss.fff}");
                break;

            case PgnNumbers.AUTOSTEER_CONFIG:     // 250 - Regular data
            case PgnNumbers.AUTOSTEER_DATA:       // 253 - Regular data
            case PgnNumbers.AUTOSTEER_DATA2:      // 254
            case PgnNumbers.STEER_SETTINGS:       // 252
            case PgnNumbers.STEER_CONFIG:         // 251
                _lastDataFromAutoSteer = now;
                break;

            // Machine PGNs (receive-only, only Hello matters)
            case PgnNumbers.HELLO_FROM_MACHINE:  // 123
                _lastHelloFromMachine = now;
                System.Diagnostics.Debug.WriteLine($"Machine HELLO received at {now:HH:mm:ss.fff}");
                break;

            // IMU PGNs (only Hello matters - data only sent when active)
            case PgnNumbers.HELLO_FROM_IMU: // 121
                _lastHelloFromIMU = now;
                System.Diagnostics.Debug.WriteLine($"IMU HELLO received at {now:HH:mm:ss.fff}");
                break;

            default:
                // Log unknown PGNs to help debug
                System.Diagnostics.Debug.WriteLine($"Unknown PGN {pgn} (0x{pgn:X2}) received");
                break;
        }
    }

    private string? GetLocalIPAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            // Log all available IPs for debugging
            System.Diagnostics.Debug.WriteLine($"[UDP] Available network interfaces:");
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    System.Diagnostics.Debug.WriteLine($"[UDP]   - {ip}");
                }
            }

            // Priority 1: Look for IP in the 192.168.5.x subnet (AgIO board subnet)
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    var ipStr = ip.ToString();
                    if (ipStr.StartsWith("192.168.5."))
                    {
                        System.Diagnostics.Debug.WriteLine($"[UDP] Selected 192.168.5.x (AgIO board): {ipStr}");
                        return ipStr;
                    }
                }
            }

            // Priority 2: Look for any valid private IP (skip link-local and loopback)
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    var ipStr = ip.ToString();

                    // Skip loopback (127.x.x.x)
                    if (ipStr.StartsWith("127."))
                        continue;

                    // Skip link-local (169.254.x.x) - this is what was causing your issue!
                    if (ipStr.StartsWith("169.254."))
                        continue;

                    // Accept any other private IP (192.168.x.x, 10.x.x.x, 172.16-31.x.x)
                    if (ipStr.StartsWith("192.168.") ||
                        ipStr.StartsWith("10.") ||
                        (ipStr.StartsWith("172.") &&
                         int.TryParse(ipStr.Split('.')[1], out var second) &&
                         second >= 16 && second <= 31))
                    {
                        System.Diagnostics.Debug.WriteLine($"[UDP] Selected private IP: {ipStr}");
                        return ipStr;
                    }
                }
            }

            // Priority 3: Fall back to any IPv4 (if only public IPs available)
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    var ipStr = ip.ToString();
                    if (!ipStr.StartsWith("127.") && !ipStr.StartsWith("169.254."))
                    {
                        return ipStr;
                    }
                }
            }
        }
        catch { }

        return null;
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        StopAsync().Wait();
        _cancellationTokenSource?.Dispose();
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
}