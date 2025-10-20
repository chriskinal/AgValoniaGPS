using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Models.Display;
using GuidanceLineType = AgValoniaGPS.Models.Guidance.GuidanceLineType;

namespace AgValoniaGPS.Services.Display
{
    /// <summary>
    /// Provides culture-invariant formatting for all display elements.
    /// Supports both metric and imperial unit systems with dynamic precision.
    /// All methods are thread-safe and return safe defaults for invalid inputs (never throw).
    /// </summary>
    /// <remarks>
    /// Performance target: All formatting operations must complete in less than 1ms.
    /// All numeric formatting uses InvariantCulture to ensure consistent decimal separators.
    /// </remarks>
    public interface IDisplayFormatterService
    {
        /// <summary>
        /// Formats speed with dynamic precision based on magnitude.
        /// </summary>
        /// <param name="speedMetersPerSecond">Speed in meters per second</param>
        /// <param name="unitSystem">Target unit system (Metric: km/h, Imperial: mph)</param>
        /// <returns>Formatted speed string without unit suffix</returns>
        /// <remarks>
        /// Dynamic precision:
        /// - If converted speed > 2 km/h: 1 decimal place (e.g., "20.0")
        /// - If converted speed <= 2 km/h: 2 decimal places (e.g., "1.85")
        /// Returns "0.0" for NaN or Infinity.
        /// Example: 5.56 m/s, Metric → "20.0" km/h
        /// </remarks>
        string FormatSpeed(double speedMetersPerSecond, UnitSystem unitSystem);

        /// <summary>
        /// Formats heading in degrees with ° symbol.
        /// </summary>
        /// <param name="headingDegrees">Heading in degrees (0-360)</param>
        /// <returns>Formatted heading string with ° symbol (e.g., "46°")</returns>
        /// <remarks>
        /// Rounds to nearest whole degree.
        /// Returns "0°" for NaN or Infinity.
        /// Example: 45.5 → "46°", 0.0 → "0°"
        /// </remarks>
        string FormatHeading(double headingDegrees);

        /// <summary>
        /// Formats GPS quality with fix type text and color name.
        /// </summary>
        /// <param name="fixType">GPS fix type (None, Autonomous, DGPS, RtkFixed, RtkFloat)</param>
        /// <param name="age">Age of GPS fix in seconds</param>
        /// <returns>GpsQualityDisplay with formatted text and color name</returns>
        /// <remarks>
        /// Format: "[FixType]: Age: [age with 1 decimal]"
        /// Color mapping:
        /// - RtkFixed (4) → "RTK fix: Age: 0.0", "PaleGreen"
        /// - RtkFloat (5) → "RTK float: Age: 1.2", "Orange"
        /// - DGPS (2) → "DGPS: Age: 0.5", "Yellow"
        /// - Autonomous (1) → "Autonomous: Age: 2.0", "Red"
        /// - None (0) → "No Fix: Age: 0.0", "Red"
        /// </remarks>
        GpsQualityDisplay FormatGpsQuality(GpsFixType fixType, double age);

        /// <summary>
        /// Formats GPS precision in meters.
        /// </summary>
        /// <param name="precisionMeters">GPS precision in meters</param>
        /// <returns>Formatted precision string with 2 decimal places (e.g., "0.01")</returns>
        /// <remarks>
        /// No unit suffix included.
        /// Returns "0.00" for NaN or Infinity.
        /// Example: 0.012 → "0.01"
        /// </remarks>
        string FormatGpsPrecision(double precisionMeters);

        /// <summary>
        /// Formats cross-track error (distance from guidance line).
        /// </summary>
        /// <param name="errorMeters">Cross-track error in meters</param>
        /// <param name="unitSystem">Target unit system (Metric: cm, Imperial: inches)</param>
        /// <returns>Formatted error string with unit suffix</returns>
        /// <remarks>
        /// Metric: Convert to cm (×100), 1 decimal, append " cm"
        /// Imperial: Convert to inches (×39.3701), 1 decimal, append " in"
        /// Returns "0.0 cm" or "0.0 in" for NaN or Infinity.
        /// Example: 0.018 m, Metric → "1.8 cm"
        /// </remarks>
        string FormatCrossTrackError(double errorMeters, UnitSystem unitSystem);

        /// <summary>
        /// Formats distance with automatic unit switching based on magnitude.
        /// </summary>
        /// <param name="distanceMeters">Distance in meters</param>
        /// <param name="unitSystem">Target unit system</param>
        /// <returns>Formatted distance string with unit suffix</returns>
        /// <remarks>
        /// Metric:
        /// - Less than 1000m: "[value] m" with 1 decimal (e.g., "450.5 m")
        /// - 1000m or more: "[value] km" with 2 decimals (e.g., "1.25 km")
        /// Imperial:
        /// - Less than 5280ft: "[value] ft" with 0 decimals (e.g., "3000 ft")
        /// - 5280ft or more: "[value] mi" with 2 decimals (e.g., "1.00 mi")
        /// Returns "0.0 m" or "0 ft" for NaN or Infinity.
        /// </remarks>
        string FormatDistance(double distanceMeters, UnitSystem unitSystem);

        /// <summary>
        /// Formats area in hectares (metric) or acres (imperial).
        /// </summary>
        /// <param name="areaSquareMeters">Area in square meters</param>
        /// <param name="unitSystem">Target unit system</param>
        /// <returns>Formatted area string with unit suffix</returns>
        /// <remarks>
        /// Metric: Convert to hectares (÷10000), 2 decimals, append " ha"
        /// Imperial: Convert to acres (×0.000247105), 2 decimals, append " ac"
        /// Returns "0.00 ha" or "0.00 ac" for NaN or Infinity.
        /// Example: 10000 m², Metric → "1.00 ha"
        /// </remarks>
        string FormatArea(double areaSquareMeters, UnitSystem unitSystem);

        /// <summary>
        /// Formats time duration in hours.
        /// </summary>
        /// <param name="hours">Time duration in hours</param>
        /// <returns>Formatted time string with " hr" suffix</returns>
        /// <remarks>
        /// Format: 2 decimal places, append " hr"
        /// Returns "0.00 hr" for NaN or Infinity.
        /// Example: 2.25 → "2.25 hr", 0.5 → "0.50 hr"
        /// </remarks>
        string FormatTime(double hours);

        /// <summary>
        /// Formats application rate in area per hour.
        /// </summary>
        /// <param name="rate">Application rate value</param>
        /// <param name="unitSystem">Target unit system</param>
        /// <returns>Formatted rate string with unit suffix</returns>
        /// <remarks>
        /// Metric: "[value] ha/hr" with 2 decimals
        /// Imperial: "[value] ac/hr" with 2 decimals
        /// Returns "0.00 ha/hr" or "0.00 ac/hr" for NaN or Infinity.
        /// Example: 5.0, Imperial → "5.00 ac/hr"
        /// </remarks>
        string FormatApplicationRate(double rate, UnitSystem unitSystem);

        /// <summary>
        /// Formats percentage with 1 decimal place.
        /// </summary>
        /// <param name="percentage">Percentage value (0-100)</param>
        /// <returns>Formatted percentage string with "%" suffix</returns>
        /// <remarks>
        /// Format: 1 decimal place, append "%"
        /// Returns "0.0%" for NaN or Infinity.
        /// Example: 100.0 → "100.0%", 75.5 → "75.5%"
        /// </remarks>
        string FormatPercentage(double percentage);

        /// <summary>
        /// Formats guidance line information with type and heading.
        /// </summary>
        /// <param name="lineType">Guidance line type (ABLine, CurveLine, Contour)</param>
        /// <param name="headingDegrees">Heading in degrees</param>
        /// <returns>Formatted guidance line string</returns>
        /// <remarks>
        /// Format: "Line: [LineType] [heading]°"
        /// Heading rounded to nearest degree.
        /// Returns "Line: ABLine 0°" for NaN or Infinity heading.
        /// Example: ABLine, 0.0 → "Line: ABLine 0°", CurveLine, 45.5 → "Line: CurveLine 46°"
        /// </remarks>
        string FormatGuidanceLine(GuidanceLineType lineType, double headingDegrees);
    }
}
