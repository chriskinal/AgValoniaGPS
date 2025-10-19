using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Section;

namespace AgValoniaGPS.Services.Section;

/// <summary>
/// Service for reading and writing section control configuration files
/// File format: SectionConfig.txt (AgOpenGPS-compatible text format)
/// Thread-safe with async I/O on background thread
/// </summary>
public class SectionControlFileService : ISectionControlFileService
{
    private const string ConfigFileName = "SectionConfig.txt";
    private readonly ISectionConfigurationService _configurationService;

    /// <summary>
    /// Creates a new section control file service
    /// </summary>
    /// <param name="configurationService">Configuration service for validation</param>
    public SectionControlFileService(ISectionConfigurationService configurationService)
    {
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
    }

    /// <summary>
    /// Saves section configuration to SectionConfig.txt in the field directory (async)
    /// </summary>
    /// <param name="fieldPath">Path to the field directory</param>
    /// <param name="configuration">Configuration to save</param>
    public async Task SaveConfigurationAsync(string fieldPath, SectionConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(fieldPath))
            throw new ArgumentException("Field path must be specified", nameof(fieldPath));

        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        if (!configuration.IsValid())
            throw new ArgumentException("Configuration is invalid", nameof(configuration));

        // Ensure directory exists
        if (!Directory.Exists(fieldPath))
        {
            Directory.CreateDirectory(fieldPath);
        }

        var filePath = Path.Combine(fieldPath, ConfigFileName);

        try
        {
            // Write to memory stream first, then write to file atomically
            using var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
            {
                // Write header
                await writer.WriteLineAsync("[SectionControl]");

                // Write section count
                await writer.WriteLineAsync($"SectionCount={configuration.SectionCount}");

                // Write section widths (comma-separated)
                var widthsString = string.Join(",", configuration.SectionWidths.Select(w =>
                    w.ToString("F2", CultureInfo.InvariantCulture)));
                await writer.WriteLineAsync($"SectionWidths={widthsString}");

                // Write timing parameters
                await writer.WriteLineAsync($"TurnOnDelay={configuration.TurnOnDelay.ToString("F2", CultureInfo.InvariantCulture)}");
                await writer.WriteLineAsync($"TurnOffDelay={configuration.TurnOffDelay.ToString("F2", CultureInfo.InvariantCulture)}");

                // Write tolerance and look-ahead parameters
                await writer.WriteLineAsync($"OverlapTolerance={configuration.OverlapTolerance.ToString("F2", CultureInfo.InvariantCulture)}");
                await writer.WriteLineAsync($"LookAheadDistance={configuration.LookAheadDistance.ToString("F2", CultureInfo.InvariantCulture)}");
                await writer.WriteLineAsync($"MinimumSpeed={configuration.MinimumSpeed.ToString("F2", CultureInfo.InvariantCulture)}");

                await writer.FlushAsync();
            }

            // Write memory stream to file atomically
            memoryStream.Position = 0;
            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
            await memoryStream.CopyToAsync(fileStream);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save section configuration to {filePath}", ex);
        }
    }

    /// <summary>
    /// Loads section configuration from SectionConfig.txt in the field directory (async)
    /// </summary>
    /// <param name="fieldPath">Path to the field directory</param>
    /// <returns>Loaded configuration, or null if file doesn't exist or is invalid</returns>
    public async Task<SectionConfiguration?> LoadConfigurationAsync(string fieldPath)
    {
        if (string.IsNullOrWhiteSpace(fieldPath))
            return null;

        var filePath = Path.Combine(fieldPath, ConfigFileName);

        if (!File.Exists(filePath))
            return null;

        try
        {
            string content;
            await using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            using (var reader = new StreamReader(fileStream, Encoding.UTF8))
            {
                content = await reader.ReadToEndAsync();
            }

            return ParseConfiguration(content);
        }
        catch (Exception ex)
        {
            // Backup corrupted file
            await BackupCorruptedFileAsync(filePath);
            Console.WriteLine($"Error loading section configuration from {filePath}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Saves section configuration synchronously
    /// </summary>
    public void SaveConfiguration(string fieldPath, SectionConfiguration configuration)
    {
        SaveConfigurationAsync(fieldPath, configuration).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Loads section configuration synchronously
    /// </summary>
    public SectionConfiguration? LoadConfiguration(string fieldPath)
    {
        return LoadConfigurationAsync(fieldPath).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Parses configuration from text content
    /// Format:
    /// [SectionControl]
    /// SectionCount=5
    /// SectionWidths=2.5,2.5,2.5,2.5,2.5
    /// TurnOnDelay=2.0
    /// TurnOffDelay=1.5
    /// OverlapTolerance=10
    /// LookAheadDistance=3.0
    /// MinimumSpeed=0.1
    /// </summary>
    private SectionConfiguration? ParseConfiguration(string content)
    {
        try
        {
            var config = new SectionConfiguration();
            using var reader = new StringReader(content);
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                // Skip empty lines and section headers
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("[") || line.StartsWith("#"))
                    continue;

                // Parse key=value pairs
                var parts = line.Split('=', 2);
                if (parts.Length != 2)
                    continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                switch (key)
                {
                    case "SectionCount":
                        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int count))
                            config.SectionCount = count;
                        break;

                    case "SectionWidths":
                        var widthStrings = value.Split(',');
                        var widths = new double[widthStrings.Length];
                        for (int i = 0; i < widthStrings.Length; i++)
                        {
                            if (double.TryParse(widthStrings[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double width))
                                widths[i] = width;
                        }
                        config.SectionWidths = widths;
                        break;

                    case "TurnOnDelay":
                        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double turnOnDelay))
                            config.TurnOnDelay = turnOnDelay;
                        break;

                    case "TurnOffDelay":
                        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double turnOffDelay))
                            config.TurnOffDelay = turnOffDelay;
                        break;

                    case "OverlapTolerance":
                        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double overlapTolerance))
                            config.OverlapTolerance = overlapTolerance;
                        break;

                    case "LookAheadDistance":
                        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double lookAhead))
                            config.LookAheadDistance = lookAhead;
                        break;

                    case "MinimumSpeed":
                        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double minSpeed))
                            config.MinimumSpeed = minSpeed;
                        break;
                }
            }

            // Validate configuration before returning
            return config.IsValid() ? config : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing section configuration: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Backs up a corrupted file with timestamp
    /// </summary>
    private async Task BackupCorruptedFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return;

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupPath = $"{filePath}.corrupt_{timestamp}.bak";

            await using var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            await using var destStream = new FileStream(backupPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
            await sourceStream.CopyToAsync(destStream);

            Console.WriteLine($"Backed up corrupted file to: {backupPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to backup corrupted file: {ex.Message}");
        }
    }
}
