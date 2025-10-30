using System;
using System.Threading.Tasks;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.FieldOperations;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Service for loading and saving elevation data in Elevation.txt format
/// Compatible with legacy AgOpenGPS format
/// </summary>
public interface IElevationFileService
{
    /// <summary>
    /// Loads elevation data from Elevation.txt file in the field directory
    /// </summary>
    /// <param name="fieldDirectory">Path to field directory</param>
    /// <returns>Loaded elevation grid, or empty grid if file doesn't exist</returns>
    Task<ElevationGrid> LoadElevationDataAsync(string fieldDirectory);

    /// <summary>
    /// Saves elevation data to Elevation.txt file in the field directory
    /// </summary>
    /// <param name="fieldDirectory">Path to field directory</param>
    /// <param name="grid">Elevation grid to save</param>
    /// <param name="startFix">Start position for field origin</param>
    Task SaveElevationDataAsync(string fieldDirectory, ElevationGrid grid, Position? startFix = null);

    /// <summary>
    /// Validates the format of an Elevation.txt file
    /// </summary>
    /// <param name="fieldDirectory">Path to field directory</param>
    /// <returns>True if file is valid, false otherwise</returns>
    Task<bool> ValidateElevationFileAsync(string fieldDirectory);

    /// <summary>
    /// Exports elevation data to a custom format
    /// </summary>
    /// <param name="filePath">Path to export file</param>
    /// <param name="grid">Elevation grid to export</param>
    Task ExportElevationDataAsync(string filePath, ElevationGrid grid);

    /// <summary>
    /// Imports elevation data from a custom format
    /// </summary>
    /// <param name="filePath">Path to import file</param>
    /// <returns>Imported elevation grid</returns>
    Task<ElevationGrid> ImportElevationDataAsync(string filePath);
}
