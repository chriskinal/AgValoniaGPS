using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AgValoniaGPS.Desktop.Converters;

/// <summary>
/// Converts GPS fix quality string to color brush for visual indication.
/// "No Fix" = Gray, "GPS Fix" = Yellow, "DGPS Fix" = Orange, "RTK Float" = Light Green, "RTK Fixed" = Green
/// </summary>
public class FixQualityToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string fixQuality)
        {
            return fixQuality switch
            {
                "No Fix" => new SolidColorBrush(Color.FromRgb(149, 165, 166)), // Gray
                "GPS Fix" => new SolidColorBrush(Color.FromRgb(241, 196, 15)), // Yellow
                "DGPS Fix" => new SolidColorBrush(Color.FromRgb(230, 126, 34)), // Orange
                "RTK Float" => new SolidColorBrush(Color.FromRgb(46, 204, 113)), // Light Green
                "RTK Fixed" => new SolidColorBrush(Color.FromRgb(39, 174, 96)), // Green
                _ => new SolidColorBrush(Color.FromRgb(149, 165, 166)) // Gray for unknown
            };
        }

        return new SolidColorBrush(Color.FromRgb(149, 165, 166)); // Gray
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
