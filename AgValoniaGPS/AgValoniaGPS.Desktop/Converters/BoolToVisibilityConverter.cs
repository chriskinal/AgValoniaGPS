using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AgValoniaGPS.Desktop.Converters;

/// <summary>
/// Converts a boolean value to a visibility state for UI controls.
/// True = Visible, False = Collapsed (hidden and takes no space).
/// Use parameter="Inverse" to invert the logic (True = Collapsed, False = Visible).
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean to a visibility state.
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue)
        {
            return false; // Default to not visible for non-boolean values
        }

        // Check if we should invert the logic
        var inverse = parameter as string == "Inverse";

        if (inverse)
        {
            return !boolValue;
        }

        return boolValue;
    }

    /// <summary>
    /// Converts back from visibility to boolean (not typically used).
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue)
        {
            return false;
        }

        var inverse = parameter as string == "Inverse";
        return inverse ? !boolValue : boolValue;
    }
}
