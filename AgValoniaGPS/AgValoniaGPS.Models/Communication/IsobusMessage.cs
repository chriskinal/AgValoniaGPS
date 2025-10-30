using System;

namespace AgValoniaGPS.Models.Communication;

/// <summary>
/// Represents an ISO 11783 (ISOBUS) message for agricultural equipment communication.
/// ISOBUS uses CAN 2.0B extended frames (29-bit identifier) with priority-based arbitration.
/// </summary>
/// <remarks>
/// Message format follows PGN (Parameter Group Number) structure:
/// [0x80, 0x81, source, PGN, length, ...data..., CRC]
///
/// Standard compliance: ISO 11783-7 (Implement Messages)
/// Section control PG: 0xEF00 (61184)
/// </remarks>
public class IsobusMessage
{
    /// <summary>
    /// Type of ISOBUS message
    /// </summary>
    public IsobusMessageType MessageType { get; set; }

    /// <summary>
    /// Source address (0x7F for AgOpenGPS/AgIO)
    /// </summary>
    public byte SourceAddress { get; set; } = 0x7F;

    /// <summary>
    /// Destination address (0xFF for broadcast)
    /// </summary>
    public byte DestinationAddress { get; set; } = 0xFF;

    /// <summary>
    /// Message priority (0-7, lower is higher priority)
    /// </summary>
    public byte Priority { get; set; } = 3;

    /// <summary>
    /// Message data payload
    /// </summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Timestamp when message was created or received
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a new ISOBUS message
    /// </summary>
    public IsobusMessage()
    {
    }

    /// <summary>
    /// Creates a new ISOBUS message with specified type and data
    /// </summary>
    public IsobusMessage(IsobusMessageType messageType, byte[] data)
    {
        MessageType = messageType;
        Data = data ?? Array.Empty<byte>();
    }
}
