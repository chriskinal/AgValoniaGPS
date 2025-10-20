using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Validation;

namespace AgValoniaGPS.Services.Validation;

/// <summary>
/// Validates cross-setting dependencies where settings from different categories interact.
/// Handles validation rules that span multiple settings categories.
/// </summary>
internal static class CrossSettingValidator
{
    public static ValidationResult Validate(ApplicationSettings settings)
    {
        var result = new ValidationResult { IsValid = true };

        // Tool Width affects Guidance: Warn if LookAhead < ToolWidth * 0.5
        if (settings.Tool != null && settings.Guidance != null)
        {
            var minimumLookAhead = settings.Tool.ToolWidth * 0.5;
            if (settings.Guidance.LookAhead < minimumLookAhead)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    SettingPath = "Guidance.LookAhead",
                    Message = $"Look-ahead distance ({settings.Guidance.LookAhead}m) is shorter than recommended for tool width ({settings.Tool.ToolWidth}m). " +
                              $"Recommended minimum: {minimumLookAhead:F2}m for effective guidance.",
                    Value = settings.Guidance.LookAhead
                });
            }
        }

        // Section Count affects Section Positions: Length must match
        if (settings.SectionControl != null)
        {
            if (settings.SectionControl.SectionPositions != null &&
                settings.SectionControl.SectionPositions.Length != settings.SectionControl.NumberSections)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    SettingPath = "SectionControl.SectionPositions",
                    Message = $"Section positions array length ({settings.SectionControl.SectionPositions.Length}) " +
                              $"must match number of sections ({settings.SectionControl.NumberSections}).",
                    InvalidValue = settings.SectionControl.SectionPositions.Length,
                    Constraints = new SettingConstraints
                    {
                        DataType = "double[]",
                        Dependencies = new[] { "SectionControl.NumberSections" },
                        ValidationRule = "Section positions array length must equal NumberSections"
                    }
                });
            }

            // Section Positions should span Tool Width
            if (settings.Tool != null &&
                settings.SectionControl.SectionPositions != null &&
                settings.SectionControl.SectionPositions.Length > 0)
            {
                var minPosition = settings.SectionControl.SectionPositions.Min();
                var maxPosition = settings.SectionControl.SectionPositions.Max();
                var positionSpan = maxPosition - minPosition;
                var expectedSpan = settings.Tool.ToolWidth;

                // Warn if positions don't span approximately the tool width (allow 20% tolerance)
                if (positionSpan < expectedSpan * 0.8 || positionSpan > expectedSpan * 1.2)
                {
                    result.Warnings.Add(new ValidationWarning
                    {
                        SettingPath = "SectionControl.SectionPositions",
                        Message = $"Section positions span ({positionSpan:F2}m) differs significantly from tool width ({expectedSpan:F2}m). " +
                                  $"Verify section positions are correctly configured.",
                        Value = positionSpan
                    });
                }
            }
        }

        // GPS Heading Source affects IMU Settings relevance
        if (settings.Gps != null && settings.Imu != null)
        {
            // If HeadingFrom = "Dual", DualAsImu and DualHeadingOffset are relevant
            if (settings.Gps.HeadingFrom == "Dual" && !settings.Imu.DualAsImu)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    SettingPath = "Imu.DualAsImu",
                    Message = "GPS heading source is set to 'Dual' but DualAsImu is disabled. " +
                              "Consider enabling DualAsImu for dual antenna configuration.",
                    Value = settings.Imu.DualAsImu
                });
            }

            // If HeadingFrom = "IMU", ImuFusionWeight is relevant
            if (settings.Gps.HeadingFrom == "IMU" && settings.Imu.ImuFusionWeight < 0.01)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    SettingPath = "Imu.ImuFusionWeight",
                    Message = "GPS heading source is set to 'IMU' but ImuFusionWeight is very low. " +
                              "Increase ImuFusionWeight for IMU-based heading.",
                    Value = settings.Imu.ImuFusionWeight
                });
            }

            // Warn if HeadingFrom = "GPS" (single antenna) which may be insufficient for heading
            if (settings.Gps.HeadingFrom == "GPS")
            {
                result.Warnings.Add(new ValidationWarning
                {
                    SettingPath = "Gps.HeadingFrom",
                    Message = "GPS heading source is set to 'GPS' (single antenna). " +
                              "Single antenna GPS may not provide reliable heading at low speeds. " +
                              "Consider using 'Dual' for dual antenna or 'IMU' for IMU-based heading.",
                    Value = settings.Gps.HeadingFrom
                });
            }
        }

        // Steering Limits should be achievable by hardware
        if (settings.Vehicle != null && settings.Steering != null)
        {
            // MaxSteerAngle should be realistic given CPD (Counts Per Degree)
            // With higher CPD, finer control is possible but may limit max angle
            if (settings.Vehicle.MaxSteerAngle > 60.0 && settings.Steering.CountsPerDegree < 50)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    SettingPath = "Steering.CountsPerDegree",
                    Message = $"High maximum steer angle ({settings.Vehicle.MaxSteerAngle}°) with low CPD ({settings.Steering.CountsPerDegree}). " +
                              $"Consider increasing CPD for better control precision.",
                    Value = settings.Steering.CountsPerDegree
                });
            }

            // MaxAngularVelocity should be realistic given MaxSteerAngle
            // Extremely high angular velocity with low max steer angle is unusual
            if (settings.Vehicle.MaxAngularVelocity > 150.0 && settings.Vehicle.MaxSteerAngle < 20.0)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    SettingPath = "Vehicle.MaxAngularVelocity",
                    Message = $"High angular velocity ({settings.Vehicle.MaxAngularVelocity}°/s) with low max steer angle ({settings.Vehicle.MaxSteerAngle}°) is unusual. " +
                              $"Verify settings match your hardware capabilities.",
                    Value = settings.Vehicle.MaxAngularVelocity
                });
            }
        }

        return result;
    }
}
