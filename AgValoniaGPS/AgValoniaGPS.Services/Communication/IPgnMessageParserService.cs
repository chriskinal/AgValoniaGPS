using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Communication;
using ImuDataModel = AgValoniaGPS.Models.Communication.ImuData;

namespace AgValoniaGPS.Services.Communication;

/// <summary>
/// Service for parsing inbound PGN (Parameter Group Number) messages from hardware modules.
/// Validates CRC checksums and converts binary data to domain models.
/// </summary>
public interface IPgnMessageParserService
{
    /// <summary>
    /// Parse a generic PGN message from raw bytes.
    /// Returns basic PGN structure without type-specific parsing.
    /// </summary>
    /// <param name="data">Raw byte array containing PGN message</param>
    /// <returns>Parsed PgnMessage or null if invalid</returns>
    PgnMessage? ParseMessage(byte[] data);

    /// <summary>
    /// Parse AutoSteer configuration response (capability negotiation).
    /// </summary>
    /// <param name="data">Raw byte array</param>
    /// <returns>Parsed AutoSteerConfigResponse or null if invalid</returns>
    AutoSteerConfigResponse? ParseAutoSteerConfig(byte[] data);

    /// <summary>
    /// Parse AutoSteer feedback data (actual wheel angle, switch states).
    /// Typically PGN 253.
    /// </summary>
    /// <param name="data">Raw byte array</param>
    /// <returns>Parsed AutoSteerFeedback or null if invalid</returns>
    AutoSteerFeedback? ParseAutoSteerData(byte[] data);

    /// <summary>
    /// Parse Machine configuration response (max sections, zones).
    /// </summary>
    /// <param name="data">Raw byte array</param>
    /// <returns>Parsed MachineConfigResponse or null if invalid</returns>
    MachineConfigResponse? ParseMachineConfig(byte[] data);

    /// <summary>
    /// Parse Machine feedback data (work switch, section sensors).
    /// Typically PGN 234.
    /// </summary>
    /// <param name="data">Raw byte array</param>
    /// <returns>Parsed MachineFeedback or null if invalid</returns>
    MachineFeedback? ParseMachineData(byte[] data);

    /// <summary>
    /// Parse IMU data (roll, pitch, heading, yaw rate, calibration status).
    /// Typically PGN 219.
    /// </summary>
    /// <param name="data">Raw byte array</param>
    /// <returns>Parsed ImuData or null if invalid</returns>
    ImuDataModel? ParseImuData(byte[] data);

    /// <summary>
    /// Parse scan response from module discovery.
    /// </summary>
    /// <param name="data">Raw byte array</param>
    /// <returns>Parsed ScanResponse or null if invalid</returns>
    ScanResponse? ParseScanResponse(byte[] data);

    /// <summary>
    /// Parse hello packet from a module (PGN 126/123/121).
    /// Used for connection keepalive and module identification.
    /// </summary>
    /// <param name="data">Raw byte array</param>
    /// <returns>Parsed HelloResponse or null if invalid</returns>
    HelloResponse? ParseHelloPacket(byte[] data);

    /// <summary>
    /// Validate CRC checksum of a PGN message.
    /// </summary>
    /// <param name="data">Raw byte array</param>
    /// <returns>True if CRC is valid, false otherwise</returns>
    bool ValidateCrc(byte[] data);

    /// <summary>
    /// Calculate CRC checksum for a PGN message.
    /// Used for validation and debugging.
    /// </summary>
    /// <param name="data">Raw byte array</param>
    /// <returns>Calculated CRC byte</returns>
    byte CalculateCrc(byte[] data);
}
