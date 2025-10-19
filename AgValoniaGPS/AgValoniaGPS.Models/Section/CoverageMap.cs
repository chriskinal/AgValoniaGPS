namespace AgValoniaGPS.Models.Section;

/// <summary>
/// Represents the complete coverage map with triangles and overlap tracking
/// </summary>
public class CoverageMap
{
    /// <summary>
    /// List of all coverage triangles
    /// </summary>
    public List<CoverageTriangle> Triangles { get; set; }

    /// <summary>
    /// Total covered area in square meters
    /// </summary>
    public double TotalCoveredArea { get; set; }

    /// <summary>
    /// Dictionary tracking overlap areas by overlap count
    /// Key: overlap count (1, 2, 3+), Value: area in square meters
    /// </summary>
    public Dictionary<int, double> OverlapAreas { get; set; }

    /// <summary>
    /// Creates a new empty coverage map
    /// </summary>
    public CoverageMap()
    {
        Triangles = new List<CoverageTriangle>();
        TotalCoveredArea = 0.0;
        OverlapAreas = new Dictionary<int, double>();
    }

    /// <summary>
    /// Adds a triangle to the coverage map and updates statistics
    /// </summary>
    /// <param name="triangle">Triangle to add</param>
    public void AddTriangle(CoverageTriangle triangle)
    {
        if (triangle == null)
            throw new ArgumentNullException(nameof(triangle));

        Triangles.Add(triangle);
        RecalculateStatistics();
    }

    /// <summary>
    /// Adds multiple triangles to the coverage map
    /// </summary>
    /// <param name="triangles">Triangles to add</param>
    public void AddTriangles(IEnumerable<CoverageTriangle> triangles)
    {
        if (triangles == null)
            throw new ArgumentNullException(nameof(triangles));

        Triangles.AddRange(triangles);
        RecalculateStatistics();
    }

    /// <summary>
    /// Recalculates total covered area and overlap statistics
    /// </summary>
    public void RecalculateStatistics()
    {
        TotalCoveredArea = 0.0;
        OverlapAreas.Clear();

        foreach (var triangle in Triangles)
        {
            double area = triangle.CalculateArea();
            TotalCoveredArea += area;

            int overlapKey = Math.Min(triangle.OverlapCount, 3); // Cap at 3+ for statistics
            if (!OverlapAreas.ContainsKey(overlapKey))
                OverlapAreas[overlapKey] = 0.0;

            OverlapAreas[overlapKey] += area;
        }
    }

    /// <summary>
    /// Clears all coverage data
    /// </summary>
    public void Clear()
    {
        Triangles.Clear();
        TotalCoveredArea = 0.0;
        OverlapAreas.Clear();
    }
}
