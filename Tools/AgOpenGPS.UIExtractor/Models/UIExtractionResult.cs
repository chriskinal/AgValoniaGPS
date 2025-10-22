namespace AgOpenGPS.UIExtractor.Models;

/// <summary>
/// Complete UI extraction result containing all forms
/// </summary>
public class UIExtractionResult
{
    public string Version { get; set; } = "1.0";
    public string Source { get; set; } = "AgOpenGPS v6.x";
    public DateTime ExtractedDate { get; set; } = DateTime.Now;
    public Dictionary<string, FormInfo> Forms { get; set; } = new();
    public ExtractionStatistics Statistics { get; set; } = new();
    public NavigationGraph? NavigationGraph { get; set; }
    public Dictionary<string, FormStructure> FormStructures { get; set; } = new();
}

public class ExtractionStatistics
{
    public int TotalForms { get; set; }
    public int TotalMenuItems { get; set; }
    public int TotalToolbarButtons { get; set; }
    public int TotalControls { get; set; }
    public int TotalEventHandlers { get; set; }
    public int TotalStateVariables { get; set; }
    public int TotalKeyboardShortcuts { get; set; }
    public int TotalImages { get; set; }
    public int TotalStringResources { get; set; }
}
