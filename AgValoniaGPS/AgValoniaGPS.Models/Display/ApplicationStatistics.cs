namespace AgValoniaGPS.Models.Display;

/// <summary>
/// Represents application statistics for field operations including area coverage,
/// application rates, and work efficiency metrics.
/// </summary>
public class ApplicationStatistics
{
    /// <summary>
    /// Gets or sets the total area covered during field operations, measured in square meters (m²).
    /// </summary>
    public double TotalAreaCovered { get; set; }

    /// <summary>
    /// Gets or sets the target application rate for the operation.
    /// The unit depends on the operation context (e.g., volume per area for spraying).
    /// </summary>
    public double ApplicationRateTarget { get; set; }

    /// <summary>
    /// Gets or sets the actual application rate achieved during the operation.
    /// The unit matches ApplicationRateTarget for comparison purposes.
    /// </summary>
    public double ActualApplicationRate { get; set; }

    /// <summary>
    /// Gets or sets the coverage percentage, representing the efficiency of application.
    /// Value ranges from 0 to 100+, where 100% indicates perfect coverage and values
    /// above 100% indicate over-application.
    /// </summary>
    public double CoveragePercentage { get; set; }

    /// <summary>
    /// Gets or sets the work rate, representing area covered per hour.
    /// Measured in the appropriate unit (hectares/hour or acres/hour) based on unit system.
    /// </summary>
    public double WorkRate { get; set; }
}
