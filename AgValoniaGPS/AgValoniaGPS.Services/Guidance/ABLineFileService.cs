using System;
using System.Globalization;
using System.IO;
using System.Text;
using AgValoniaGPS.Models.Guidance;
using PositionModel = AgValoniaGPS.Models.Position;

namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Service for reading and writing AB Line guidance line files.
/// Supports JSON format with backward compatibility for AgOpenGPS text format.
/// File name: ABLine.txt
/// </summary>
public class ABLineFileService
{
    private const string ABLineFileName = "ABLine.txt";

    /// <summary>
    /// Load AB line from ABLine.txt file.
    /// </summary>
    /// <param name="fieldDirectory">Directory containing the field data.</param>
    /// <returns>ABLine object if file exists and is valid, null otherwise.</returns>
    public ABLine? LoadABLine(string fieldDirectory)
    {
        var filePath = Path.Combine(fieldDirectory, ABLineFileName);

        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            using var reader = new StreamReader(filePath);
            var content = reader.ReadToEnd();

            // Try JSON format first
            if (content.TrimStart().StartsWith("{"))
            {
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                };
                return System.Text.Json.JsonSerializer.Deserialize<ABLine>(content, options);
            }

            // Fall back to AgOpenGPS text format for backward compatibility
            return ParseAgOpenGPSFormat(content);
        }
        catch (Exception ex)
        {
            // Log error if needed
            Console.WriteLine($"Error loading AB line from {filePath}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Save AB line to ABLine.txt file in JSON format.
    /// </summary>
    /// <param name="abLine">AB line to save.</param>
    /// <param name="fieldDirectory">Directory to save the file to.</param>
    public void SaveABLine(ABLine abLine, string fieldDirectory)
    {
        if (string.IsNullOrWhiteSpace(fieldDirectory))
        {
            throw new ArgumentException("Field directory must be specified", nameof(fieldDirectory));
        }

        if (!Directory.Exists(fieldDirectory))
        {
            Directory.CreateDirectory(fieldDirectory);
        }

        var filePath = Path.Combine(fieldDirectory, ABLineFileName);

        try
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };

            var json = System.Text.Json.JsonSerializer.Serialize(abLine, options);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save AB line to {filePath}", ex);
        }
    }

    /// <summary>
    /// Delete AB line file from field directory.
    /// </summary>
    /// <param name="fieldDirectory">Directory containing the file to delete.</param>
    /// <returns>True if file was deleted, false if file didn't exist.</returns>
    public bool DeleteABLine(string fieldDirectory)
    {
        var filePath = Path.Combine(fieldDirectory, ABLineFileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Parse legacy AgOpenGPS AB line text format.
    /// Format example (from ABLines.txt):
    /// $ABLine
    /// Name
    /// PointA.Easting,PointA.Northing,PointA.Altitude
    /// PointB.Easting,PointB.Northing,PointB.Altitude
    /// Heading
    /// </summary>
    private ABLine? ParseAgOpenGPSFormat(string content)
    {
        try
        {
            using var reader = new StringReader(content);
            var line = reader.ReadLine();

            // Skip header if present
            if (line != null && line.StartsWith("$"))
            {
                line = reader.ReadLine();
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            var abLine = new ABLine
            {
                Name = line.Trim()
            };

            // Read Point A
            line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            var pointAParts = line.Split(',');
            if (pointAParts.Length >= 3)
            {
                abLine.PointA = new PositionModel
                {
                    Easting = double.Parse(pointAParts[0], CultureInfo.InvariantCulture),
                    Northing = double.Parse(pointAParts[1], CultureInfo.InvariantCulture),
                    Altitude = double.Parse(pointAParts[2], CultureInfo.InvariantCulture)
                };
            }

            // Read Point B
            line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            var pointBParts = line.Split(',');
            if (pointBParts.Length >= 3)
            {
                abLine.PointB = new PositionModel
                {
                    Easting = double.Parse(pointBParts[0], CultureInfo.InvariantCulture),
                    Northing = double.Parse(pointBParts[1], CultureInfo.InvariantCulture),
                    Altitude = double.Parse(pointBParts[2], CultureInfo.InvariantCulture)
                };
            }

            // Read Heading
            line = reader.ReadLine();
            if (!string.IsNullOrWhiteSpace(line))
            {
                abLine.Heading = double.Parse(line, CultureInfo.InvariantCulture);
            }

            // Read Nudge Offset (optional, may not exist in older files)
            line = reader.ReadLine();
            if (!string.IsNullOrWhiteSpace(line))
            {
                abLine.NudgeOffset = double.Parse(line, CultureInfo.InvariantCulture);
            }

            return abLine;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing AgOpenGPS AB line format: {ex.Message}");
            return null;
        }
    }
}
