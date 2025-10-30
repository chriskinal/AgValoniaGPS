using AgValoniaGPS.Models.FieldOperations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Implementation of recorded path file I/O service.
/// Compatible with legacy AgOpenGPS .rec file format.
/// </summary>
public class RecordedPathFileService : IRecordedPathFileService
{
    private const string DefaultFileName = "RecPath.txt";
    private const string FileExtension = ".rec";
    private const string FileHeader = "$RecPath";

    public async Task<bool> SavePathAsync(RecordedPath path, string fieldDirectory, string? fileName = null)
    {
        if (path == null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (string.IsNullOrWhiteSpace(fieldDirectory))
        {
            throw new ArgumentException("Field directory cannot be empty", nameof(fieldDirectory));
        }

        // Ensure directory exists
        Directory.CreateDirectory(fieldDirectory);

        fileName ??= DefaultFileName;
        var filePath = Path.Combine(fieldDirectory, fileName);

        try
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // Write header
                await writer.WriteLineAsync(FileHeader);

                // Write point count
                await writer.WriteLineAsync(path.PointCount.ToString(CultureInfo.InvariantCulture));

                // Write each point
                foreach (var point in path.Points)
                {
                    string line = string.Format(CultureInfo.InvariantCulture,
                        "{0:F3},{1:F3},{2:F3},{3:F1},{4}",
                        point.Easting,
                        point.Northing,
                        point.Heading,
                        point.Speed,
                        point.IsSectionAutoOn);

                    await writer.WriteLineAsync(line);
                }
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<RecordedPath?> LoadPathAsync(string fieldDirectory, string? fileName = null)
    {
        if (string.IsNullOrWhiteSpace(fieldDirectory))
        {
            throw new ArgumentException("Field directory cannot be empty", nameof(fieldDirectory));
        }

        fileName ??= DefaultFileName;
        var filePath = Path.Combine(fieldDirectory, fileName);

        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {
                // Read and validate header
                string? header = await reader.ReadLineAsync();
                if (header == null)
                {
                    return null;
                }

                // Read point count
                string? countLine = await reader.ReadLineAsync();
                if (countLine == null || !int.TryParse(countLine.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int pointCount))
                {
                    return null;
                }

                // Create path object
                string pathName = Path.GetFileNameWithoutExtension(fileName);
                var path = new RecordedPath(pathName);

                // Read all points
                for (int i = 0; i < pointCount && !reader.EndOfStream; i++)
                {
                    string? line = await reader.ReadLineAsync();
                    if (line == null)
                    {
                        break;
                    }

                    string[] parts = line.Split(',');
                    if (parts.Length < 5)
                    {
                        continue; // Skip malformed lines
                    }

                    try
                    {
                        double easting = double.Parse(parts[0], CultureInfo.InvariantCulture);
                        double northing = double.Parse(parts[1], CultureInfo.InvariantCulture);
                        double heading = double.Parse(parts[2], CultureInfo.InvariantCulture);
                        double speed = double.Parse(parts[3], CultureInfo.InvariantCulture);
                        bool isSectionAutoOn = bool.Parse(parts[4]);

                        path.AddPoint(easting, northing, heading, speed, isSectionAutoOn);
                    }
                    catch (FormatException)
                    {
                        // Skip invalid points
                        continue;
                    }
                }

                return path;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<List<string>> ListPathsAsync(string fieldDirectory)
    {
        if (string.IsNullOrWhiteSpace(fieldDirectory))
        {
            throw new ArgumentException("Field directory cannot be empty", nameof(fieldDirectory));
        }

        if (!Directory.Exists(fieldDirectory))
        {
            return new List<string>();
        }

        return await Task.Run(() =>
        {
            var files = Directory.GetFiles(fieldDirectory, "*" + FileExtension);
            return files.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
        });
    }

    public async Task<bool> DeletePathAsync(string fieldDirectory, string pathName)
    {
        if (string.IsNullOrWhiteSpace(fieldDirectory))
        {
            throw new ArgumentException("Field directory cannot be empty", nameof(fieldDirectory));
        }

        if (string.IsNullOrWhiteSpace(pathName))
        {
            throw new ArgumentException("Path name cannot be empty", nameof(pathName));
        }

        string filePath = Path.Combine(fieldDirectory, pathName + FileExtension);

        try
        {
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
                return true;
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool PathExists(string fieldDirectory, string? fileName = null)
    {
        if (string.IsNullOrWhiteSpace(fieldDirectory))
        {
            return false;
        }

        fileName ??= DefaultFileName;
        string filePath = Path.Combine(fieldDirectory, fileName);

        return File.Exists(filePath);
    }
}
