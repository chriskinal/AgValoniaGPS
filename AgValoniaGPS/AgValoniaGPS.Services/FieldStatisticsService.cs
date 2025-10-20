using System;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Display;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Services;

/// <summary>
/// Field statistics and area calculation service
/// Ported from AOG_Dev CFieldData.cs
/// Expanded in Wave 7 with rotating display and application statistics
/// </summary>
public class FieldStatisticsService : IFieldStatisticsService
{
    /// <summary>
    /// Total area worked (sum of all section areas) in square meters
    /// </summary>
    public double WorkedAreaSquareMeters { get; set; } = 0;

    /// <summary>
    /// User-accumulated distance in meters
    /// </summary>
    public double UserDistance { get; set; } = 0;

    /// <summary>
    /// Boundary area (outer minus inner) in square meters
    /// </summary>
    public double BoundaryAreaSquareMeters { get; private set; } = 0;

    /// <summary>
    /// Actual area covered (worked area minus overlap) in square meters
    /// </summary>
    public double ActualAreaCovered { get; private set; } = 0;

    /// <summary>
    /// Overlap percentage
    /// </summary>
    public double OverlapPercent { get; private set; } = 0;

    /// <summary>
    /// Update boundary area from field boundary
    /// </summary>
    public void UpdateBoundaryArea(Boundary? boundary)
    {
        if (boundary == null || !boundary.IsValid)
        {
            BoundaryAreaSquareMeters = 0;
            return;
        }

        // Outer boundary area
        double outerArea = boundary.OuterBoundary?.AreaSquareMeters ?? 0;

        // Subtract inner boundaries (holes)
        double innerArea = 0;
        foreach (var inner in boundary.InnerBoundaries)
        {
            innerArea += inner.AreaSquareMeters;
        }

        BoundaryAreaSquareMeters = outerArea - innerArea;
    }

    /// <summary>
    /// Calculate overlap statistics
    /// </summary>
    public void CalculateOverlap()
    {
        if (WorkedAreaSquareMeters > 0)
        {
            // This is simplified - actual implementation would need section data
            // to calculate actual vs theoretical coverage
            ActualAreaCovered = WorkedAreaSquareMeters * 0.95; // Placeholder
            OverlapPercent = ((WorkedAreaSquareMeters - ActualAreaCovered) / WorkedAreaSquareMeters) * 100;
        }
        else
        {
            ActualAreaCovered = 0;
            OverlapPercent = 0;
        }
    }

    /// <summary>
    /// Get remaining area to work in hectares
    /// </summary>
    public double GetRemainingAreaHectares()
    {
        double remaining = BoundaryAreaSquareMeters - WorkedAreaSquareMeters;
        return remaining / 10000.0; // Convert to hectares
    }

    /// <summary>
    /// Get remaining area percentage
    /// </summary>
    public double GetRemainingPercent()
    {
        if (BoundaryAreaSquareMeters > 10)
        {
            return ((BoundaryAreaSquareMeters - WorkedAreaSquareMeters) * 100 / BoundaryAreaSquareMeters);
        }
        return 0;
    }

    /// <summary>
    /// Calculate estimated time to finish
    /// Ported from AOG_Dev CFieldData.TimeTillFinished
    /// </summary>
    /// <param name="currentSpeed">Current speed in km/h</param>
    /// <param name="toolWidth">Tool width in meters</param>
    /// <returns>Estimated time in minutes</returns>
    public double GetEstimatedTimeToFinish(double currentSpeed, double toolWidth)
    {
        if (currentSpeed > 2)
        {
            // Remaining area (ha) / (tool width (m) * speed (km/h) * 0.1)
            double hoursRemaining = (BoundaryAreaSquareMeters - WorkedAreaSquareMeters) / 10000.0
                / (toolWidth * currentSpeed * 0.1);

            return hoursRemaining * 60; // Convert to minutes
        }

        return double.PositiveInfinity; // Infinite time if not moving
    }

    /// <summary>
    /// Calculate current work rate in hectares per hour
    /// Ported from AOG_Dev CFieldData.WorkRateHour
    /// </summary>
    /// <param name="currentSpeed">Current speed in km/h</param>
    /// <param name="toolWidth">Tool width in meters</param>
    /// <returns>Work rate in hectares per hour</returns>
    public double GetWorkRatePerHour(double currentSpeed, double toolWidth)
    {
        // Tool width (m) * speed (km/h) * 0.1 (km to ha conversion)
        return toolWidth * currentSpeed * 0.1;
    }

    /// <summary>
    /// Reset all statistics
    /// </summary>
    public void Reset()
    {
        WorkedAreaSquareMeters = 0;
        UserDistance = 0;
        ActualAreaCovered = 0;
        OverlapPercent = 0;
    }

    /// <summary>
    /// Format area for display in hectares or acres
    /// </summary>
    public string FormatArea(double squareMeters, bool useMetric = true)
    {
        if (useMetric)
        {
            return (squareMeters / 10000.0).ToString("F2") + " ha";
        }
        else
        {
            return (squareMeters / 4046.86).ToString("F2") + " ac";
        }
    }

    /// <summary>
    /// Format distance for display in meters or feet
    /// </summary>
    public string FormatDistance(double meters, bool useMetric = true)
    {
        if (useMetric)
        {
            return meters.ToString("F1") + " m";
        }
        else
        {
            return (meters * 3.28084).ToString("F1") + " ft";
        }
    }

    #region Wave 7: Display & Visualization Methods

    /// <summary>
    /// Calculate application statistics including area coverage, application rates,
    /// coverage percentage, and work rate.
    /// </summary>
    /// <param name="currentSpeed">Current speed in km/h for work rate calculation</param>
    /// <param name="toolWidth">Tool width in meters for work rate calculation</param>
    /// <returns>ApplicationStatistics object with calculated values</returns>
    public ApplicationStatistics CalculateApplicationStatistics(double currentSpeed, double toolWidth)
    {
        // Ensure overlap statistics are current
        CalculateOverlap();

        var stats = new ApplicationStatistics
        {
            TotalAreaCovered = WorkedAreaSquareMeters,
            ApplicationRateTarget = 0.0, // Default - would come from configuration
            ActualApplicationRate = 0.0, // Default - would come from section control data
            CoveragePercentage = CalculateCoveragePercentage(),
            WorkRate = GetWorkRatePerHour(currentSpeed, toolWidth)
        };

        return stats;
    }

    /// <summary>
    /// Get rotating display data for specified screen number.
    /// </summary>
    /// <param name="screenNumber">Screen number (1, 2, or 3)</param>
    /// <param name="currentSpeed">Current speed in km/h for statistics calculation</param>
    /// <param name="toolWidth">Tool width in meters for statistics calculation</param>
    /// <returns>RotatingDisplayData with appropriate data populated for the screen</returns>
    public RotatingDisplayData GetRotatingDisplayData(int screenNumber, double currentSpeed, double toolWidth)
    {
        var displayData = new RotatingDisplayData
        {
            CurrentScreen = screenNumber
        };

        switch (screenNumber)
        {
            case 1:
                // Screen 1: Application Statistics
                displayData.AppStats = CalculateApplicationStatistics(currentSpeed, toolWidth);
                break;

            case 2:
                // Screen 2: Field Name
                displayData.FieldName = GetCurrentFieldName();
                break;

            case 3:
                // Screen 3: Guidance Line Info
                var (lineType, heading) = GetActiveGuidanceLineInfo();
                displayData.GuidanceLineInfo = FormatGuidanceLineInfo(lineType, heading);
                break;

            default:
                // Default to screen 1 if invalid screen number
                displayData.CurrentScreen = 1;
                displayData.AppStats = CalculateApplicationStatistics(currentSpeed, toolWidth);
                break;
        }

        return displayData;
    }

    /// <summary>
    /// Get the current field name for display.
    /// </summary>
    /// <returns>Field name string, or "No Field" if no field is selected</returns>
    public string GetCurrentFieldName()
    {
        // TODO: This would eventually retrieve from a field management service
        // For now, return a default placeholder
        return "No Field";
    }

    /// <summary>
    /// Get active guidance line information including type and heading.
    /// </summary>
    /// <returns>Tuple containing GuidanceLineType and heading in degrees</returns>
    public (GuidanceLineType Type, double Heading) GetActiveGuidanceLineInfo()
    {
        // TODO: This would eventually retrieve from guidance line services
        // (IABLineService, ICurveLineService, IContourService)
        // For now, return default ABLine at 0 degrees
        return (GuidanceLineType.ABLine, 0.0);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Calculate coverage percentage based on actual area covered vs boundary area
    /// </summary>
    /// <returns>Coverage percentage (0-100)</returns>
    private double CalculateCoveragePercentage()
    {
        if (BoundaryAreaSquareMeters > 10)
        {
            // Calculate percentage of field that has been covered
            double percentage = (ActualAreaCovered / BoundaryAreaSquareMeters) * 100.0;

            // Clamp to 0-100 range
            return Math.Max(0, Math.Min(100, percentage));
        }

        return 0.0;
    }

    /// <summary>
    /// Format guidance line information for display
    /// </summary>
    /// <param name="lineType">Type of guidance line</param>
    /// <param name="heading">Heading in degrees</param>
    /// <returns>Formatted string "Line: [Type] [Heading]°"</returns>
    private string FormatGuidanceLineInfo(GuidanceLineType lineType, double heading)
    {
        int roundedHeading = (int)Math.Round(heading, 0);
        return $"Line: {lineType} {roundedHeading}°";
    }

    #endregion
}
