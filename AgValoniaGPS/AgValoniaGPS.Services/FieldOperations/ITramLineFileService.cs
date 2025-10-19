using AgValoniaGPS.Models;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Service for tram line file I/O operations
/// Supports AgOpenGPS TramLines.txt, GeoJSON, and KML formats
/// </summary>
public interface ITramLineFileService
{
    /// <summary>
    /// Save tram lines in AgOpenGPS format
    /// Format: $TramLine,{id} followed by easting,northing lines
    /// </summary>
    /// <param name="tramLines">Array of tram lines to save</param>
    /// <param name="filePath">Full path to TramLines.txt file</param>
    void SaveAgOpenGPSFormat(Position[][] tramLines, string filePath);

    /// <summary>
    /// Load tram lines from AgOpenGPS format
    /// </summary>
    /// <param name="filePath">Full path to TramLines.txt file</param>
    /// <returns>Array of tram lines, or null if file doesn't exist</returns>
    Position[][]? LoadAgOpenGPSFormat(string filePath);

    /// <summary>
    /// Export tram lines to GeoJSON format
    /// Format: MultiLineString feature collection
    /// </summary>
    /// <param name="tramLines">Array of tram lines to export</param>
    /// <param name="filePath">Full path to output .geojson file</param>
    void ExportGeoJSON(Position[][] tramLines, string filePath);

    /// <summary>
    /// Import tram lines from GeoJSON format
    /// </summary>
    /// <param name="filePath">Full path to input .geojson file</param>
    /// <returns>Array of tram lines</returns>
    Position[][] ImportGeoJSON(string filePath);

    /// <summary>
    /// Export tram lines to KML format
    /// Format: Multiple LineString placemarks
    /// </summary>
    /// <param name="tramLines">Array of tram lines to export</param>
    /// <param name="filePath">Full path to output .kml file</param>
    void ExportKML(Position[][] tramLines, string filePath);

    /// <summary>
    /// Import tram lines from KML format
    /// </summary>
    /// <param name="filePath">Full path to input .kml file</param>
    /// <returns>Array of tram lines</returns>
    Position[][] ImportKML(string filePath);
}
