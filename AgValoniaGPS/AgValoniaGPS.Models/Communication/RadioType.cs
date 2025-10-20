namespace AgValoniaGPS.Models.Communication;

/// <summary>
/// Specifies the type of radio module for wireless communication.
/// </summary>
public enum RadioType
{
    /// <summary>
    /// LoRa (Long Range) radio module for 1+ mile range
    /// </summary>
    LoRa,

    /// <summary>
    /// 900MHz spread spectrum radio module
    /// </summary>
    Spread900MHz,

    /// <summary>
    /// WiFi-based radio communication
    /// </summary>
    WiFi
}
