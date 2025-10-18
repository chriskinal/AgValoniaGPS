using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AgValoniaGPS.Models;
using AgValoniaGPS.Services.Interfaces;

namespace AgValoniaGPS.Services;

/// <summary>
/// NTRIP client for receiving RTK correction data from base station
/// Forwards RTCM3 corrections to GPS module via UDP port 2233
/// Based on AgIO NTRIP implementation
/// </summary>
public class NtripClientService : INtripClientService, IDisposable
{
    public event EventHandler<NtripConnectionEventArgs>? ConnectionStatusChanged;
    public event EventHandler<RtcmDataReceivedEventArgs>? RtcmDataReceived;

    private Socket? _tcpSocket;
    private Socket? _udpSocket;
    private readonly byte[] _receiveBuffer = new byte[4096];
    private readonly List<byte> _headerBuffer = new List<byte>();
    private bool _headerDumped = false;
    private CancellationTokenSource? _cancellationTokenSource;
    private NtripConfiguration? _config;
    private bool _isDisposed;

    private IPEndPoint? _rtcmUdpEndpoint;
    private Timer? _ggaTimer;
    private Timer? _rtcmForwardTimer;
    private readonly Queue<byte> _rtcmQueue = new Queue<byte>();
    private readonly object _queueLock = new object();
    private const int RTCM_PACKET_SIZE = 256; // Match AgIO default
    private readonly IGpsService _gpsService;

    public bool IsConnected { get; private set; }
    public ulong TotalBytesReceived { get; private set; }

    public NtripClientService(IGpsService gpsService)
    {
        _gpsService = gpsService;
    }

    public async Task ConnectAsync(NtripConfiguration config)
    {
        if (IsConnected)
        {
            await DisconnectAsync();
        }

        _config = config;

        try
        {
            // Create UDP socket for forwarding RTCM data to GPS module (port 2233)
            _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            _udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // Set up RTCM forward endpoint (subnet.255:2233)
            _rtcmUdpEndpoint = new IPEndPoint(
                IPAddress.Parse($"{config.SubnetAddress}.255"),
                config.UdpForwardPort);

            // Create TCP socket for NTRIP caster connection
            _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _tcpSocket.NoDelay = true;

            // Resolve hostname if needed
            IPAddress? casterIP;
            if (!IPAddress.TryParse(config.CasterAddress, out casterIP))
            {
                var addresses = await Dns.GetHostAddressesAsync(config.CasterAddress);
                casterIP = addresses.Length > 0 ? addresses[0] : throw new Exception("Could not resolve hostname");
            }

            // Connect to NTRIP caster
            await _tcpSocket.ConnectAsync(new IPEndPoint(casterIP, config.CasterPort));

            // Clear header buffer from any previous connection
            _headerBuffer.Clear();
            _headerDumped = false;

            // Send NTRIP request
            await SendNtripRequestAsync();

            // Start receiving RTCM data
            _cancellationTokenSource = new CancellationTokenSource();
            _ = Task.Run(() => ReceiveLoop(_cancellationTokenSource.Token));

            // Start GGA timer if interval > 0
            if (config.GgaIntervalSeconds > 0)
            {
                _ggaTimer = new Timer(
                    GgaTimerCallback,
                    null,
                    TimeSpan.FromSeconds(5), // First GGA after 5 seconds
                    TimeSpan.FromSeconds(config.GgaIntervalSeconds));
            }

            // Start RTCM forward timer (50ms interval like AgIO)
            _rtcmForwardTimer = new Timer(
                RtcmForwardTimerCallback,
                null,
                TimeSpan.FromMilliseconds(50),
                TimeSpan.FromMilliseconds(50));

            IsConnected = true;
            TotalBytesReceived = 0;

            ConnectionStatusChanged?.Invoke(this, new NtripConnectionEventArgs
            {
                IsConnected = true,
                Message = $"Connected to {config.CasterAddress}:{config.CasterPort}/{config.MountPoint}"
            });
        }
        catch (Exception ex)
        {
            IsConnected = false;
            ConnectionStatusChanged?.Invoke(this, new NtripConnectionEventArgs
            {
                IsConnected = false,
                Message = $"Connection failed: {ex.Message}"
            });
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        if (!IsConnected) return;

        _ggaTimer?.Dispose();
        _ggaTimer = null;

        _rtcmForwardTimer?.Dispose();
        _rtcmForwardTimer = null;

        _cancellationTokenSource?.Cancel();

        _tcpSocket?.Close();
        _tcpSocket?.Dispose();
        _tcpSocket = null;

        _udpSocket?.Close();
        _udpSocket?.Dispose();
        _udpSocket = null;

        IsConnected = false;

        ConnectionStatusChanged?.Invoke(this, new NtripConnectionEventArgs
        {
            IsConnected = false,
            Message = "Disconnected"
        });

        await Task.CompletedTask;
    }

    private async Task SendNtripRequestAsync()
    {
        if (_tcpSocket == null || _config == null) return;

        // Build NTRIP request (HTTP GET with Basic Auth)
        var credentials = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{_config.Username}:{_config.Password}"));

        var request = new StringBuilder();
        request.AppendLine($"GET /{_config.MountPoint} HTTP/{_config.HttpVersion}");
        request.AppendLine($"Host: {_config.CasterAddress}:{_config.CasterPort}");
        request.AppendLine($"Authorization: Basic {credentials}");
        request.AppendLine("User-Agent: NTRIP AgValoniaGPS/1.0");
        request.AppendLine("Accept: */*");
        request.AppendLine("Connection: close");
        request.AppendLine();

        byte[] requestBytes = Encoding.ASCII.GetBytes(request.ToString());
        await _tcpSocket.SendAsync(requestBytes, SocketFlags.None);
    }

    private async Task ReceiveLoop(CancellationToken cancellationToken)
    {
        bool headerReceived = false;

        while (!cancellationToken.IsCancellationRequested && _tcpSocket != null)
        {
            try
            {
                int bytesReceived = await _tcpSocket.ReceiveAsync(
                    new ArraySegment<byte>(_receiveBuffer),
                    SocketFlags.None,
                    cancellationToken);

                if (bytesReceived > 0)
                {
                    // First response is HTTP header - check for success
                    if (!headerReceived)
                    {
                        // Accumulate header bytes
                        for (int i = 0; i < bytesReceived; i++)
                        {
                            _headerBuffer.Add(_receiveBuffer[i]);
                        }

                        Console.WriteLine($"NTRIP: Accumulating header bytes, buffer size: {_headerBuffer.Count}");

                        // Dump first 200 bytes once to see actual header
                        if (!_headerDumped && _headerBuffer.Count >= 200)
                        {
                            _headerDumped = true;
                            int dumpSize = Math.Min(200, _headerBuffer.Count);
                            string headerPreview = Encoding.ASCII.GetString(_headerBuffer.Take(dumpSize).ToArray());
                            Console.WriteLine($"NTRIP: First {dumpSize} bytes:\n{headerPreview}\n---END---");
                        }

                        // Find header/body boundary
                        // ICY protocol uses single \r\n, HTTP uses \r\n\r\n
                        int headerEnd = -1;
                        int dataStart = -1;

                        // First check for ICY single line response (just \r\n)
                        for (int i = 0; i < _headerBuffer.Count - 1; i++)
                        {
                            if (_headerBuffer[i] == '\r' && _headerBuffer[i + 1] == '\n')
                            {
                                // Check if this looks like ICY response
                                if (i < 50)
                                {
                                    string testHeader = Encoding.ASCII.GetString(_headerBuffer.ToArray(), 0, i);
                                    if (testHeader.StartsWith("ICY 200"))
                                    {
                                        headerEnd = i;
                                        dataStart = i + 2; // After \r\n
                                        Console.WriteLine("NTRIP: ICY protocol detected");
                                        break;
                                    }
                                }

                                // Check for HTTP \r\n\r\n
                                if (i + 3 < _headerBuffer.Count &&
                                    _headerBuffer[i + 2] == '\r' && _headerBuffer[i + 3] == '\n')
                                {
                                    headerEnd = i;
                                    dataStart = i + 4; // After \r\n\r\n
                                    break;
                                }
                            }
                        }

                        if (headerEnd >= 0)
                        {
                            // Parse header as ASCII string
                            string response = Encoding.ASCII.GetString(_headerBuffer.ToArray(), 0, headerEnd);

                            if (response.Contains("200 OK") || response.Contains("ICY 200"))
                            {
                                headerReceived = true;
                                Console.WriteLine("NTRIP: Header found, setting headerReceived=true, authorized");

                                // Forward any RTCM data after header
                                if (dataStart < _headerBuffer.Count)
                                {
                                    int rtcmBytes = _headerBuffer.Count - dataStart;
                                    byte[] rtcmData = new byte[rtcmBytes];
                                    _headerBuffer.CopyTo(dataStart, rtcmData, 0, rtcmBytes);
                                    Console.WriteLine($"NTRIP: Forwarding {rtcmBytes} bytes from header packet");
                                    ForwardRtcmData(rtcmData);
                                }

                                // Clear header buffer
                                _headerBuffer.Clear();
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"NTRIP: Authorization failed - {response}");
                                await DisconnectAsync();
                                return;
                            }
                        }
                        // If no complete header yet, accumulate more data
                    }
                    else
                    {
                        // All subsequent data is RTCM3 corrections - forward as raw bytes
                        Console.WriteLine($"NTRIP: Forwarding {bytesReceived} bytes of RTCM data");
                        byte[] rtcmData = new byte[bytesReceived];
                        Array.Copy(_receiveBuffer, rtcmData, bytesReceived);
                        ForwardRtcmData(rtcmData);
                    }
                }
                else
                {
                    // Connection closed by server
                    System.Diagnostics.Debug.WriteLine("NTRIP: Connection closed by caster");
                    await DisconnectAsync();
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NTRIP receive error: {ex.Message}");
                await DisconnectAsync();
                break;
            }
        }
    }

    private void ForwardRtcmData(byte[] rtcmData)
    {
        if (rtcmData.Length == 0)
            return;

        // Enqueue received RTCM bytes for timer-based forwarding (like AgIO)
        lock (_queueLock)
        {
            foreach (byte b in rtcmData)
            {
                _rtcmQueue.Enqueue(b);
            }
        }

        TotalBytesReceived += (ulong)rtcmData.Length;
    }

    private void RtcmForwardTimerCallback(object? state)
    {
        if (!IsConnected || _udpSocket == null || _rtcmUdpEndpoint == null)
            return;

        lock (_queueLock)
        {
            if (_rtcmQueue.Count == 0)
                return;

            // Limit to packet size (256 bytes like AgIO)
            int count = Math.Min(_rtcmQueue.Count, RTCM_PACKET_SIZE);
            byte[] packet = new byte[count];

            for (int i = 0; i < count; i++)
            {
                packet[i] = _rtcmQueue.Dequeue();
            }

            try
            {
                // Forward RTCM3 corrections to GPS module via UDP broadcast
                _udpSocket.SendTo(packet, _rtcmUdpEndpoint);

                RtcmDataReceived?.Invoke(this, new RtcmDataReceivedEventArgs
                {
                    BytesReceived = packet.Length
                });

                Console.WriteLine($"NTRIP: Forwarded {packet.Length} bytes to port 2233, queue remaining: {_rtcmQueue.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"NTRIP: Failed to forward RTCM data: {ex.Message}");
            }

            // Clear queue if it gets too large (like AgIO does at 10000 bytes)
            if (_rtcmQueue.Count > 10000)
            {
                Console.WriteLine($"NTRIP: Queue overflow, clearing {_rtcmQueue.Count} bytes");
                _rtcmQueue.Clear();
            }
        }
    }

    private void GgaTimerCallback(object? state)
    {
        if (!IsConnected || _config == null) return;

        try
        {
            string ggaSentence;

            if (_config.UseManualPosition)
            {
                // Use manual position
                ggaSentence = GenerateGgaSentence(
                    _config.ManualLatitude,
                    _config.ManualLongitude,
                    0, // altitude
                    4, // fix quality (RTK fixed)
                    12); // satellites
            }
            else
            {
                // Use GPS position from GpsService
                var gpsData = _gpsService.CurrentData;
                if (gpsData != null && gpsData.IsValid)
                {
                    ggaSentence = GenerateGgaSentence(
                        gpsData.Latitude,
                        gpsData.Longitude,
                        gpsData.Altitude,
                        gpsData.FixQuality,
                        gpsData.SatellitesInUse);
                }
                else
                {
                    // No GPS data available yet - send default position (center of US)
                    // This allows caster to start sending corrections
                    ggaSentence = GenerateGgaSentence(
                        39.8283, // Latitude (Kansas, US)
                        -98.5795, // Longitude
                        0, // altitude
                        1, // fix quality (GPS fix)
                        8); // satellites
                    Console.WriteLine("NTRIP: No GPS data, sending default position");
                }
            }

            _ = SendGgaSentenceAsync(ggaSentence);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"NTRIP: GGA timer error: {ex.Message}");
        }
    }

    public async Task SendGgaSentenceAsync(string ggaSentence)
    {
        if (!IsConnected || _tcpSocket == null) return;

        try
        {
            byte[] ggaBytes = Encoding.ASCII.GetBytes(ggaSentence + "\r\n");
            await _tcpSocket.SendAsync(ggaBytes, SocketFlags.None);

            Console.WriteLine($"NTRIP: Sent GGA - {ggaSentence}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NTRIP: Failed to send GGA: {ex.Message}");
        }
    }

    private string GenerateGgaSentence(double lat, double lon, double alt, int fixQuality, int sats)
    {
        // Convert decimal degrees to NMEA format (DDMM.MMMM)
        double latDeg = Math.Abs(lat);
        int latDegrees = (int)latDeg;
        double latMinutes = (latDeg - latDegrees) * 60.0;
        string latStr = $"{latDegrees:00}{latMinutes:00.0000}";
        string latDir = lat >= 0 ? "N" : "S";

        double lonDeg = Math.Abs(lon);
        int lonDegrees = (int)lonDeg;
        double lonMinutes = (lonDeg - lonDegrees) * 60.0;
        string lonStr = $"{lonDegrees:000}{lonMinutes:00.0000}";
        string lonDir = lon >= 0 ? "E" : "W";

        // Get UTC time
        DateTime utc = DateTime.UtcNow;
        string timeStr = utc.ToString("HHmmss.ff", CultureInfo.InvariantCulture);

        // Build GGA sentence (without checksum yet)
        string gga = $"GPGGA,{timeStr},{latStr},{latDir},{lonStr},{lonDir},{fixQuality},{sats:00},1.0,{alt:F1},M,0.0,M,,";

        // Calculate checksum (XOR of all characters between $ and *)
        byte checksum = 0;
        foreach (char c in gga)
        {
            checksum ^= (byte)c;
        }

        return $"${gga}*{checksum:X2}";
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        DisconnectAsync().Wait();
        _cancellationTokenSource?.Dispose();
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
}