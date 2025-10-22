namespace AgOpenGPS.UIExtractor.Models;

/// <summary>
/// Represents a navigation relationship between forms
/// </summary>
public class FormNavigation
{
    public string SourceForm { get; set; } = string.Empty;
    public string TargetForm { get; set; } = string.Empty;
    public string TriggerEvent { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}

/// <summary>
/// Represents a control grouping pattern (tabs, panels, etc.)
/// </summary>
public class ControlGroup
{
    public string GroupType { get; set; } = string.Empty; // Tab, Panel, GroupBox, etc.
    public string Name { get; set; } = string.Empty;
    public int ControlCount { get; set; }
    public List<string> ChildControls { get; set; } = new();
}

/// <summary>
/// Represents hierarchical structure of a complex form
/// </summary>
public class FormStructure
{
    public string FormName { get; set; } = string.Empty;
    public List<ControlGroup> Groups { get; set; } = new();
    public int MaxDepth { get; set; }
    public Dictionary<string, int> ControlTypeDistribution { get; set; } = new();
}

/// <summary>
/// Navigation graph for the entire application
/// </summary>
public class NavigationGraph
{
    public List<FormNavigation> Navigations { get; set; } = new();
    public Dictionary<string, List<string>> FormChildren { get; set; } = new(); // Form -> List of forms it opens
    public Dictionary<string, List<string>> FormParents { get; set; } = new();  // Form -> List of forms that open it
    public List<string> EntryPoints { get; set; } = new(); // Forms with no parents (likely main windows)
}
