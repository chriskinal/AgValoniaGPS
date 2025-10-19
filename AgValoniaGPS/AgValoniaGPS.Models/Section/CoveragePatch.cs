namespace AgValoniaGPS.Models.Section;

/// <summary>
/// Represents a rectangular patch for spatial indexing of coverage data
/// Used for efficient overlap detection and coverage queries
/// </summary>
public class CoveragePatch
{
    /// <summary>
    /// Minimum easting (left edge) in meters
    /// </summary>
    public double MinEasting { get; set; }

    /// <summary>
    /// Maximum easting (right edge) in meters
    /// </summary>
    public double MaxEasting { get; set; }

    /// <summary>
    /// Minimum northing (bottom edge) in meters
    /// </summary>
    public double MinNorthing { get; set; }

    /// <summary>
    /// Maximum northing (top edge) in meters
    /// </summary>
    public double MaxNorthing { get; set; }

    /// <summary>
    /// Triangles contained within this patch
    /// </summary>
    public List<CoverageTriangle> Triangles { get; set; }

    /// <summary>
    /// Creates a new coverage patch with specified bounds
    /// </summary>
    /// <param name="minEasting">Minimum easting</param>
    /// <param name="maxEasting">Maximum easting</param>
    /// <param name="minNorthing">Minimum northing</param>
    /// <param name="maxNorthing">Maximum northing</param>
    public CoveragePatch(double minEasting, double maxEasting, double minNorthing, double maxNorthing)
    {
        MinEasting = minEasting;
        MaxEasting = maxEasting;
        MinNorthing = minNorthing;
        MaxNorthing = maxNorthing;
        Triangles = new List<CoverageTriangle>();
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public CoveragePatch()
    {
        Triangles = new List<CoverageTriangle>();
    }

    /// <summary>
    /// Checks if a position falls within this patch
    /// </summary>
    /// <param name="easting">Easting coordinate</param>
    /// <param name="northing">Northing coordinate</param>
    /// <returns>True if position is within patch bounds</returns>
    public bool ContainsPoint(double easting, double northing)
    {
        return easting >= MinEasting && easting <= MaxEasting &&
               northing >= MinNorthing && northing <= MaxNorthing;
    }

    /// <summary>
    /// Checks if this patch intersects with another patch
    /// </summary>
    /// <param name="other">Other patch to check</param>
    /// <returns>True if patches intersect</returns>
    public bool Intersects(CoveragePatch other)
    {
        if (other == null)
            return false;

        return !(MaxEasting < other.MinEasting || MinEasting > other.MaxEasting ||
                 MaxNorthing < other.MinNorthing || MinNorthing > other.MaxNorthing);
    }
}
