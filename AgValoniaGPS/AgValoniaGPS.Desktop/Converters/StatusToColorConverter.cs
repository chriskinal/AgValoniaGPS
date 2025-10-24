using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AgValoniaGPS.Desktop.Converters;

/// <summary>
/// Converts module status string to Color for status indicators.
/// Connected/Ready = Green, Disconnected = Red, Other = Gray
/// </summary>
public class StatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            // Map status strings to colors
            return status.ToLowerInvariant() switch
            {
                "connected" => Colors.Green,
                "ready" => Colors.Green,
                "active" => Colors.Green,
                "disconnected" => Colors.Red,
                "error" => Colors.Red,
                "failed" => Colors.Red,
                "idle" => Colors.Gray,
                "unknown" => Colors.Gray,
                _ => Colors.Gray
            };
        }

        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
