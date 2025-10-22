namespace AgOpenGPS.UIExtractor.Models;

/// <summary>
/// Represents a state variable that controls UI element visibility or behavior
/// </summary>
public class StateVariable
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<string> AffectsControls { get; set; } = new();
}
