namespace AgValoniaGPS.Models.Guidance;

/// <summary>
/// Enum representing the type of guidance line for file operations.
/// </summary>
public enum GuidanceLineType
{
    /// <summary>
    /// Straight AB guidance line.
    /// </summary>
    ABLine = 0,

    /// <summary>
    /// Curved guidance line.
    /// </summary>
    CurveLine = 1,

    /// <summary>
    /// Contour guidance line.
    /// </summary>
    Contour = 2
}
