using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AgValoniaGPS.Desktop.Converters;

/// <summary>
/// Converts empty/null strings to visibility state for UI controls.
/// Empty/Null = Collapsed (hidden), Non-Empty = Visible.
/// Use parameter="Inverse" to invert the logic (Empty = Visible, Non-Empty = Collapsed).
/// </summary>
public class EmptyStringToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts empty/null string to visibility state.
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isEmpty = string.IsNullOrWhiteSpace(value as string);
        var inverse = parameter as string == "Inverse";

        // Normal: empty = collapsed, non-empty = visible
        // Inverse: empty = visible, non-empty = collapsed
        if (inverse)
        {
            return isEmpty; // empty -> true (visible), non-empty -> false (collapsed)
        }

        return !isEmpty; // empty -> false (collapsed), non-empty -> true (visible)
    }

    /// <summary>
    /// ConvertBack is not supported for this converter.
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("EmptyStringToVisibilityConverter does not support ConvertBack.");
    }
}
