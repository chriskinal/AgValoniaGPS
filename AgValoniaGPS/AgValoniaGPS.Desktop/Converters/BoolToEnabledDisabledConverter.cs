using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AgValoniaGPS.Desktop.Converters;

/// <summary>
/// Converts boolean value to "Enabled"/"Disabled" text.
/// True = "Enabled", False = "Disabled"
/// </summary>
public class BoolToEnabledDisabledConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "Enabled" : "Disabled";
        }

        return "Unknown";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            return stringValue.Equals("Enabled", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}
