using System;

namespace AgValoniaGPS.Models;

/// <summary>
/// Represents GPS data received from receiver
/// </summary>
public class GpsData
{
    /// <summary>
    /// Latitude in decimal degrees
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude in decimal degrees
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// UTM Easting coordinate in meters
    /// </summary>
    public double Easting { get; set; }

    /// <summary>
    /// UTM Northing coordinate in meters
    /// </summary>
    public double Northing { get; set; }

    /// <summary>
    /// Altitude in meters above sea level
    /// </summary>
    public double Altitude { get; set; }

    /// <summary>
    /// Speed in meters per second
    /// </summary>
    public double Speed { get; set; }

    /// <summary>
    /// Heading in degrees (0-360)
    /// </summary>
    public double Heading { get; set; }

    /// <summary>
    /// GPS fix quality (0=invalid, 1=GPS fix, 2=DGPS fix, 4=RTK fixed, 5=RTK float)
    /// </summary>
    public int FixQuality { get; set; }

    /// <summary>
    /// Number of satellites in use
    /// </summary>
    public int SatelliteCount { get; set; }

    /// <summary>
    /// Number of satellites in use (alias for compatibility)
    /// </summary>
    public int SatellitesInUse
    {
        get => SatelliteCount;
        set => SatelliteCount = value;
    }

    /// <summary>
    /// Horizontal dilution of precision
    /// </summary>
    public double Hdop { get; set; }

    /// <summary>
    /// Age of differential corrections in seconds
    /// </summary>
    public double DifferentialAge { get; set; }

    /// <summary>
    /// Timestamp when data was received
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Whether GPS data is currently valid
    /// </summary>
    public bool IsValid => FixQuality > 0 && SatelliteCount >= 4;
}