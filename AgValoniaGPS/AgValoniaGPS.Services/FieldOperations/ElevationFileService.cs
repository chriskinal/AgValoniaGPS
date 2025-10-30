using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.FieldOperations;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Service for loading and saving elevation data in Elevation.txt format
/// Compatible with legacy AgOpenGPS format
/// Thread-safe implementation
/// </summary>
public class ElevationFileService : IElevationFileService
{
    private const string ElevationFileName = "Elevation.txt";

    public async Task<ElevationGrid> LoadElevationDataAsync(string fieldDirectory)
    {
        if (string.IsNullOrWhiteSpace(fieldDirectory))
        {
            throw new ArgumentException("Field directory cannot be null or empty", nameof(fieldDirectory));
        }

        var filePath = Path.Combine(fieldDirectory, ElevationFileName);
        if (!File.Exists(filePath))
        {
            return new ElevationGrid();
        }

        var grid = new ElevationGrid();

        try
        {
            using var reader = new StreamReader(filePath);

            // Read header lines (timestamp, field info, etc.)
            // Format:
            // Line 1: Timestamp (yyyy-MMMM-dd hh:mm:ss tt)
            // Line 2: $FieldDir
            // Line 3: Elevation
            // Line 4: $Offsets
            // Line 5: 0,0
            // Line 6: Convergence
            // Line 7: 0
            // Line 8: StartFix
            // Line 9: latitude,longitude
            // Line 10: Header (Latitude,Longitude,Elevation,Quality,Easting,Northing,Heading,Roll)
            // Line 11+: Data lines (GridX,GridY,Elevation)

            // Skip header lines and look for resolution
            int lineCount = 0;
            while (!reader.EndOfStream && lineCount < 15)
            {
                var line = await reader.ReadLineAsync();
                if (line == null) break;
                lineCount++;

                // Parse grid resolution if present
                if (line.StartsWith("Resolution:"))
                {
                    var parts = line.Split(':');
                    if (parts.Length >= 2 && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var resolution))
                    {
                        grid.GridResolution = resolution;
                    }
                }

                // Break after header section
                if (line.StartsWith("GridX,GridY,Elevation") || line.Contains("Latitude,Longitude"))
                {
                    break;
                }
            }

            // Read data lines
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Parse data line: GridX,GridY,Elevation
                var parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    if (int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var gridX) &&
                        int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var gridY) &&
                        double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var elevation))
                    {
                        var cell = new GridCell(gridX, gridY);
                        grid.ElevationPoints[cell] = elevation;

                        // Update bounding box
                        var position = new Position2D(gridX * grid.GridResolution, gridY * grid.GridResolution);
                        grid.UpdateBoundingBox(position);
                    }
                }
            }

            grid.UpdateStatistics();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load elevation data from {filePath}", ex);
        }

        return grid;
    }

    public async Task SaveElevationDataAsync(string fieldDirectory, ElevationGrid grid, Position? startFix = null)
    {
        if (string.IsNullOrWhiteSpace(fieldDirectory))
        {
            throw new ArgumentException("Field directory cannot be null or empty", nameof(fieldDirectory));
        }

        if (grid == null)
        {
            throw new ArgumentNullException(nameof(grid));
        }

        if (!Directory.Exists(fieldDirectory))
        {
            Directory.CreateDirectory(fieldDirectory);
        }

        var filePath = Path.Combine(fieldDirectory, ElevationFileName);

        try
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

            // Write header
            await writer.WriteLineAsync(DateTime.Now.ToString("yyyy-MMMM-dd hh:mm:ss tt", CultureInfo.InvariantCulture));
            await writer.WriteLineAsync("$FieldDir");
            await writer.WriteLineAsync("Elevation");
            await writer.WriteLineAsync($"Resolution:{grid.GridResolution.ToString(CultureInfo.InvariantCulture)}");
            await writer.WriteLineAsync("$Offsets");
            await writer.WriteLineAsync("0,0");
            await writer.WriteLineAsync("Convergence");
            await writer.WriteLineAsync("0");
            await writer.WriteLineAsync("StartFix");

            // Write start fix if provided
            if (startFix != null)
            {
                await writer.WriteLineAsync($"{startFix.Latitude.ToString(CultureInfo.InvariantCulture)},{startFix.Longitude.ToString(CultureInfo.InvariantCulture)}");
            }
            else
            {
                await writer.WriteLineAsync("0,0");
            }

            await writer.WriteLineAsync("Latitude,Longitude,Elevation,Quality,Easting,Northing,Heading,Roll");

            // Write elevation data
            foreach (var kvp in grid.ElevationPoints)
            {
                var cell = kvp.Key;
                var elevation = kvp.Value;
                await writer.WriteLineAsync($"{cell.X},{cell.Y},{elevation.ToString("F3", CultureInfo.InvariantCulture)}");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save elevation data to {filePath}", ex);
        }
    }

    public async Task<bool> ValidateElevationFileAsync(string fieldDirectory)
    {
        if (string.IsNullOrWhiteSpace(fieldDirectory))
        {
            return false;
        }

        var filePath = Path.Combine(fieldDirectory, ElevationFileName);
        if (!File.Exists(filePath))
        {
            return false;
        }

        try
        {
            using var reader = new StreamReader(filePath);

            // Check for header lines
            int headerLineCount = 0;
            while (!reader.EndOfStream && headerLineCount < 10)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                {
                    return false;
                }
                headerLineCount++;
            }

            // Must have at least header lines
            if (headerLineCount < 10)
            {
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task ExportElevationDataAsync(string filePath, ElevationGrid grid)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        if (grid == null)
        {
            throw new ArgumentNullException(nameof(grid));
        }

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        try
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

            // Write metadata
            await writer.WriteLineAsync($"# Elevation Grid Export");
            await writer.WriteLineAsync($"# Exported: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            await writer.WriteLineAsync($"# Grid Resolution: {grid.GridResolution:F2} meters");
            await writer.WriteLineAsync($"# Point Count: {grid.PointCount}");
            await writer.WriteLineAsync($"# Min Elevation: {grid.MinElevation:F2}m");
            await writer.WriteLineAsync($"# Max Elevation: {grid.MaxElevation:F2}m");
            await writer.WriteLineAsync($"# Average Elevation: {grid.AverageElevation:F2}m");
            await writer.WriteLineAsync();

            // Write column headers
            await writer.WriteLineAsync("GridX,GridY,Elevation");

            // Write data
            foreach (var kvp in grid.ElevationPoints)
            {
                var cell = kvp.Key;
                var elevation = kvp.Value;
                await writer.WriteLineAsync($"{cell.X},{cell.Y},{elevation.ToString("F3", CultureInfo.InvariantCulture)}");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to export elevation data to {filePath}", ex);
        }
    }

    public async Task<ElevationGrid> ImportElevationDataAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Elevation file not found: {filePath}");
        }

        var grid = new ElevationGrid();

        try
        {
            using var reader = new StreamReader(filePath);

            // Skip comment lines and header
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue;
                }

                // Check for grid resolution in comments
                if (line.Contains("Grid Resolution:"))
                {
                    var resParts = line.Split(':');
                    if (resParts.Length >= 2)
                    {
                        var resStr = resParts[1].Replace("meters", "").Trim();
                        if (double.TryParse(resStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var resolution))
                        {
                            grid.GridResolution = resolution;
                        }
                    }
                    continue;
                }

                // Skip column header line
                if (line.StartsWith("GridX,GridY,Elevation"))
                {
                    continue;
                }

                // Parse data line: GridX,GridY,Elevation
                var parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    if (int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var gridX) &&
                        int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var gridY) &&
                        double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var elevation))
                    {
                        var cell = new GridCell(gridX, gridY);
                        grid.ElevationPoints[cell] = elevation;

                        // Update bounding box
                        var position = new Position2D(gridX * grid.GridResolution, gridY * grid.GridResolution);
                        grid.UpdateBoundingBox(position);
                    }
                }
            }

            grid.UpdateStatistics();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to import elevation data from {filePath}", ex);
        }

        return grid;
    }
}
