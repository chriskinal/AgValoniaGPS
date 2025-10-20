using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Validation;
using System.Globalization;

namespace AgValoniaGPS.Services.Validation;

/// <summary>
/// Validates culture settings including language codes.
/// </summary>
internal static class CultureSettingsValidator
{
    public static ValidationResult Validate(CultureSettings settings)
    {
        var result = new ValidationResult { IsValid = true };

        // Validate CultureCode matches valid locale codes
        if (!string.IsNullOrEmpty(settings.CultureCode))
        {
            try
            {
                // Attempt to create a CultureInfo to verify the code is valid
                var culture = new CultureInfo(settings.CultureCode);
            }
            catch (CultureNotFoundException)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    SettingPath = "Culture.CultureCode",
                    Message = $"Invalid culture code: '{settings.CultureCode}'. Must be a valid locale identifier (e.g., 'en', 'de', 'fr').",
                    InvalidValue = settings.CultureCode,
                    Constraints = new SettingConstraints
                    {
                        DataType = "string",
                        ValidationRule = "Must be a valid ISO locale code"
                    }
                });
            }
        }

        return result;
    }

    public static SettingConstraints? GetConstraints(string propertyName)
    {
        return propertyName switch
        {
            "CultureCode" => new SettingConstraints
            {
                DataType = "string",
                ValidationRule = "Must be a valid ISO locale code (e.g., 'en', 'de', 'fr', 'es')"
            },
            "LanguageName" => new SettingConstraints
            {
                DataType = "string",
                ValidationRule = "Human-readable language name"
            },
            _ => null
        };
    }
}
