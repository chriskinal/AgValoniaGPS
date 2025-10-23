using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AgValoniaGPS.Desktop.Converters;

/// <summary>
/// Converts null values to visibility state for UI controls.
/// Null = Collapsed (hidden), Not Null = Visible.
/// Use parameter="Inverse" to invert the logic (Null = Visible, Not Null = Collapsed).
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts null/not-null to visibility state.
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isNull = value == null;
        var inverse = parameter as string == "Inverse";

        // Normal: null = collapsed, not null = visible
        // Inverse: null = visible, not null = collapsed
        if (inverse)
        {
            return isNull; // null -> true (visible), not null -> false (collapsed)
        }

        return !isNull; // null -> false (collapsed), not null -> true (visible)
    }

    /// <summary>
    /// ConvertBack is not supported for this converter.
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("NullToVisibilityConverter does not support ConvertBack.");
    }
}
