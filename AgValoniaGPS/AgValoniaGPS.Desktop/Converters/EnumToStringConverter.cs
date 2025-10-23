using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AgValoniaGPS.Desktop.Converters;

/// <summary>
/// Converts enum values to their string representation.
/// Uses ToString() by default, or can apply custom formatting rules.
/// </summary>
public class EnumToStringConverter : IValueConverter
{
    /// <summary>
    /// Converts an enum value to a string.
    /// </summary>
    /// <param name="value">The enum value to convert.</param>
    /// <param name="targetType">The target type (should be string).</param>
    /// <param name="parameter">Optional format parameter: "Upper", "Lower", "Title", or custom format string.</param>
    /// <param name="culture">The culture to use for formatting.</param>
    /// <returns>The string representation of the enum value.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (!value.GetType().IsEnum)
        {
            return value.ToString();
        }

        var enumString = value.ToString() ?? string.Empty;

        // Apply formatting based on parameter
        var format = (parameter as string)?.ToLowerInvariant();

        return format switch
        {
            "upper" => enumString.ToUpperInvariant(),
            "lower" => enumString.ToLowerInvariant(),
            "title" => FormatAsTitle(enumString),
            _ => enumString
        };
    }

    /// <summary>
    /// Converts a string back to an enum value.
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || !targetType.IsEnum)
        {
            return null;
        }

        var stringValue = value.ToString();
        if (string.IsNullOrWhiteSpace(stringValue))
        {
            return null;
        }

        try
        {
            return Enum.Parse(targetType, stringValue, ignoreCase: true);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Formats an enum string as title case with spaces between words.
    /// Example: "MyEnumValue" -> "My Enum Value"
    /// </summary>
    private static string FormatAsTitle(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var result = new System.Text.StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];

            // Add space before uppercase letters (except first character)
            if (i > 0 && char.IsUpper(c) && !char.IsUpper(input[i - 1]))
            {
                result.Append(' ');
            }

            result.Append(c);
        }

        return result.ToString();
    }
}
