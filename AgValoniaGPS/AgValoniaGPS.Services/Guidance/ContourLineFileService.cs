using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using AgValoniaGPS.Models.Guidance;
using PositionModel = AgValoniaGPS.Models.Position;

namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Service for reading and writing Contour Line guidance line files.
/// Supports JSON format with backward compatibility for AgOpenGPS text format.
/// File name: Contour.txt
/// </summary>
public class ContourLineFileService
{
    private const string ContourFileName = "Contour.txt";

    /// <summary>
    /// Load contour line from Contour.txt file.
    /// </summary>
    /// <param name="fieldDirectory">Directory containing the field data.</param>
    /// <returns>ContourLine object if file exists and is valid, null otherwise.</returns>
    public ContourLine? LoadContour(string fieldDirectory)
    {
        var filePath = Path.Combine(fieldDirectory, ContourFileName);

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
                return System.Text.Json.JsonSerializer.Deserialize<ContourLine>(content, options);
            }

            // Fall back to AgOpenGPS text format for backward compatibility
            return ParseAgOpenGPSFormat(content);
        }
        catch (Exception ex)
        {
            // Log error if needed
            Console.WriteLine($"Error loading contour line from {filePath}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Save contour line to Contour.txt file in JSON format.
    /// </summary>
    /// <param name="contour">Contour line to save.</param>
    /// <param name="fieldDirectory">Directory to save the file to.</param>
    public void SaveContour(ContourLine contour, string fieldDirectory)
    {
        if (string.IsNullOrWhiteSpace(fieldDirectory))
        {
            throw new ArgumentException("Field directory must be specified", nameof(fieldDirectory));
        }

        if (!Directory.Exists(fieldDirectory))
        {
            Directory.CreateDirectory(fieldDirectory);
        }

        var filePath = Path.Combine(fieldDirectory, ContourFileName);

        try
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };

            var json = System.Text.Json.JsonSerializer.Serialize(contour, options);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save contour line to {filePath}", ex);
        }
    }

    /// <summary>
    /// Delete contour line file from field directory.
    /// </summary>
    /// <param name="fieldDirectory">Directory containing the file to delete.</param>
    /// <returns>True if file was deleted, false if file didn't exist.</returns>
    public bool DeleteContour(string fieldDirectory)
    {
        var filePath = Path.Combine(fieldDirectory, ContourFileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Parse legacy AgOpenGPS contour text format.
    /// Format example (from Contour.txt):
    /// $Contour
    /// (empty or minimal data in the example - appears to be simple point list)
    /// </summary>
    private ContourLine? ParseAgOpenGPSFormat(string content)
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

            var contour = new ContourLine
            {
                Name = "Imported Contour",
                IsLocked = true, // Assume imported contours are locked
                Points = new List<PositionModel>()
            };

            // Read all points (format: easting,northing,altitude)
            while (line != null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 3)
                    {
                        contour.Points.Add(new PositionModel
                        {
                            Easting = double.Parse(parts[0], CultureInfo.InvariantCulture),
                            Northing = double.Parse(parts[1], CultureInfo.InvariantCulture),
                            Altitude = double.Parse(parts[2], CultureInfo.InvariantCulture)
                        });
                    }
                }

                line = reader.ReadLine();
            }

            // Return null if no points were loaded
            return contour.Points.Count > 0 ? contour : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing AgOpenGPS contour format: {ex.Message}");
            return null;
        }
    }
}
