using System.Threading.Tasks;
using AgValoniaGPS.Models.Communication;

namespace AgValoniaGPS.Services.Communication.Transports
{
    /// <summary>
    /// Interface for Bluetooth transport (SPP and BLE modes).
    /// Provides Bluetooth-specific configuration and device discovery.
    /// </summary>
    /// <remarks>
    /// Supports both Bluetooth Classic (SPP) and Bluetooth Low Energy (BLE).
    /// Platform-specific implementations handle Windows (WinRT), Linux (BlueZ), and macOS (IOBluetooth).
    /// Graceful degradation on platforms without Bluetooth support.
    /// </remarks>
    public interface IBluetoothTransportService : ITransport
    {
        /// <summary>
        /// Gets or sets the Bluetooth communication mode (SPP or BLE).
        /// </summary>
        BluetoothMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the target Bluetooth device address (MAC address format: XX:XX:XX:XX:XX:XX).
        /// </summary>
        string? DeviceAddress { get; set; }

        /// <summary>
        /// Scans for available Bluetooth devices.
        /// </summary>
        /// <returns>Array of discovered device addresses</returns>
        /// <exception cref="System.NotSupportedException">Thrown if Bluetooth is not supported on this platform</exception>
        Task<string[]> ScanDevicesAsync();
    }
}
