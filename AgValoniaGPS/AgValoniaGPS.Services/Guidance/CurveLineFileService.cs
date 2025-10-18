using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using AgValoniaGPS.Models.Guidance;
using PositionModel = AgValoniaGPS.Models.Position;

namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Service for reading and writing Curve Line guidance line files.
/// Supports JSON format with backward compatibility for AgOpenGPS text format.
/// File name: CurveLine.txt
/// </summary>
public class CurveLineFileService
{
    private const string CurveLineFileName = "CurveLine.txt";

    /// <summary>
    /// Load curve line from CurveLine.txt file.
    /// </summary>
    /// <param name="fieldDirectory">Directory containing the field data.</param>
    /// <returns>CurveLine object if file exists and is valid, null otherwise.</returns>
    public CurveLine? LoadCurveLine(string fieldDirectory)
    {
        var filePath = Path.Combine(fieldDirectory, CurveLineFileName);

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
                return System.Text.Json.JsonSerializer.Deserialize<CurveLine>(content, options);
            }

            // Fall back to AgOpenGPS text format for backward compatibility
            return ParseAgOpenGPSFormat(content);
        }
        catch (Exception ex)
        {
            // Log error if needed
            Console.WriteLine($"Error loading curve line from {filePath}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Save curve line to CurveLine.txt file in JSON format.
    /// </summary>
    /// <param name="curveLine">Curve line to save.</param>
    /// <param name="fieldDirectory">Directory to save the file to.</param>
    public void SaveCurveLine(CurveLine curveLine, string fieldDirectory)
    {
        if (string.IsNullOrWhiteSpace(fieldDirectory))
        {
            throw new ArgumentException("Field directory must be specified", nameof(fieldDirectory));
        }

        if (!Directory.Exists(fieldDirectory))
        {
            Directory.CreateDirectory(fieldDirectory);
        }

        var filePath = Path.Combine(fieldDirectory, CurveLineFileName);

        try
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };

            var json = System.Text.Json.JsonSerializer.Serialize(curveLine, options);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save curve line to {filePath}", ex);
        }
    }

    /// <summary>
    /// Delete curve line file from field directory.
    /// </summary>
    /// <param name="fieldDirectory">Directory containing the file to delete.</param>
    /// <returns>True if file was deleted, false if file didn't exist.</returns>
    public bool DeleteCurveLine(string fieldDirectory)
    {
        var filePath = Path.Combine(fieldDirectory, CurveLineFileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Parse legacy AgOpenGPS curve line text format.
    /// Format example (from CurveLines.txt):
    /// $CurveLines
    /// Cu 275.4°
    /// 4.80700069562968
    /// 660 (point count)
    /// -17.708,63.64,1.7319
    /// -16.721,63.48,1.7319
    /// ...
    /// </summary>
    private CurveLine? ParseAgOpenGPSFormat(string content)
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

            var curveLine = new CurveLine
            {
                // Parse name/heading from first line (e.g., "Cu 275.4°")
                Name = line.Trim()
            };

            // Skip the numeric value line (appears to be some parameter)
            line = reader.ReadLine();

            // Read point count
            line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            if (!int.TryParse(line, out int pointCount))
            {
                return null;
            }

            // Read all points
            curveLine.Points = new List<PositionModel>();
            for (int i = 0; i < pointCount; i++)
            {
                line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                var parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    curveLine.Points.Add(new PositionModel
                    {
                        Easting = double.Parse(parts[0], CultureInfo.InvariantCulture),
                        Northing = double.Parse(parts[1], CultureInfo.InvariantCulture),
                        Altitude = double.Parse(parts[2], CultureInfo.InvariantCulture)
                    });
                }
            }

            return curveLine;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing AgOpenGPS curve line format: {ex.Message}");
            return null;
        }
    }
}
