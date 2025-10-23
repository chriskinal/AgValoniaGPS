using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AgValoniaGPS.Desktop.Converters;

/// <summary>
/// Converts a boolean value to its inverse.
/// True becomes False, False becomes True.
/// Useful for inverting boolean bindings without logic in ViewModels.
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean to its inverse value.
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return value;
    }

    /// <summary>
    /// Converts back from inverted boolean to original value.
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return value;
    }
}
