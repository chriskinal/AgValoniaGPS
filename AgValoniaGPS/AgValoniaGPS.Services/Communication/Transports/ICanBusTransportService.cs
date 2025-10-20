namespace AgValoniaGPS.Services.Communication.Transports
{
    /// <summary>
    /// Interface for CAN bus / ISOBUS transport.
    /// Provides CAN-specific configuration and ISOBUS compliance.
    /// </summary>
    /// <remarks>
    /// Supports ISOBUS (ISO 11783) protocol for agricultural equipment communication.
    /// Uses USB CAN adapters (PCAN, SocketCAN) for cross-platform support.
    /// Standard ISOBUS baudrate is 250kbps.
    /// Handles multi-frame message reassembly (TP.CM, TP.DT).
    /// </remarks>
    public interface ICanBusTransportService : ITransport
    {
        /// <summary>
        /// Gets or sets the CAN adapter device path.
        /// Examples: "/dev/pcanusb0" (Linux), "COM3" (Windows PCAN).
        /// </summary>
        string? AdapterPath { get; set; }

        /// <summary>
        /// Gets or sets the CAN bus baudrate.
        /// Standard ISOBUS baudrate is 250000 (250kbps).
        /// </summary>
        int Baudrate { get; set; }
    }
}
