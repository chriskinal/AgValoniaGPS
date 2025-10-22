namespace AgOpenGPS.UIExtractor.Models;

/// <summary>
/// Represents an image resource extracted from .resx files
/// </summary>
public class ImageResource
{
    public string Name { get; set; } = string.Empty;
    public string OriginalFormat { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
}

/// <summary>
/// Represents a string resource extracted from .resx files
/// </summary>
public class StringResource
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Comment { get; set; }
}

/// <summary>
/// Collection of resources for a form
/// </summary>
public class FormResources
{
    public string FormName { get; set; } = string.Empty;
    public List<ImageResource> Images { get; set; } = new();
    public List<StringResource> Strings { get; set; } = new();
    public Dictionary<string, string> OtherResources { get; set; } = new();
}
