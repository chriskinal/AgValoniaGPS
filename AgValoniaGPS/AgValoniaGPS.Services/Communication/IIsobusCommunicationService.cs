using System;
using AgValoniaGPS.Models.Communication;

namespace AgValoniaGPS.Services.Communication;

/// <summary>
/// Interface for ISOBUS (ISO 11783) communication service.
/// Enables section control and implement communication via standardized agricultural protocol.
/// </summary>
/// <remarks>
/// ISOBUS is the ISO 11783 standard for agricultural equipment communication.
/// Uses CAN 2.0B extended frames with priority-based message arbitration.
/// Section control PG: 0xEF00 (61184)
/// </remarks>
public interface IIsobusCommunicationService
{
    /// <summary>
    /// Event raised when an ISOBUS message is received
    /// </summary>
    event EventHandler<IsobusMessage> MessageReceived;

    /// <summary>
    /// Event raised when an ISOBUS message is sent
    /// </summary>
    event EventHandler<IsobusMessage> MessageSent;

    /// <summary>
    /// Gets whether section control is enabled on the ISOBUS device
    /// </summary>
    bool SectionControlEnabled { get; }

    /// <summary>
    /// Gets whether the ISOBUS device is alive (heartbeat within last second)
    /// </summary>
    bool IsAlive { get; }

    /// <summary>
    /// Builds an ISOBUS section control message from section states
    /// </summary>
    /// <param name="sectionStates">Array of section states (true = on, false = off)</param>
    /// <returns>ISOBUS message ready for transmission</returns>
    IsobusMessage BuildSectionControlMessage(bool[] sectionStates);

    /// <summary>
    /// Parses incoming ISOBUS message data
    /// </summary>
    /// <param name="data">Raw message bytes</param>
    /// <returns>Parsed ISOBUS message, or null if invalid</returns>
    IsobusMessage? ParseIncomingMessage(byte[] data);

    /// <summary>
    /// Sends a section control command to the ISOBUS device
    /// </summary>
    /// <param name="sectionStates">Array of section states (true = on, false = off)</param>
    void SendSectionControlCommand(bool[] sectionStates);

    /// <summary>
    /// Requests section control enabled/disabled state
    /// </summary>
    /// <param name="enabled">True to enable section control, false to disable</param>
    void RequestSectionControlEnabled(bool enabled);

    /// <summary>
    /// Sets the guidance line deviation (cross-track error)
    /// </summary>
    /// <param name="deviationMm">Deviation in millimeters</param>
    void SetGuidanceLineDeviation(int deviationMm);

    /// <summary>
    /// Sets the actual vehicle speed
    /// </summary>
    /// <param name="speedMmPerSec">Speed in mm/s</param>
    void SetActualSpeed(int speedMmPerSec);

    /// <summary>
    /// Sets the total distance traveled
    /// </summary>
    /// <param name="distanceMm">Distance in millimeters</param>
    void SetTotalDistance(int distanceMm);

    /// <summary>
    /// Gets the state of a specific section from ISOBUS device feedback
    /// </summary>
    /// <param name="sectionIndex">Zero-based section index</param>
    /// <returns>True if section is on, false if off</returns>
    bool IsSectionOn(int sectionIndex);

    /// <summary>
    /// Encodes an ISOBUS message to byte array for transmission
    /// </summary>
    /// <param name="message">ISOBUS message to encode</param>
    /// <returns>Encoded byte array</returns>
    byte[] EncodeMessage(IsobusMessage message);

    /// <summary>
    /// Decodes a byte array to an ISOBUS message
    /// </summary>
    /// <param name="data">Raw message bytes</param>
    /// <returns>Decoded ISOBUS message, or null if invalid</returns>
    IsobusMessage? DecodeMessage(byte[] data);

    /// <summary>
    /// Processes a heartbeat message from the ISOBUS device
    /// Updates section states and alive status
    /// </summary>
    /// <param name="data">Heartbeat message data</param>
    /// <returns>True if heartbeat was valid, false otherwise</returns>
    bool ProcessHeartbeat(byte[] data);
}
