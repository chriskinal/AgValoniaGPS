namespace AgOpenGPS.UIExtractor.Models;

/// <summary>
/// Represents a complete Windows Form with all its UI elements
/// </summary>
public class FormInfo
{
    public string FormName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? Title { get; set; }
    public Size? Size { get; set; }
    public List<UIElement> MenuItems { get; set; } = new();
    public List<UIElement> ToolbarButtons { get; set; } = new();
    public List<UIElement> Controls { get; set; } = new();
    public Dictionary<string, string> EventHandlers { get; set; } = new();
    public Dictionary<string, StateVariable> StateVariables { get; set; } = new();
    public Dictionary<string, string> KeyboardShortcuts { get; set; } = new();
    public FormResources? Resources { get; set; }
}

public class Size
{
    public int Width { get; set; }
    public int Height { get; set; }
}
