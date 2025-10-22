using AgOpenGPS.UIExtractor.Models;

namespace AgOpenGPS.UIExtractor.Parsers;

/// <summary>
/// Parses .resx files to extract icons, images, and localized strings
/// </summary>
public class ResourceParser
{
    private readonly string _outputImageDir;

    public ResourceParser(string outputImageDir)
    {
        _outputImageDir = outputImageDir;
        Directory.CreateDirectory(_outputImageDir);
    }

    public FormResources ParseResourceFile(string resxPath, string formName)
    {
        var resources = new FormResources
        {
            FormName = formName
        };

        if (!File.Exists(resxPath))
        {
            return resources;
        }

        try
        {
            // Parse .resx as XML (simplified approach that works cross-platform)
            var xml = System.Xml.Linq.XDocument.Load(resxPath);
            var ns = xml.Root?.Name.Namespace ?? System.Xml.Linq.XNamespace.None;

            foreach (var dataElement in xml.Descendants("data"))
            {
                var key = dataElement.Attribute("name")?.Value ?? string.Empty;
                var typeAttr = dataElement.Attribute("type")?.Value ?? string.Empty;
                var mimeTypeAttr = dataElement.Attribute("mimetype")?.Value ?? string.Empty;

                // Check if it's an image resource (has mimetype or type contains Image/Bitmap/Icon)
                if (!string.IsNullOrEmpty(mimeTypeAttr) ||
                    typeAttr.Contains("Image") ||
                    typeAttr.Contains("Bitmap") ||
                    typeAttr.Contains("Icon"))
                {
                    resources.Images.Add(new ImageResource
                    {
                        Name = key,
                        OriginalFormat = typeAttr.Split(',').FirstOrDefault()?.Trim() ?? "Unknown",
                        OutputPath = $"{formName}_{SanitizeFileName(key)}.png",
                        Width = 0,  // Unknown without loading
                        Height = 0  // Unknown without loading
                    });
                }
                // Otherwise treat as string resource
                else
                {
                    var valueElement = dataElement.Element("value");
                    if (valueElement != null)
                    {
                        resources.Strings.Add(new StringResource
                        {
                            Key = key,
                            Value = valueElement.Value
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to parse {resxPath}: {ex.Message}");
        }

        return resources;
    }

    private string SanitizeFileName(string fileName)
    {
        // Remove invalid characters from file names
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Where(c => !invalid.Contains(c)).ToArray());

        // Replace dots and spaces with underscores
        sanitized = sanitized.Replace('.', '_').Replace(' ', '_');

        return sanitized;
    }

    /// <summary>
    /// Find all .resx files associated with a form
    /// </summary>
    public List<string> FindResxFiles(string designerPath)
    {
        var resxFiles = new List<string>();
        var directory = Path.GetDirectoryName(designerPath);
        if (directory == null)
            return resxFiles;

        var baseName = Path.GetFileNameWithoutExtension(designerPath).Replace(".Designer", "");

        // Look for main .resx file
        var mainResx = Path.Combine(directory, $"{baseName}.resx");
        if (File.Exists(mainResx))
        {
            resxFiles.Add(mainResx);
        }

        // Look for localized .resx files (e.g., FormGPS.es.resx, FormGPS.de.resx)
        var localizedFiles = Directory.GetFiles(directory, $"{baseName}.*.resx", SearchOption.TopDirectoryOnly);
        resxFiles.AddRange(localizedFiles);

        return resxFiles;
    }
}
