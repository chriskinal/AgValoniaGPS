using System;
using System.Globalization;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Models.Display;
using GuidanceLineType = AgValoniaGPS.Models.Guidance.GuidanceLineType;

namespace AgValoniaGPS.Services.Display
{
    /// <summary>
    /// Implements culture-invariant formatting for all display elements.
    /// Provides thread-safe, UI-agnostic formatting with dynamic precision and unit conversion.
    /// </summary>
    /// <remarks>
    /// All formatting uses InvariantCulture to ensure consistent decimal separators.
    /// All methods return safe defaults for invalid inputs (NaN, Infinity) - never throws exceptions.
    /// Performance optimized for less than 1ms per operation.
    /// </remarks>
    public class DisplayFormatterService : IDisplayFormatterService
    {
        #region Conversion Constants

        // Distance conversions
        private const double MetersToFeet = 3.28084;
        private const double MetersToInches = 39.3701;
        private const double MetersToKilometers = 0.001;
        private const double MetersToMiles = 0.000621371;
        private const double MetersToCentimeters = 100.0;

        // Speed conversions
        private const double MetersPerSecondToKmh = 3.6;
        private const double MetersPerSecondToMph = 2.23694;

        // Area conversions
        private const double SquareMetersToHectares = 0.0001;
        private const double SquareMetersToAcres = 0.000247105;

        // Thresholds for unit switching
        private const double SpeedPrecisionThresholdKmh = 2.0;
        private const double DistanceThresholdMeters = 1000.0;
        private const double DistanceThresholdFeet = 5280.0;

        #endregion

        #region Speed Formatting

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
        /// </remarks>
        public string FormatSpeed(double speedMetersPerSecond, UnitSystem unitSystem)
        {
            // Handle invalid inputs
            if (double.IsNaN(speedMetersPerSecond) || double.IsInfinity(speedMetersPerSecond))
            {
                return "0.0";
            }

            double convertedSpeed;

            // Convert to target unit system
            if (unitSystem == UnitSystem.Metric)
            {
                convertedSpeed = speedMetersPerSecond * MetersPerSecondToKmh;
            }
            else // Imperial
            {
                convertedSpeed = speedMetersPerSecond * MetersPerSecondToMph;
            }

            // Apply dynamic precision based on speed magnitude
            // For metric: use km/h threshold of 2.0
            // For imperial: convert threshold to mph (2.0 km/h ≈ 1.24 mph)
            double precisionThreshold = unitSystem == UnitSystem.Metric
                ? SpeedPrecisionThresholdKmh
                : SpeedPrecisionThresholdKmh * MetersPerSecondToMph / MetersPerSecondToKmh;

            string format = convertedSpeed > precisionThreshold ? "F1" : "F2";

            return convertedSpeed.ToString(format, CultureInfo.InvariantCulture);
        }

        #endregion

        #region Heading Formatting

        /// <summary>
        /// Formats heading in degrees with ° symbol.
        /// </summary>
        /// <param name="headingDegrees">Heading in degrees (0-360)</param>
        /// <returns>Formatted heading string with ° symbol (e.g., "46°")</returns>
        /// <remarks>
        /// Rounds to nearest whole degree.
        /// Returns "0°" for NaN or Infinity.
        /// </remarks>
        public string FormatHeading(double headingDegrees)
        {
            // Handle invalid inputs
            if (double.IsNaN(headingDegrees) || double.IsInfinity(headingDegrees))
            {
                return "0°";
            }

            // Round to nearest whole degree
            int roundedHeading = (int)Math.Round(headingDegrees, 0);

            return $"{roundedHeading}°";
        }

        #endregion

        #region Distance Formatting

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
        public string FormatDistance(double distanceMeters, UnitSystem unitSystem)
        {
            // Handle invalid inputs
            if (double.IsNaN(distanceMeters) || double.IsInfinity(distanceMeters))
            {
                return unitSystem == UnitSystem.Metric ? "0.0 m" : "0 ft";
            }

            if (unitSystem == UnitSystem.Metric)
            {
                // Metric: meters or kilometers
                if (distanceMeters < DistanceThresholdMeters)
                {
                    // Use meters with 1 decimal
                    return distanceMeters.ToString("F1", CultureInfo.InvariantCulture) + " m";
                }
                else
                {
                    // Use kilometers with 2 decimals
                    double kilometers = distanceMeters * MetersToKilometers;
                    return kilometers.ToString("F2", CultureInfo.InvariantCulture) + " km";
                }
            }
            else // Imperial
            {
                // Convert to feet first
                double feet = distanceMeters * MetersToFeet;

                if (feet < DistanceThresholdFeet)
                {
                    // Use feet with 0 decimals
                    return feet.ToString("F0", CultureInfo.InvariantCulture) + " ft";
                }
                else
                {
                    // Use miles with 2 decimals
                    double miles = distanceMeters * MetersToMiles;
                    return miles.ToString("F2", CultureInfo.InvariantCulture) + " mi";
                }
            }
        }

        #endregion

        #region GPS Quality Formatting

        /// <summary>
        /// Formats GPS quality with fix type text and color name.
        /// </summary>
        /// <param name="fixType">GPS fix type (None, Autonomous, DGPS, RtkFixed, RtkFloat)</param>
        /// <param name="age">Age of GPS fix in seconds</param>
        /// <returns>GpsQualityDisplay with formatted text and color name</returns>
        public GpsQualityDisplay FormatGpsQuality(GpsFixType fixType, double age)
        {
            // Handle invalid age
            if (double.IsNaN(age) || double.IsInfinity(age))
            {
                age = 0.0;
            }

            string fixTypeText;
            string colorName;

            // Map fix type to text and color
            switch (fixType)
            {
                case GpsFixType.RtkFixed:
                    fixTypeText = "RTK fix";
                    colorName = "PaleGreen";
                    break;
                case GpsFixType.RtkFloat:
                    fixTypeText = "RTK float";
                    colorName = "Orange";
                    break;
                case GpsFixType.DGPS:
                    fixTypeText = "DGPS";
                    colorName = "Yellow";
                    break;
                case GpsFixType.Autonomous:
                    fixTypeText = "Autonomous";
                    colorName = "Red";
                    break;
                case GpsFixType.None:
                default:
                    fixTypeText = "No Fix";
                    colorName = "Red";
                    break;
            }

            // Format age with 1 decimal place
            string ageFormatted = age.ToString("F1", CultureInfo.InvariantCulture);

            // Build formatted text
            string formattedText = $"{fixTypeText}: Age: {ageFormatted}";

            return new GpsQualityDisplay
            {
                FormattedText = formattedText,
                ColorName = colorName
            };
        }

        #endregion

        #region GPS Precision Formatting

        /// <summary>
        /// Formats GPS precision in meters.
        /// </summary>
        /// <param name="precisionMeters">GPS precision in meters</param>
        /// <returns>Formatted precision string with 2 decimal places (e.g., "0.01")</returns>
        public string FormatGpsPrecision(double precisionMeters)
        {
            // Handle invalid inputs
            if (double.IsNaN(precisionMeters) || double.IsInfinity(precisionMeters))
            {
                return "0.00";
            }

            return precisionMeters.ToString("F2", CultureInfo.InvariantCulture);
        }

        #endregion

        #region Cross-Track Error Formatting

        /// <summary>
        /// Formats cross-track error (distance from guidance line).
        /// </summary>
        /// <param name="errorMeters">Cross-track error in meters</param>
        /// <param name="unitSystem">Target unit system (Metric: cm, Imperial: inches)</param>
        /// <returns>Formatted error string with unit suffix</returns>
        public string FormatCrossTrackError(double errorMeters, UnitSystem unitSystem)
        {
            // Handle invalid inputs
            if (double.IsNaN(errorMeters) || double.IsInfinity(errorMeters))
            {
                return unitSystem == UnitSystem.Metric ? "0.0 cm" : "0.0 in";
            }

            if (unitSystem == UnitSystem.Metric)
            {
                // Convert to centimeters with 1 decimal
                double centimeters = errorMeters * MetersToCentimeters;
                return centimeters.ToString("F1", CultureInfo.InvariantCulture) + " cm";
            }
            else // Imperial
            {
                // Convert to inches with 1 decimal
                double inches = errorMeters * MetersToInches;
                return inches.ToString("F1", CultureInfo.InvariantCulture) + " in";
            }
        }

        #endregion

        #region Area Formatting

        /// <summary>
        /// Formats area in hectares (metric) or acres (imperial).
        /// </summary>
        /// <param name="areaSquareMeters">Area in square meters</param>
        /// <param name="unitSystem">Target unit system</param>
        /// <returns>Formatted area string with unit suffix</returns>
        public string FormatArea(double areaSquareMeters, UnitSystem unitSystem)
        {
            // Handle invalid inputs
            if (double.IsNaN(areaSquareMeters) || double.IsInfinity(areaSquareMeters))
            {
                return unitSystem == UnitSystem.Metric ? "0.00 ha" : "0.00 ac";
            }

            if (unitSystem == UnitSystem.Metric)
            {
                // Convert to hectares with 2 decimals
                double hectares = areaSquareMeters * SquareMetersToHectares;
                return hectares.ToString("F2", CultureInfo.InvariantCulture) + " ha";
            }
            else // Imperial
            {
                // Convert to acres with 2 decimals
                double acres = areaSquareMeters * SquareMetersToAcres;
                return acres.ToString("F2", CultureInfo.InvariantCulture) + " ac";
            }
        }

        #endregion

        #region Time Formatting

        /// <summary>
        /// Formats time duration in hours.
        /// </summary>
        /// <param name="hours">Time duration in hours</param>
        /// <returns>Formatted time string with " hr" suffix</returns>
        public string FormatTime(double hours)
        {
            // Handle invalid inputs
            if (double.IsNaN(hours) || double.IsInfinity(hours))
            {
                return "0.00 hr";
            }

            return hours.ToString("F2", CultureInfo.InvariantCulture) + " hr";
        }

        #endregion

        #region Application Rate Formatting

        /// <summary>
        /// Formats application rate in area per hour.
        /// </summary>
        /// <param name="rate">Application rate value</param>
        /// <param name="unitSystem">Target unit system</param>
        /// <returns>Formatted rate string with unit suffix</returns>
        public string FormatApplicationRate(double rate, UnitSystem unitSystem)
        {
            // Handle invalid inputs
            if (double.IsNaN(rate) || double.IsInfinity(rate))
            {
                return unitSystem == UnitSystem.Metric ? "0.00 ha/hr" : "0.00 ac/hr";
            }

            if (unitSystem == UnitSystem.Metric)
            {
                return rate.ToString("F2", CultureInfo.InvariantCulture) + " ha/hr";
            }
            else // Imperial
            {
                return rate.ToString("F2", CultureInfo.InvariantCulture) + " ac/hr";
            }
        }

        #endregion

        #region Percentage Formatting

        /// <summary>
        /// Formats percentage with 1 decimal place.
        /// </summary>
        /// <param name="percentage">Percentage value (0-100)</param>
        /// <returns>Formatted percentage string with "%" suffix</returns>
        public string FormatPercentage(double percentage)
        {
            // Handle invalid inputs
            if (double.IsNaN(percentage) || double.IsInfinity(percentage))
            {
                return "0.0%";
            }

            return percentage.ToString("F1", CultureInfo.InvariantCulture) + "%";
        }

        #endregion

        #region Guidance Line Formatting

        /// <summary>
        /// Formats guidance line information with type and heading.
        /// </summary>
        /// <param name="lineType">Guidance line type (ABLine, CurveLine, Contour)</param>
        /// <param name="headingDegrees">Heading in degrees</param>
        /// <returns>Formatted guidance line string</returns>
        public string FormatGuidanceLine(GuidanceLineType lineType, double headingDegrees)
        {
            // Handle invalid heading
            if (double.IsNaN(headingDegrees) || double.IsInfinity(headingDegrees))
            {
                headingDegrees = 0.0;
            }

            // Round heading to nearest degree
            int roundedHeading = (int)Math.Round(headingDegrees, 0);

            // Format as "Line: [LineType] [heading]°"
            return $"Line: {lineType} {roundedHeading}°";
        }

        #endregion
    }
}
