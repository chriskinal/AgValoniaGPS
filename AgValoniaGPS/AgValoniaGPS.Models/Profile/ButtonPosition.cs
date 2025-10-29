namespace AgValoniaGPS.Models.Profile;

/// <summary>
/// Represents the saved position and visibility state of a floating button.
/// </summary>
public class ButtonPosition
{
    /// <summary>
    /// Gets or sets the unique identifier for the button.
    /// </summary>
    public string ButtonId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the X offset from the button's base position.
    /// </summary>
    public double OffsetX { get; set; }

    /// <summary>
    /// Gets or sets the Y offset from the button's base position.
    /// </summary>
    public double OffsetY { get; set; }

    /// <summary>
    /// Gets or sets whether the button is visible.
    /// </summary>
    public bool IsVisible { get; set; } = true;
}
