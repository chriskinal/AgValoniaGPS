namespace AgValoniaGPS.Models.Section;

/// <summary>
/// Represents a coverage triangle formed by vehicle movement
/// </summary>
public class CoverageTriangle
{
    /// <summary>
    /// The three vertices of the triangle
    /// </summary>
    public Position[] Vertices { get; set; }

    /// <summary>
    /// Section ID that created this triangle (0-based)
    /// </summary>
    public int SectionId { get; set; }

    /// <summary>
    /// Timestamp when the triangle was created
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Number of times this area has been covered (1 = first pass, 2+ = overlap)
    /// </summary>
    public int OverlapCount { get; set; }

    /// <summary>
    /// Creates a new coverage triangle
    /// </summary>
    /// <param name="vertex1">First vertex</param>
    /// <param name="vertex2">Second vertex</param>
    /// <param name="vertex3">Third vertex</param>
    /// <param name="sectionId">Section ID</param>
    public CoverageTriangle(Position vertex1, Position vertex2, Position vertex3, int sectionId)
    {
        if (vertex1 == null) throw new ArgumentNullException(nameof(vertex1));
        if (vertex2 == null) throw new ArgumentNullException(nameof(vertex2));
        if (vertex3 == null) throw new ArgumentNullException(nameof(vertex3));

        Vertices = new Position[3] { vertex1, vertex2, vertex3 };
        SectionId = sectionId;
        Timestamp = DateTime.UtcNow;
        OverlapCount = 1;
    }

    /// <summary>
    /// Default constructor for deserialization
    /// </summary>
    public CoverageTriangle()
    {
        Vertices = new Position[3];
        SectionId = 0;
        Timestamp = DateTime.UtcNow;
        OverlapCount = 1;
    }

    /// <summary>
    /// Calculates the area of the triangle in square meters using the cross product method
    /// </summary>
    public double CalculateArea()
    {
        if (Vertices == null || Vertices.Length != 3)
            return 0.0;

        // Use UTM coordinates (Easting/Northing) for area calculation
        var v1 = Vertices[0];
        var v2 = Vertices[1];
        var v3 = Vertices[2];

        // Area = 0.5 * |x1(y2 - y3) + x2(y3 - y1) + x3(y1 - y2)|
        double area = 0.5 * Math.Abs(
            v1.Easting * (v2.Northing - v3.Northing) +
            v2.Easting * (v3.Northing - v1.Northing) +
            v3.Easting * (v1.Northing - v2.Northing)
        );

        return area;
    }
}
