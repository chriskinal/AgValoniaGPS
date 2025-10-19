using System.Collections.Generic;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Section;

namespace AgValoniaGPS.Services.Section;

/// <summary>
/// Service interface for reading and writing coverage map files
/// File format: Coverage.txt (text-based triangle data, one triangle per line)
/// </summary>
public interface ICoverageMapFileService
{
    /// <summary>
    /// Saves complete coverage map to Coverage.txt in the field directory
    /// </summary>
    /// <param name="fieldPath">Path to the field directory</param>
    /// <param name="triangles">Triangles to save</param>
    /// <returns>Task that completes when save is done</returns>
    Task SaveCoverageAsync(string fieldPath, IEnumerable<CoverageTriangle> triangles);

    /// <summary>
    /// Loads coverage map from Coverage.txt in the field directory
    /// </summary>
    /// <param name="fieldPath">Path to the field directory</param>
    /// <returns>List of coverage triangles, or empty list if file doesn't exist</returns>
    Task<List<CoverageTriangle>> LoadCoverageAsync(string fieldPath);

    /// <summary>
    /// Appends new coverage triangles to existing Coverage.txt file
    /// </summary>
    /// <param name="fieldPath">Path to the field directory</param>
    /// <param name="triangles">Triangles to append</param>
    /// <returns>Task that completes when append is done</returns>
    Task AppendCoverageAsync(string fieldPath, IEnumerable<CoverageTriangle> triangles);

    /// <summary>
    /// Synchronous version of SaveCoverage for compatibility
    /// </summary>
    void SaveCoverage(string fieldPath, IEnumerable<CoverageTriangle> triangles);

    /// <summary>
    /// Synchronous version of LoadCoverage for compatibility
    /// </summary>
    List<CoverageTriangle> LoadCoverage(string fieldPath);

    /// <summary>
    /// Synchronous version of AppendCoverage for compatibility
    /// </summary>
    void AppendCoverage(string fieldPath, IEnumerable<CoverageTriangle> triangles);
}
