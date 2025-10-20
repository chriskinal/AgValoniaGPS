using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Display;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Services;

/// <summary>
/// Field statistics and area calculation service interface.
/// Provides field operations statistics, rotating display data, and area calculations.
/// </summary>
public interface IFieldStatisticsService
{
    #region Existing Properties & Methods (Wave 5)

    /// <summary>
    /// Total area worked (sum of all section areas) in square meters
    /// </summary>
    double WorkedAreaSquareMeters { get; set; }

    /// <summary>
    /// User-accumulated distance in meters
    /// </summary>
    double UserDistance { get; set; }

    /// <summary>
    /// Boundary area (outer minus inner) in square meters
    /// </summary>
    double BoundaryAreaSquareMeters { get; }

    /// <summary>
    /// Actual area covered (worked area minus overlap) in square meters
    /// </summary>
    double ActualAreaCovered { get; }

    /// <summary>
    /// Overlap percentage
    /// </summary>
    double OverlapPercent { get; }

    /// <summary>
    /// Update boundary area from field boundary
    /// </summary>
    /// <param name="boundary">Field boundary to calculate area from</param>
    void UpdateBoundaryArea(Boundary? boundary);

    /// <summary>
    /// Calculate overlap statistics based on worked area
    /// </summary>
    void CalculateOverlap();

    /// <summary>
    /// Get remaining area to work in hectares
    /// </summary>
    /// <returns>Remaining area in hectares</returns>
    double GetRemainingAreaHectares();

    /// <summary>
    /// Get remaining area percentage
    /// </summary>
    /// <returns>Percentage of field remaining to work (0-100)</returns>
    double GetRemainingPercent();

    /// <summary>
    /// Calculate estimated time to finish field operations
    /// </summary>
    /// <param name="currentSpeed">Current speed in km/h</param>
    /// <param name="toolWidth">Tool width in meters</param>
    /// <returns>Estimated time remaining in minutes</returns>
    double GetEstimatedTimeToFinish(double currentSpeed, double toolWidth);

    /// <summary>
    /// Calculate current work rate in hectares per hour
    /// </summary>
    /// <param name="currentSpeed">Current speed in km/h</param>
    /// <param name="toolWidth">Tool width in meters</param>
    /// <returns>Work rate in hectares per hour</returns>
    double GetWorkRatePerHour(double currentSpeed, double toolWidth);

    /// <summary>
    /// Reset all statistics to zero
    /// </summary>
    void Reset();

    /// <summary>
    /// Format area for display in hectares or acres
    /// </summary>
    /// <param name="squareMeters">Area in square meters</param>
    /// <param name="useMetric">True for metric (hectares), false for imperial (acres)</param>
    /// <returns>Formatted area string with units</returns>
    string FormatArea(double squareMeters, bool useMetric = true);

    /// <summary>
    /// Format distance for display in meters or feet
    /// </summary>
    /// <param name="meters">Distance in meters</param>
    /// <param name="useMetric">True for metric (meters), false for imperial (feet)</param>
    /// <returns>Formatted distance string with units</returns>
    string FormatDistance(double meters, bool useMetric = true);

    #endregion

    #region Wave 7: Display & Visualization Methods

    /// <summary>
    /// Calculate application statistics including area coverage, application rates,
    /// coverage percentage, and work rate.
    /// </summary>
    /// <param name="currentSpeed">Current speed in km/h for work rate calculation</param>
    /// <param name="toolWidth">Tool width in meters for work rate calculation</param>
    /// <returns>ApplicationStatistics object with calculated values</returns>
    /// <remarks>
    /// Returns default values (zeros) if no valid data is available.
    /// Work rate is calculated using GetWorkRatePerHour method.
    /// Coverage percentage is calculated as (ActualAreaCovered / BoundaryAreaSquareMeters * 100).
    /// </remarks>
    ApplicationStatistics CalculateApplicationStatistics(double currentSpeed, double toolWidth);

    /// <summary>
    /// Get rotating display data for specified screen number.
    /// The rotating display cycles through 3 screens:
    /// - Screen 1: Application statistics (area, rates, efficiency)
    /// - Screen 2: Field name
    /// - Screen 3: Guidance line information
    /// </summary>
    /// <param name="screenNumber">Screen number (1, 2, or 3)</param>
    /// <param name="currentSpeed">Current speed in km/h for statistics calculation</param>
    /// <param name="toolWidth">Tool width in meters for statistics calculation</param>
    /// <returns>RotatingDisplayData with appropriate data populated for the screen</returns>
    /// <remarks>
    /// Screen 1: Populates AppStats using CalculateApplicationStatistics
    /// Screen 2: Populates FieldName using GetCurrentFieldName
    /// Screen 3: Populates GuidanceLineInfo using GetActiveGuidanceLineInfo
    /// </remarks>
    RotatingDisplayData GetRotatingDisplayData(int screenNumber, double currentSpeed, double toolWidth);

    /// <summary>
    /// Get the current field name for display.
    /// </summary>
    /// <returns>Field name string, or "No Field" if no field is selected</returns>
    /// <remarks>
    /// Returns a default field name if no field is currently selected or tracked.
    /// </remarks>
    string GetCurrentFieldName();

    /// <summary>
    /// Get active guidance line information including type and heading.
    /// </summary>
    /// <returns>Tuple containing GuidanceLineType and heading in degrees</returns>
    /// <remarks>
    /// Returns (GuidanceLineType.ABLine, 0.0) as default if no active guidance line.
    /// Heading is in degrees (0-360).
    /// </remarks>
    (GuidanceLineType Type, double Heading) GetActiveGuidanceLineInfo();

    #endregion
}
