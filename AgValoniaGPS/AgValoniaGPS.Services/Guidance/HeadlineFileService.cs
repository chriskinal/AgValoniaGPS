using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Service for reading and writing headline guidance files.
/// Supports AgOpenGPS Headlines.txt format for backward compatibility.
/// File name: Headlines.txt
/// Format:
/// $HeadLines
/// Name
/// MoveDistance
/// Mode
/// APointIndex
/// PointCount
/// Easting,Northing,Heading
/// ...
/// </summary>
public class HeadlineFileService
{
    private const string HeadlineFileName = "Headlines.txt";
    private const string FileHeader = "$HeadLines";

    /// <summary>
    /// Load all headlines from Headlines.txt file.
    /// </summary>
    /// <param name="fieldDirectory">Directory containing the field data.</param>
    /// <returns>List of headlines. Empty list if file doesn't exist or has no valid headlines.</returns>
    public List<Headline> LoadHeadlines(string fieldDirectory)
    {
        var headlines = new List<Headline>();
        var filePath = Path.Combine(fieldDirectory, HeadlineFileName);

        if (!File.Exists(filePath))
        {
            return headlines;
        }

        try
        {
            using var reader = new StreamReader(filePath);

            // Read header (optional)
            var line = reader.ReadLine();
            if (line != null && !line.StartsWith("$"))
            {
                // No header, reset to beginning
                reader.BaseStream.Position = 0;
                reader.DiscardBufferedData();
            }

            int headlineId = 1;

            while (!reader.EndOfStream)
            {
                var headline = ReadSingleHeadline(reader, headlineId);
                if (headline != null && headline.Validate())
                {
                    headlines.Add(headline);
                    headlineId++;
                }
            }
        }
        catch (Exception ex)
        {
            // Log error if needed
            Console.WriteLine($"Error loading headlines from {filePath}: {ex.Message}");
        }

        return headlines;
    }

    /// <summary>
    /// Save all headlines to Headlines.txt file.
    /// </summary>
    /// <param name="headlines">List of headlines to save.</param>
    /// <param name="fieldDirectory">Directory to save the file to.</param>
    public void SaveHeadlines(List<Headline> headlines, string fieldDirectory)
    {
        if (string.IsNullOrWhiteSpace(fieldDirectory))
        {
            throw new ArgumentException("Field directory must be specified", nameof(fieldDirectory));
        }

        if (!Directory.Exists(fieldDirectory))
        {
            Directory.CreateDirectory(fieldDirectory);
        }

        var filePath = Path.Combine(fieldDirectory, HeadlineFileName);

        try
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

            // Write header
            writer.WriteLine(FileHeader);

            if (headlines == null || headlines.Count == 0)
            {
                return;
            }

            // Write each headline
            foreach (var headline in headlines)
            {
                WriteSingleHeadline(writer, headline);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save headlines to {filePath}", ex);
        }
    }

    /// <summary>
    /// Delete headlines file from field directory.
    /// </summary>
    /// <param name="fieldDirectory">Directory containing the file to delete.</param>
    /// <returns>True if file was deleted, false if file didn't exist.</returns>
    public bool DeleteHeadlines(string fieldDirectory)
    {
        var filePath = Path.Combine(fieldDirectory, HeadlineFileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if headlines file exists.
    /// </summary>
    /// <param name="fieldDirectory">Directory to check.</param>
    /// <returns>True if file exists, false otherwise.</returns>
    public bool HeadlinesExist(string fieldDirectory)
    {
        var filePath = Path.Combine(fieldDirectory, HeadlineFileName);
        return File.Exists(filePath);
    }

    #region Private Helper Methods

    /// <summary>
    /// Read a single headline from the file stream.
    /// </summary>
    private Headline? ReadSingleHeadline(StreamReader reader, int id)
    {
        try
        {
            // Read name
            var name = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            // Read move distance
            var moveDistanceLine = reader.ReadLine();
            if (moveDistanceLine == null)
            {
                return null;
            }
            double moveDistance = double.Parse(moveDistanceLine, CultureInfo.InvariantCulture);

            // Read mode
            var modeLine = reader.ReadLine();
            if (modeLine == null)
            {
                return null;
            }
            int mode = int.Parse(modeLine, CultureInfo.InvariantCulture);

            // Read A-point index
            var aPointLine = reader.ReadLine();
            if (aPointLine == null)
            {
                return null;
            }
            int aPointIndex = int.Parse(aPointLine, CultureInfo.InvariantCulture);

            // Read point count
            var pointCountLine = reader.ReadLine();
            if (pointCountLine == null)
            {
                return null;
            }
            int pointCount = int.Parse(pointCountLine, CultureInfo.InvariantCulture);

            // Read track points
            var trackPoints = new List<Position>();
            for (int i = 0; i < pointCount && !reader.EndOfStream; i++)
            {
                var pointLine = reader.ReadLine();
                if (pointLine == null)
                {
                    break;
                }

                var parts = pointLine.Split(',');
                if (parts.Length >= 3)
                {
                    if (double.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double easting) &&
                        double.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double northing) &&
                        double.TryParse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double heading))
                    {
                        trackPoints.Add(new Position
                        {
                            Easting = easting,
                            Northing = northing,
                            Altitude = 0.0 // Legacy format doesn't store altitude
                        });
                    }
                }
            }

            // Must have at least 4 points to be valid
            if (trackPoints.Count < 4)
            {
                return null;
            }

            // Calculate heading from track points
            double averageHeading = CalculateAverageHeading(trackPoints);

            // Create headline
            var headline = new Headline
            {
                Id = id,
                Name = name.Trim(),
                TrackPoints = trackPoints,
                MoveDistance = moveDistance,
                Mode = mode,
                APointIndex = Math.Max(0, Math.Min(aPointIndex, trackPoints.Count - 1)), // Ensure valid index
                Heading = averageHeading,
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            };

            return headline;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading headline: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Write a single headline to the file stream.
    /// </summary>
    private void WriteSingleHeadline(StreamWriter writer, Headline headline)
    {
        // Write name
        writer.WriteLine(headline.Name);

        // Write move distance
        writer.WriteLine(headline.MoveDistance.ToString("F3", CultureInfo.InvariantCulture));

        // Write mode
        writer.WriteLine(headline.Mode.ToString(CultureInfo.InvariantCulture));

        // Write A-point index
        writer.WriteLine(headline.APointIndex.ToString(CultureInfo.InvariantCulture));

        // Write point count
        writer.WriteLine(headline.TrackPoints.Count.ToString(CultureInfo.InvariantCulture));

        // Write track points
        // Legacy format uses local heading for each segment (stored in 3rd column but not actually used by headline logic)
        for (int i = 0; i < headline.TrackPoints.Count; i++)
        {
            var point = headline.TrackPoints[i];

            // Calculate local heading for this segment
            double localHeading = 0.0;
            if (i < headline.TrackPoints.Count - 1)
            {
                double dx = headline.TrackPoints[i + 1].Easting - point.Easting;
                double dy = headline.TrackPoints[i + 1].Northing - point.Northing;
                localHeading = Math.Atan2(dx, dy);
            }
            else if (i > 0)
            {
                double dx = point.Easting - headline.TrackPoints[i - 1].Easting;
                double dy = point.Northing - headline.TrackPoints[i - 1].Northing;
                localHeading = Math.Atan2(dx, dy);
            }

            // Format: Easting,Northing,Heading (with spaces after commas for readability)
            writer.WriteLine($"{FormatDouble(point.Easting, 3)} , {FormatDouble(point.Northing, 3)} , {FormatDouble(localHeading, 5)}");
        }
    }

    /// <summary>
    /// Calculate average heading from a path of points.
    /// </summary>
    private double CalculateAverageHeading(List<Position> points)
    {
        if (points == null || points.Count < 2)
        {
            return 0.0;
        }

        double sumX = 0.0;
        double sumY = 0.0;

        for (int i = 0; i < points.Count - 1; i++)
        {
            double dx = points[i + 1].Easting - points[i].Easting;
            double dy = points[i + 1].Northing - points[i].Northing;
            double length = Math.Sqrt(dx * dx + dy * dy);

            if (length > 0.001)
            {
                sumX += dx / length;
                sumY += dy / length;
            }
        }

        return Math.Atan2(sumX, sumY);
    }

    /// <summary>
    /// Format double value with specified decimal places using invariant culture.
    /// </summary>
    private string FormatDouble(double value, int decimalPlaces)
    {
        return value.ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture);
    }

    #endregion
}
