using AgValoniaGPS.Models;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Provides file I/O services for boundary data in multiple formats
/// </summary>
/// <remarks>
/// Supports three file formats:
/// - AgOpenGPS Boundary.txt (legacy format with Easting,Northing per line)
/// - GeoJSON (standard geographic format)
/// - KML (Google Earth format)
/// </remarks>
public interface IBoundaryFileService
{
    /// <summary>
    /// Loads a boundary from AgOpenGPS Boundary.txt format
    /// </summary>
    /// <param name="filePath">Path to the boundary file</param>
    /// <returns>Array of positions, or empty array if file not found or invalid</returns>
    /// <remarks>
    /// Format: Each line contains "Easting,Northing" in UTM coordinates
    /// First and last points are automatically connected to close the polygon
    /// </remarks>
    Position[] LoadFromAgOpenGPS(string filePath);

    /// <summary>
    /// Saves a boundary to AgOpenGPS Boundary.txt format
    /// </summary>
    /// <param name="boundary">Boundary positions to save</param>
    /// <param name="filePath">Path to save the file</param>
    /// <remarks>
    /// Creates parent directories if they don't exist.
    /// Saves in simple format: "Easting,Northing" per line
    /// </remarks>
    void SaveToAgOpenGPS(Position[] boundary, string filePath);

    /// <summary>
    /// Loads a boundary from GeoJSON format
    /// </summary>
    /// <param name="filePath">Path to the GeoJSON file</param>
    /// <returns>Array of positions, or empty array if file not found or invalid</returns>
    /// <remarks>
    /// Expects a GeoJSON Feature with Polygon geometry.
    /// Coordinates should be in [longitude, latitude] format.
    /// Converts WGS84 coordinates to UTM for internal use.
    /// </remarks>
    Position[] LoadFromGeoJSON(string filePath);

    /// <summary>
    /// Saves a boundary to GeoJSON format
    /// </summary>
    /// <param name="boundary">Boundary positions to save</param>
    /// <param name="filePath">Path to save the file</param>
    /// <remarks>
    /// Saves as GeoJSON Feature with Polygon geometry.
    /// Converts internal UTM coordinates to WGS84 for standard compatibility.
    /// </remarks>
    void SaveToGeoJSON(Position[] boundary, string filePath);

    /// <summary>
    /// Loads a boundary from KML format
    /// </summary>
    /// <param name="filePath">Path to the KML file</param>
    /// <returns>Array of positions, or empty array if file not found or invalid</returns>
    /// <remarks>
    /// Expects a KML Placemark with Polygon geometry.
    /// Coordinates should be in "longitude,latitude,altitude" format.
    /// Converts WGS84 coordinates to UTM for internal use.
    /// </remarks>
    Position[] LoadFromKML(string filePath);

    /// <summary>
    /// Saves a boundary to KML format
    /// </summary>
    /// <param name="boundary">Boundary positions to save</param>
    /// <param name="filePath">Path to save the file</param>
    /// <remarks>
    /// Saves as KML Placemark with Polygon geometry.
    /// Converts internal UTM coordinates to WGS84 for standard compatibility.
    /// Compatible with Google Earth.
    /// </remarks>
    void SaveToKML(Position[] boundary, string filePath);
}
