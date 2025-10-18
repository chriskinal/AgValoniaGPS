namespace AgValoniaGPS.Models.Guidance;

/// <summary>
/// Defines the parameters used for smoothing curved guidance lines.
/// </summary>
public class SmoothingParameters
{
    /// <summary>
    /// Gets or sets the smoothing method to apply.
    /// </summary>
    public SmoothingMethod Method { get; set; } = SmoothingMethod.CubicSpline;

    /// <summary>
    /// Gets or sets the tension parameter for spline smoothing.
    /// Range: 0.0 (relaxed) to 1.0 (tight).
    /// Higher values produce curves that adhere more closely to control points.
    /// </summary>
    public double Tension { get; set; } = 0.5;

    /// <summary>
    /// Gets or sets the desired point density for the smoothed curve.
    /// Represents the target spacing between points in meters.
    /// Smaller values produce smoother curves but more points.
    /// </summary>
    public double PointDensity { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the maximum number of iterations for iterative smoothing algorithms.
    /// </summary>
    public int MaxIterations { get; set; } = 100;

    /// <summary>
    /// Creates default smoothing parameters for cubic spline smoothing.
    /// </summary>
    public static SmoothingParameters CreateDefault()
    {
        return new SmoothingParameters
        {
            Method = SmoothingMethod.CubicSpline,
            Tension = 0.5,
            PointDensity = 1.0,
            MaxIterations = 100
        };
    }

    /// <summary>
    /// Creates smoothing parameters optimized for high-quality smooth curves.
    /// Uses natural cubic spline with high point density.
    /// </summary>
    public static SmoothingParameters CreateHighQuality()
    {
        return new SmoothingParameters
        {
            Method = SmoothingMethod.CubicSpline,
            Tension = 0.3,
            PointDensity = 0.5,
            MaxIterations = 150
        };
    }

    /// <summary>
    /// Creates smoothing parameters optimized for performance with acceptable quality.
    /// Uses Catmull-Rom spline with moderate point density.
    /// </summary>
    public static SmoothingParameters CreatePerformance()
    {
        return new SmoothingParameters
        {
            Method = SmoothingMethod.CatmullRom,
            Tension = 0.5,
            PointDensity = 2.0,
            MaxIterations = 50
        };
    }

    /// <summary>
    /// Creates smoothing parameters that maintain curve shape through control points.
    /// Uses Catmull-Rom spline which passes through all control points.
    /// </summary>
    public static SmoothingParameters CreateShapePreserving()
    {
        return new SmoothingParameters
        {
            Method = SmoothingMethod.CatmullRom,
            Tension = 0.7,
            PointDensity = 1.0,
            MaxIterations = 100
        };
    }

    /// <summary>
    /// Validates the smoothing parameters.
    /// </summary>
    /// <returns>ValidationResult indicating validity and any issues.</returns>
    public ValidationResult Validate()
    {
        var result = new ValidationResult();

        if (Tension < 0.0 || Tension > 1.0)
        {
            result.AddError($"Tension must be between 0.0 and 1.0. Current value: {Tension}");
        }

        if (PointDensity <= 0.0)
        {
            result.AddError($"PointDensity must be positive. Current value: {PointDensity}");
        }

        if (PointDensity < 0.1)
        {
            result.AddWarning($"Very low PointDensity ({PointDensity}m) may result in excessive point count and slow performance");
        }

        if (PointDensity > 10.0)
        {
            result.AddWarning($"High PointDensity ({PointDensity}m) may result in jagged curves");
        }

        if (MaxIterations < 1)
        {
            result.AddError($"MaxIterations must be at least 1. Current value: {MaxIterations}");
        }

        if (MaxIterations > 1000)
        {
            result.AddWarning($"Very high MaxIterations ({MaxIterations}) may impact performance");
        }

        return result;
    }
}

/// <summary>
/// Defines the available smoothing methods for curve line processing.
/// </summary>
public enum SmoothingMethod
{
    /// <summary>
    /// Natural cubic spline interpolation.
    /// Produces the smoothest curves but may not pass through all control points.
    /// Best for general-purpose smoothing.
    /// </summary>
    CubicSpline,

    /// <summary>
    /// Catmull-Rom spline interpolation.
    /// Passes through all control points while maintaining smoothness.
    /// Best for preserving the original curve shape.
    /// </summary>
    CatmullRom,

    /// <summary>
    /// B-spline interpolation.
    /// Produces very smooth curves with local control.
    /// Best for maximum smoothness with less concern for exact control point placement.
    /// </summary>
    BSpline
}
