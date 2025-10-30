using AgValoniaGPS.Models.FieldOperations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Service for saving and loading recorded paths to/from disk.
/// File format: .rec files with comma-separated values
/// </summary>
public interface IRecordedPathFileService
{
    /// <summary>
    /// Saves a recorded path to a file.
    /// </summary>
    /// <param name="path">The path to save</param>
    /// <param name="fieldDirectory">Directory where field data is stored</param>
    /// <param name="fileName">Optional filename (defaults to "RecPath.txt")</param>
    /// <returns>True if save was successful</returns>
    Task<bool> SavePathAsync(RecordedPath path, string fieldDirectory, string? fileName = null);

    /// <summary>
    /// Loads a recorded path from a file.
    /// </summary>
    /// <param name="fieldDirectory">Directory where field data is stored</param>
    /// <param name="fileName">Optional filename (defaults to "RecPath.txt")</param>
    /// <returns>The loaded path, or null if file doesn't exist</returns>
    Task<RecordedPath?> LoadPathAsync(string fieldDirectory, string? fileName = null);

    /// <summary>
    /// Lists all recorded path files in a field directory.
    /// </summary>
    /// <param name="fieldDirectory">Directory to search</param>
    /// <returns>List of .rec file names (without extension)</returns>
    Task<List<string>> ListPathsAsync(string fieldDirectory);

    /// <summary>
    /// Deletes a recorded path file.
    /// </summary>
    /// <param name="fieldDirectory">Directory where field data is stored</param>
    /// <param name="pathName">Name of the path file (without extension)</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeletePathAsync(string fieldDirectory, string pathName);

    /// <summary>
    /// Checks if a recorded path file exists.
    /// </summary>
    /// <param name="fieldDirectory">Directory where field data is stored</param>
    /// <param name="fileName">Optional filename (defaults to "RecPath.txt")</param>
    /// <returns>True if the file exists</returns>
    bool PathExists(string fieldDirectory, string? fileName = null);
}
