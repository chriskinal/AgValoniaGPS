namespace AgValoniaGPS.Models.FieldOperations;

/// <summary>
/// Result of checking a turn path against field boundaries.
/// Indicates whether the turn is valid and provides collision details.
/// </summary>
public class TurnBoundaryCheck
{
    /// <summary>
    /// Creates a boundary check result.
    /// </summary>
    /// <param name="isValid">Whether the turn path is valid (no collisions)</param>
    public TurnBoundaryCheck(bool isValid)
    {
        IsValid = isValid;
        ViolationPoints = new List<Position2D>();
    }

    /// <summary>
    /// Whether the turn path is valid (does not violate boundaries).
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Closest distance to any boundary along the turn path.
    /// Null if no boundary proximity was calculated.
    /// </summary>
    public double? MinBoundaryDistance { get; set; }

    /// <summary>
    /// Points along the turn path that violate boundary constraints.
    /// Empty list if IsValid is true.
    /// </summary>
    public List<Position2D> ViolationPoints { get; set; }

    /// <summary>
    /// Index of the first waypoint that violates boundaries.
    /// Null if IsValid is true.
    /// </summary>
    public int? FirstViolationIndex { get; set; }

    /// <summary>
    /// Human-readable description of the boundary violation.
    /// Null if IsValid is true.
    /// </summary>
    public string? ViolationReason { get; set; }

    /// <summary>
    /// Creates a valid boundary check result.
    /// </summary>
    /// <param name="minBoundaryDistance">Closest approach to boundary</param>
    /// <returns>Valid check result</returns>
    public static TurnBoundaryCheck Valid(double minBoundaryDistance)
    {
        return new TurnBoundaryCheck(true)
        {
            MinBoundaryDistance = minBoundaryDistance
        };
    }

    /// <summary>
    /// Creates an invalid boundary check result with violation details.
    /// </summary>
    /// <param name="violationReason">Description of the violation</param>
    /// <param name="firstViolationIndex">Index of first violating waypoint</param>
    /// <param name="violationPoints">Points that violate boundaries</param>
    /// <returns>Invalid check result</returns>
    public static TurnBoundaryCheck Invalid(
        string violationReason,
        int? firstViolationIndex = null,
        List<Position2D>? violationPoints = null)
    {
        return new TurnBoundaryCheck(false)
        {
            ViolationReason = violationReason,
            FirstViolationIndex = firstViolationIndex,
            ViolationPoints = violationPoints ?? new List<Position2D>()
        };
    }
}
