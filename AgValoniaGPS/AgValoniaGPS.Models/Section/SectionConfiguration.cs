namespace AgValoniaGPS.Models.Section;

/// <summary>
/// Represents the configuration for section control including section counts, widths, delays, and tolerances
/// </summary>
public class SectionConfiguration
{
    private int _sectionCount;
    private double[] _sectionWidths = Array.Empty<double>();

    /// <summary>
    /// Number of sections (1-31)
    /// </summary>
    public int SectionCount
    {
        get => _sectionCount;
        set
        {
            if (value < 1 || value > 31)
                throw new ArgumentOutOfRangeException(nameof(value), "Section count must be between 1 and 31");
            _sectionCount = value;
        }
    }

    /// <summary>
    /// Width of each section in meters (must be positive, 0.1m - 20m)
    /// </summary>
    public double[] SectionWidths
    {
        get => _sectionWidths;
        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            foreach (var width in value)
            {
                if (width < 0.1 || width > 20.0)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        $"Section widths must be between 0.1m and 20m. Invalid width: {width}m");
            }

            _sectionWidths = value;
        }
    }

    /// <summary>
    /// Total width of all sections combined in meters
    /// </summary>
    public double TotalWidth => SectionWidths?.Sum() ?? 0.0;

    /// <summary>
    /// Turn-on delay in seconds (1.0-15.0 seconds)
    /// </summary>
    public double TurnOnDelay { get; set; }

    /// <summary>
    /// Turn-off delay in seconds (1.0-15.0 seconds)
    /// </summary>
    public double TurnOffDelay { get; set; }

    /// <summary>
    /// Overlap tolerance as percentage of section width (0-50%)
    /// </summary>
    public double OverlapTolerance { get; set; }

    /// <summary>
    /// Look-ahead distance for boundary anticipation in meters (0.5-10m)
    /// </summary>
    public double LookAheadDistance { get; set; }

    /// <summary>
    /// Minimum speed threshold in meters per second
    /// </summary>
    public double MinimumSpeed { get; set; }

    /// <summary>
    /// Creates a new section configuration with default values
    /// </summary>
    public SectionConfiguration()
    {
        _sectionCount = 5;
        _sectionWidths = new double[5] { 2.5, 2.5, 2.5, 2.5, 2.5 };
        TurnOnDelay = 2.0;
        TurnOffDelay = 1.5;
        OverlapTolerance = 10.0;
        LookAheadDistance = 3.0;
        MinimumSpeed = 0.1;
    }

    /// <summary>
    /// Creates a section configuration with specified values
    /// </summary>
    /// <param name="sectionCount">Number of sections (1-31)</param>
    /// <param name="sectionWidths">Width of each section in meters</param>
    public SectionConfiguration(int sectionCount, double[] sectionWidths)
    {
        SectionCount = sectionCount; // Uses validation in setter
        SectionWidths = sectionWidths; // Uses validation in setter
        TurnOnDelay = 2.0;
        TurnOffDelay = 1.5;
        OverlapTolerance = 10.0;
        LookAheadDistance = 3.0;
        MinimumSpeed = 0.1;
    }

    /// <summary>
    /// Validates the configuration and returns whether it is valid
    /// </summary>
    public bool IsValid()
    {
        try
        {
            if (_sectionCount < 1 || _sectionCount > 31)
                return false;

            if (_sectionWidths == null || _sectionWidths.Length != _sectionCount)
                return false;

            foreach (var width in _sectionWidths)
            {
                if (width < 0.1 || width > 20.0)
                    return false;
            }

            if (TurnOnDelay < 1.0 || TurnOnDelay > 15.0)
                return false;

            if (TurnOffDelay < 1.0 || TurnOffDelay > 15.0)
                return false;

            if (OverlapTolerance < 0 || OverlapTolerance > 50)
                return false;

            if (LookAheadDistance < 0.5 || LookAheadDistance > 10.0)
                return false;

            if (MinimumSpeed < 0)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }
}
