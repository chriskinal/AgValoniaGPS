using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AgValoniaGPS.Desktop.Converters;

/// <summary>
/// Converts boolean value to color for status indicators.
/// True = Green (OK), False = Red (Error)
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // Return Green for true, Red for false
            return boolValue
                ? new SolidColorBrush(Color.FromRgb(39, 174, 96))  // Green
                : new SolidColorBrush(Color.FromRgb(231, 76, 60)); // Red
        }

        return new SolidColorBrush(Color.FromRgb(149, 165, 166)); // Gray for unknown
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
