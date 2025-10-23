using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AgValoniaGPS.Desktop.Converters;

/// <summary>
/// Converts between metric and imperial units.
/// Pass "metric" or "imperial" as the parameter to specify the target unit system.
/// Assumes input values are in meters for distance and meters/second for speed.
/// </summary>
public class UnitConverter : IValueConverter
{
    // Conversion constants
    private const double MetersToFeet = 3.28084;
    private const double MetersToMiles = 0.000621371;
    private const double MetersToKilometers = 0.001;
    private const double MpsToMph = 2.23694; // meters/second to miles/hour
    private const double MpsToKmh = 3.6; // meters/second to km/hour

    /// <summary>
    /// Converts a value from meters (or m/s) to the specified unit system.
    /// </summary>
    /// <param name="value">The value in meters or meters/second.</param>
    /// <param name="targetType">The target type (usually double or string).</param>
    /// <param name="parameter">The target unit: "metric", "imperial", "meters", "feet", "miles", "kilometers", "mph", "kmh".</param>
    /// <param name="culture">The culture to use for formatting.</param>
    /// <returns>The converted value.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return null;
        }

        if (!double.TryParse(value.ToString(), out double numericValue))
        {
            return value;
        }

        var unit = (parameter as string)?.ToLowerInvariant() ?? "metric";

        var convertedValue = unit switch
        {
            "imperial" => numericValue * MetersToFeet, // Default imperial: feet
            "metric" => numericValue, // Already in meters
            "meters" => numericValue,
            "feet" => numericValue * MetersToFeet,
            "miles" => numericValue * MetersToMiles,
            "kilometers" or "km" => numericValue * MetersToKilometers,
            "mph" => numericValue * MpsToMph,
            "kmh" or "kph" => numericValue * MpsToKmh,
            _ => numericValue
        };

        // If target type is string, format the output
        if (targetType == typeof(string))
        {
            var unitSuffix = unit switch
            {
                "imperial" => " ft",
                "feet" => " ft",
                "miles" => " mi",
                "kilometers" or "km" => " km",
                "mph" => " mph",
                "kmh" or "kph" => " km/h",
                "metric" or "meters" => " m",
                _ => ""
            };

            return $"{convertedValue:F2}{unitSuffix}";
        }

        return convertedValue;
    }

    /// <summary>
    /// Converts a value back from the specified unit system to meters (or m/s).
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return null;
        }

        if (!double.TryParse(value.ToString(), out double numericValue))
        {
            return value;
        }

        var unit = (parameter as string)?.ToLowerInvariant() ?? "metric";

        return unit switch
        {
            "imperial" or "feet" => numericValue / MetersToFeet,
            "miles" => numericValue / MetersToMiles,
            "kilometers" or "km" => numericValue / MetersToKilometers,
            "mph" => numericValue / MpsToMph,
            "kmh" or "kph" => numericValue / MpsToKmh,
            "metric" or "meters" => numericValue,
            _ => numericValue
        };
    }
}
