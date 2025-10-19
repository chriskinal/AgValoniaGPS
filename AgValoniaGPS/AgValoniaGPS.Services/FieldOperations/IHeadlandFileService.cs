using AgValoniaGPS.Models;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Service for reading and writing headland files in multiple formats
/// </summary>
public interface IHeadlandFileService
{
    /// <summary>
    /// Load headlands from AgOpenGPS Headland.txt format
    /// </summary>
    /// <param name="fieldDirectory">Directory containing headland file</param>
    /// <returns>Array of headland passes, or null if file doesn't exist</returns>
    Position[][]? LoadHeadlandsAgOpenGPS(string fieldDirectory);

    /// <summary>
    /// Save headlands to AgOpenGPS Headland.txt format
    /// </summary>
    /// <param name="headlands">Array of headland passes to save</param>
    /// <param name="fieldDirectory">Directory to save headland file to</param>
    void SaveHeadlandsAgOpenGPS(Position[][] headlands, string fieldDirectory);

    /// <summary>
    /// Load headlands from GeoJSON format
    /// </summary>
    /// <param name="filePath">Full path to GeoJSON file</param>
    /// <returns>Array of headland passes, or null if file doesn't exist</returns>
    Position[][]? LoadHeadlandsGeoJSON(string filePath);

    /// <summary>
    /// Save headlands to GeoJSON format
    /// </summary>
    /// <param name="headlands">Array of headland passes to save</param>
    /// <param name="filePath">Full path to save GeoJSON file to</param>
    void SaveHeadlandsGeoJSON(Position[][] headlands, string filePath);

    /// <summary>
    /// Load headlands from KML format
    /// </summary>
    /// <param name="filePath">Full path to KML file</param>
    /// <returns>Array of headland passes, or null if file doesn't exist</returns>
    Position[][]? LoadHeadlandsKML(string filePath);

    /// <summary>
    /// Save headlands to KML format
    /// </summary>
    /// <param name="headlands">Array of headland passes to save</param>
    /// <param name="filePath">Full path to save KML file to</param>
    void SaveHeadlandsKML(Position[][] headlands, string filePath);
}
