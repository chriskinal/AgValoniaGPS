using AgValoniaGPS.Models.Communication;

namespace AgValoniaGPS.Services.Communication.Transports
{
    /// <summary>
    /// Interface for radio transport (LoRa, 900MHz, WiFi).
    /// Provides radio-specific configuration for long-range wireless communication.
    /// </summary>
    /// <remarks>
    /// Supports multiple radio types:
    /// - LoRa: 1+ mile range with configurable spread factor and bandwidth
    /// - 900MHz spread spectrum: Frequency hopping for interference resistance
    /// - WiFi: UDP bridge mode for WiFi-based communication
    ///
    /// LoRa configured for 1-mile range with appropriate transmit power (17-20 dBm).
    /// Radio modules typically interface via UART or SPI.
    /// </remarks>
    public interface IRadioTransportService : ITransport
    {
        /// <summary>
        /// Gets or sets the radio module type (LoRa, 900MHz, WiFi).
        /// </summary>
        RadioType RadioType { get; set; }

        /// <summary>
        /// Gets or sets the transmit power in dBm.
        /// For LoRa: 17-20 dBm for ~1 mile range.
        /// For 900MHz: 10-15 dBm typical.
        /// </summary>
        int TransmitPower { get; set; }

        /// <summary>
        /// Gets or sets the radio frequency in Hz.
        /// Examples:
        /// - LoRa US: 915000000 (915 MHz ISM band)
        /// - LoRa EU: 868000000 (868 MHz ISM band)
        /// - 900MHz: 902000000-928000000 (902-928 MHz)
        /// - WiFi: 2412000000 (2.4 GHz channel 1)
        /// </summary>
        int Frequency { get; set; }
    }
}
