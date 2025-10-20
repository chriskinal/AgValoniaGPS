using System.Globalization;
using System.Xml.Linq;
using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Display;

namespace AgValoniaGPS.Services.Configuration;

/// <summary>
/// Provides XML-based configuration file I/O using flat structure for legacy compatibility.
/// Maintains backward compatibility with AgOpenGPS XML settings format.
/// </summary>
public class XmlConfigurationProvider : IConfigurationProvider
{
    private readonly ConfigurationConverter _converter;

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlConfigurationProvider"/> class.
    /// </summary>
    public XmlConfigurationProvider()
    {
        _converter = new ConfigurationConverter();
    }

    /// <summary>
    /// Loads application settings from an XML file.
    /// </summary>
    /// <param name="filePath">Full path to the XML settings file</param>
    /// <returns>Loaded application settings</returns>
    /// <exception cref="FileNotFoundException">Thrown if the file does not exist</exception>
    /// <exception cref="InvalidDataException">Thrown if the XML format is invalid</exception>
    public async Task<ApplicationSettings> LoadAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Settings file not found: {filePath}", filePath);
        }

        try
        {
            var xml = await File.ReadAllTextAsync(filePath);
            var doc = XDocument.Parse(xml);
            var root = doc.Root;

            if (root == null || root.Name != "VehicleSettings")
            {
                throw new InvalidDataException($"Invalid XML root element in file: {filePath}");
            }

            return _converter.XmlToJson(root);
        }
        catch (System.Xml.XmlException ex)
        {
            throw new InvalidDataException($"Invalid XML format in file: {filePath}", ex);
        }
    }

    /// <summary>
    /// Saves application settings to an XML file.
    /// </summary>
    /// <param name="filePath">Full path to the XML settings file</param>
    /// <param name="settings">Settings to save</param>
    /// <exception cref="IOException">Thrown if the file cannot be written</exception>
    public async Task SaveAsync(string filePath, ApplicationSettings settings)
    {
        try
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var root = _converter.JsonToXml(settings);
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                root
            );

            // Write to temporary file first for atomic operation
            var tempFilePath = filePath + ".tmp";
            await File.WriteAllTextAsync(tempFilePath, doc.ToString());

            // Replace original file with temp file (atomic on most filesystems)
            File.Move(tempFilePath, filePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new IOException($"Failed to write XML settings file: {filePath}", ex);
        }
    }
}
