namespace AgOpenGPS.UIExtractor.Models;

/// <summary>
/// Base class for all UI elements extracted from Designer.cs files
/// </summary>
public class UIElement
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Text { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
    public List<UIElement> Children { get; set; } = new();
}
