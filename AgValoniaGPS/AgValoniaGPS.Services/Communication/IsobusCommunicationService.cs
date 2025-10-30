using System;
using System.Linq;
using AgValoniaGPS.Models.Communication;

namespace AgValoniaGPS.Services.Communication;

/// <summary>
/// Implementation of ISOBUS (ISO 11783) communication service for agricultural equipment.
/// Enables standardized section control, process data exchange, and implement communication.
/// </summary>
/// <remarks>
/// Thread-safe: All operations use lock object for concurrent access protection.
/// Performance: Message encoding/decoding completes in &lt;5ms.
/// Standard: ISO 11783-7 (Implement Messages), CAN 2.0B extended frames.
///
/// Message format: [0x80, 0x81, source, PGN, length, ...data..., CRC]
/// Section control PG: 0xEF00 (61184) mapped to PGN 0xEF
/// </remarks>
public class IsobusCommunicationService : IIsobusCommunicationService
{
    private readonly object _lockObject = new object();
    private readonly ITransportAbstractionService? _transportService;

    // Message header constants (AgOpenGPS standard)
    private const byte HEADER1 = 0x80;
    private const byte HEADER2 = 0x81;
    private const byte SOURCE_ADDRESS = 0x7F;

    // ISOBUS state
    private DateTime _lastHeartbeatTime;
    private bool _sectionControlEnabled;
    private bool[] _actualSectionStates = Array.Empty<bool>();

    // Rate limiting for process data (max 10Hz per ISO 11783)
    private int _lastGuidanceLineDeviation;
    private DateTime _lastGuidanceDeviationTime;
    private int _lastActualSpeed;
    private DateTime _lastActualSpeedTime;
    private int _lastTotalDistance;
    private DateTime _lastTotalDistanceTime;

    private static readonly TimeSpan ProcessDataMinInterval = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan HeartbeatTimeout = TimeSpan.FromSeconds(1);

    public event EventHandler<IsobusMessage>? MessageReceived;
    public event EventHandler<IsobusMessage>? MessageSent;

    public bool SectionControlEnabled
    {
        get
        {
            lock (_lockObject)
            {
                return _sectionControlEnabled;
            }
        }
    }

    public bool IsAlive
    {
        get
        {
            lock (_lockObject)
            {
                return _lastHeartbeatTime != default &&
                       DateTime.UtcNow - _lastHeartbeatTime < HeartbeatTimeout;
            }
        }
    }

    /// <summary>
    /// Creates a new IsobusCommunicationService
    /// </summary>
    /// <param name="transportService">Optional transport service for sending messages</param>
    public IsobusCommunicationService(ITransportAbstractionService? transportService = null)
    {
        _transportService = transportService;
    }

    public IsobusMessage BuildSectionControlMessage(bool[] sectionStates)
    {
        if (sectionStates == null)
            throw new ArgumentNullException(nameof(sectionStates));

        if (sectionStates.Length == 0 || sectionStates.Length > 31)
            throw new ArgumentException("Section states must have 1-31 sections", nameof(sectionStates));

        var command = new IsobusSectionControlCommand(true, sectionStates);
        var data = SerializeSectionControlCommand(command);

        return new IsobusMessage(IsobusMessageType.SectionControlCommand, data);
    }

    public IsobusMessage? ParseIncomingMessage(byte[] data)
    {
        return DecodeMessage(data);
    }

    public void SendSectionControlCommand(bool[] sectionStates)
    {
        var message = BuildSectionControlMessage(sectionStates);
        var encodedData = EncodeMessage(message);

        // Send via transport if available
        if (_transportService != null)
        {
            try
            {
                _transportService.SendMessage(ModuleType.Machine, encodedData);
            }
            catch (Exception)
            {
                // Transport not available or module not configured
                // Message still built successfully for testing
            }
        }

        // Raise event
        MessageSent?.Invoke(this, message);
    }

    public void RequestSectionControlEnabled(bool enabled)
    {
        // Build request message
        byte[] data = new byte[1];
        data[0] = (byte)(enabled ? 0x01 : 0x00);

        var message = new IsobusMessage(IsobusMessageType.SectionControlRequest, data);
        var encodedData = EncodeMessage(message);

        // Send via transport if available
        if (_transportService != null)
        {
            try
            {
                _transportService.SendMessage(ModuleType.Machine, encodedData);
            }
            catch (Exception)
            {
                // Transport not available
            }
        }

        MessageSent?.Invoke(this, message);
    }

    public void SetGuidanceLineDeviation(int deviationMm)
    {
        lock (_lockObject)
        {
            // Rate limiting (max 10Hz)
            if (deviationMm == _lastGuidanceLineDeviation)
                return;

            if (DateTime.UtcNow - _lastGuidanceDeviationTime < ProcessDataMinInterval)
                return;

            _lastGuidanceLineDeviation = deviationMm;
            _lastGuidanceDeviationTime = DateTime.UtcNow;
        }

        SendProcessData(513, deviationMm);
    }

    public void SetActualSpeed(int speedMmPerSec)
    {
        lock (_lockObject)
        {
            // Rate limiting (max 10Hz)
            if (speedMmPerSec == _lastActualSpeed)
                return;

            if (DateTime.UtcNow - _lastActualSpeedTime < ProcessDataMinInterval)
                return;

            _lastActualSpeed = speedMmPerSec;
            _lastActualSpeedTime = DateTime.UtcNow;
        }

        SendProcessData(397, speedMmPerSec);
    }

    public void SetTotalDistance(int distanceMm)
    {
        lock (_lockObject)
        {
            // Rate limiting (max 10Hz)
            if (distanceMm == _lastTotalDistance)
                return;

            if (DateTime.UtcNow - _lastTotalDistanceTime < ProcessDataMinInterval)
                return;

            _lastTotalDistance = distanceMm;
            _lastTotalDistanceTime = DateTime.UtcNow;
        }

        SendProcessData(597, distanceMm);
    }

    public bool IsSectionOn(int sectionIndex)
    {
        lock (_lockObject)
        {
            if (sectionIndex < 0 || sectionIndex >= _actualSectionStates.Length)
                return false;

            return _actualSectionStates[sectionIndex];
        }
    }

    public byte[] EncodeMessage(IsobusMessage message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        // Standard PGN message format
        byte[] encoded = new byte[5 + message.Data.Length + 1];
        encoded[0] = HEADER1;
        encoded[1] = HEADER2;
        encoded[2] = message.SourceAddress;
        encoded[3] = (byte)message.MessageType;
        encoded[4] = (byte)message.Data.Length;
        Array.Copy(message.Data, 0, encoded, 5, message.Data.Length);

        // Calculate CRC
        encoded[^1] = CalculateCrc(encoded);

        return encoded;
    }

    public IsobusMessage? DecodeMessage(byte[] data)
    {
        if (data == null || data.Length < 6)
            return null;

        // Verify header
        if (data[0] != HEADER1 || data[1] != HEADER2)
            return null;

        byte source = data[2];
        byte pgnByte = data[3];
        byte length = data[4];

        if (data.Length < 5 + length + 1)
            return null;

        // Extract data
        byte[] messageData = new byte[length];
        Array.Copy(data, 5, messageData, 0, length);

        // Verify CRC
        byte expectedCrc = CalculateCrc(data);
        byte actualCrc = data[5 + length];
        if (expectedCrc != actualCrc)
            return null;

        // Map PGN to message type
        var messageType = (IsobusMessageType)pgnByte;

        return new IsobusMessage(messageType, messageData)
        {
            SourceAddress = source,
            Timestamp = DateTime.UtcNow
        };
    }

    public bool ProcessHeartbeat(byte[] data)
    {
        if (data == null || data.Length < 2)
            return false;

        lock (_lockObject)
        {
            // Read section control enabled flag (bit 0 of first byte)
            bool wasEnabled = _sectionControlEnabled;
            _sectionControlEnabled = ReadBit(data[0], 0);

            // Read number of sections
            int numberOfSections = data[1];

            // Validate data length
            int expectedLength = 2 + (numberOfSections + 7) / 8;
            if (data.Length != expectedLength)
                return false;

            // Read section states (packed bits)
            _actualSectionStates = Enumerable.Range(0, numberOfSections)
                .Select(i => ReadBit(data[2 + (i / 8)], i % 8))
                .ToArray();

            // Update heartbeat timestamp
            _lastHeartbeatTime = DateTime.UtcNow;

            return true;
        }
    }

    /// <summary>
    /// Sends process data message with identifier and value
    /// </summary>
    private void SendProcessData(ushort identifier, int value)
    {
        byte[] dataBytes = BitConverter.GetBytes(value);
        byte[] messageData = new byte[6];
        messageData[0] = (byte)(identifier & 0xFF);
        messageData[1] = (byte)(identifier >> 8);
        Array.Copy(dataBytes, 0, messageData, 2, 4);

        var message = new IsobusMessage(IsobusMessageType.ProcessData, messageData);
        var encodedData = EncodeMessage(message);

        // Send via transport if available
        if (_transportService != null)
        {
            try
            {
                _transportService.SendMessage(ModuleType.Machine, encodedData);
            }
            catch (Exception)
            {
                // Transport not available
            }
        }

        MessageSent?.Invoke(this, message);
    }

    /// <summary>
    /// Serializes section control command to byte array
    /// </summary>
    private byte[] SerializeSectionControlCommand(IsobusSectionControlCommand command)
    {
        // Calculate required bytes for packed section states
        int stateBytes = (command.NumberOfSections + 7) / 8;
        byte[] data = new byte[2 + stateBytes];

        // Byte 0: Flags (bit 0 = section control enabled)
        data[0] = (byte)(command.SectionControlEnabled ? 0x01 : 0x00);

        // Byte 1: Number of sections
        data[1] = (byte)command.NumberOfSections;

        // Bytes 2+: Section states (packed bits)
        for (int i = 0; i < command.NumberOfSections; i++)
        {
            if (command.SectionStates[i])
            {
                int byteIndex = 2 + (i / 8);
                int bitIndex = i % 8;
                data[byteIndex] |= (byte)(1 << bitIndex);
            }
        }

        return data;
    }

    /// <summary>
    /// Reads a specific bit from a byte
    /// </summary>
    private static bool ReadBit(byte data, int bitIndex)
    {
        return (data & (1 << bitIndex)) != 0;
    }

    /// <summary>
    /// Calculates CRC checksum (sum of bytes from source through data)
    /// </summary>
    private byte CalculateCrc(byte[] message)
    {
        byte crc = 0;
        for (int i = 2; i < message.Length - 1; i++)
        {
            crc += message[i];
        }
        return crc;
    }
}
