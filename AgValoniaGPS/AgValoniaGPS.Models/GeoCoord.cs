namespace AgValoniaGPS.Models;

/// <summary>
/// Represents a geographic coordinate in UTM (Universal Transverse Mercator) system
/// </summary>
public class GeoCoord
{
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

    public GeoCoord()
    {
        Easting = 0.0;
        Northing = 0.0;
        Altitude = 0.0;
    }

    public GeoCoord(double easting, double northing, double altitude = 0.0)
    {
        Easting = easting;
        Northing = northing;
        Altitude = altitude;
    }
}
