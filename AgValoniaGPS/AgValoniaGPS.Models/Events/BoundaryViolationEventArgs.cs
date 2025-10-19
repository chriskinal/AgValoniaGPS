namespace AgValoniaGPS.Models.Events;

/// <summary>
/// Event arguments for boundary violation detection
/// </summary>
public class BoundaryViolationEventArgs : EventArgs
{
    /// <summary>
    /// Position that violated the boundary
    /// </summary>
    public readonly Position ViolatingPosition;

    /// <summary>
    /// Distance from the boundary in meters (negative means inside, positive means outside)
    /// </summary>
    public readonly double DistanceFromBoundary;

    /// <summary>
    /// When the violation occurred
    /// </summary>
    public readonly DateTime Timestamp;

    /// <summary>
    /// Creates a new instance of BoundaryViolationEventArgs
    /// </summary>
    /// <param name="violatingPosition">Position that violated the boundary</param>
    /// <param name="distanceFromBoundary">Distance from boundary in meters</param>
    public BoundaryViolationEventArgs(Position violatingPosition, double distanceFromBoundary)
    {
        ViolatingPosition = violatingPosition ?? throw new ArgumentNullException(nameof(violatingPosition));
        DistanceFromBoundary = distanceFromBoundary;
        Timestamp = DateTime.UtcNow;
    }
}
