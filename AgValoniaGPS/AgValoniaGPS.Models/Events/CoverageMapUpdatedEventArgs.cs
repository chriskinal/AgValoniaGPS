namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for coverage map updates
/// </summary>
public class CoverageMapUpdatedEventArgs : EventArgs
{
    /// <summary>
    /// Number of triangles added in this update
    /// </summary>
    public readonly int AddedTrianglesCount;

    /// <summary>
    /// Total covered area in square meters after update
    /// </summary>
    public readonly double TotalCoveredArea;

    /// <summary>
    /// When the update occurred
    /// </summary>
    public readonly DateTime Timestamp;

    /// <summary>
    /// Creates a new instance of CoverageMapUpdatedEventArgs
    /// </summary>
    /// <param name="addedTrianglesCount">Number of triangles added</param>
    /// <param name="totalCoveredArea">Total covered area</param>
    public CoverageMapUpdatedEventArgs(int addedTrianglesCount, double totalCoveredArea)
    {
        if (addedTrianglesCount < 0)
            throw new ArgumentOutOfRangeException(nameof(addedTrianglesCount), "Added triangles count must be non-negative");

        if (totalCoveredArea < 0)
            throw new ArgumentOutOfRangeException(nameof(totalCoveredArea), "Total covered area must be non-negative");

        AddedTrianglesCount = addedTrianglesCount;
        TotalCoveredArea = totalCoveredArea;
        Timestamp = DateTime.UtcNow;
    }
}
