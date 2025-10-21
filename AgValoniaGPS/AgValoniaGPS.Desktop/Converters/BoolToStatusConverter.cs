using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AgValoniaGPS.Desktop.Converters;

/// <summary>
/// Converts boolean value to status text.
/// True = "Connected", False = "Disconnected"
/// </summary>
public class BoolToStatusConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "Connected" : "Disconnected";
        }

        return "Unknown";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
