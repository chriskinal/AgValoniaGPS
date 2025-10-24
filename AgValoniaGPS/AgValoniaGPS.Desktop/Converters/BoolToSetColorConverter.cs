using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AgValoniaGPS.Desktop.Converters;

/// <summary>
/// Converts boolean value to Color for point set indicators.
/// True = Green (Set), False = Red (Not Set)
/// Returns Color type (not Brush) for direct Color binding.
/// </summary>
public class BoolToSetColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // Return Green for true (set), Red for false (not set)
            return boolValue
                ? Color.FromRgb(39, 174, 96)  // Green
                : Color.FromRgb(231, 76, 60); // Red
        }

        return Color.FromRgb(149, 165, 166); // Gray for unknown
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
