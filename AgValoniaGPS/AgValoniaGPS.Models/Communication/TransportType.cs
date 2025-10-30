namespace AgValoniaGPS.Models.Communication;

/// <summary>
/// Specifies the communication transport type for hardware modules.
/// </summary>
public enum TransportType
{
    /// <summary>
    /// UDP network communication (default)
    /// </summary>
    UDP,

    /// <summary>
    /// Bluetooth Classic SPP (Serial Port Profile)
    /// </summary>
    BluetoothSPP,

    /// <summary>
    /// Bluetooth Low Energy (BLE)
    /// </summary>
    BluetoothBLE,

    /// <summary>
    /// CAN bus communication
    /// </summary>
    CAN,

    /// <summary>
    /// ISOBUS (ISO 11783) agricultural equipment communication
    /// </summary>
    ISOBUS,

    /// <summary>
    /// Radio communication (LoRa, 900MHz, WiFi)
    /// </summary>
    Radio
}
