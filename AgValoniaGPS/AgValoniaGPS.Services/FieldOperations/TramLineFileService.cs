using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using AgValoniaGPS.Models;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Service for tram line file I/O operations
/// Supports AgOpenGPS TramLines.txt, GeoJSON, and KML formats
/// </summary>
public class TramLineFileService : ITramLineFileService
{
    /// <summary>
    /// Save tram lines in AgOpenGPS format
    /// Format: $TramLine,{id} followed by easting,northing lines
    /// </summary>
    public void SaveAgOpenGPSFormat(Position[][] tramLines, string filePath)
    {
        if (tramLines == null)
            throw new ArgumentNullException(nameof(tramLines));
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        var lines = new List<string>();

        for (int i = 0; i < tramLines.Length; i++)
        {
            var tramLine = tramLines[i];
            if (tramLine == null || tramLine.Length == 0)
                continue;

            // Write tram line header
            lines.Add($"$TramLine,{i}");

            // Write positions
            foreach (var position in tramLine)
            {
                lines.Add($"{position.Easting:F2},{position.Northing:F2}");
            }
        }

        // Ensure directory exists
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllLines(filePath, lines);
    }

    /// <summary>
    /// Load tram lines from AgOpenGPS format
    /// </summary>
    public Position[][]? LoadAgOpenGPSFormat(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        if (!File.Exists(filePath))
            return null;

        var lines = File.ReadAllLines(filePath);
        var tramLines = new List<Position[]>();
        List<Position>? currentTramLine = null;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("$TramLine,"))
            {
                // Start new tram line
                if (currentTramLine != null && currentTramLine.Count > 0)
                {
                    tramLines.Add(currentTramLine.ToArray());
                }
                currentTramLine = new List<Position>();
            }
            else if (currentTramLine != null)
            {
                // Parse position line
                var parts = line.Split(',');
                if (parts.Length >= 2 &&
                    double.TryParse(parts[0], out double easting) &&
                    double.TryParse(parts[1], out double northing))
                {
                    currentTramLine.Add(new Position
                    {
                        Easting = easting,
                        Northing = northing,
                        Altitude = 0,
                        Latitude = 0,
                        Longitude = 0,
                        Zone = 0,
                        Hemisphere = 'N'
                    });
                }
            }
        }

        // Add last tram line
        if (currentTramLine != null && currentTramLine.Count > 0)
        {
            tramLines.Add(currentTramLine.ToArray());
        }

        return tramLines.Count > 0 ? tramLines.ToArray() : null;
    }

    /// <summary>
    /// Export tram lines to GeoJSON format
    /// Format: MultiLineString feature collection
    /// </summary>
    public void ExportGeoJSON(Position[][] tramLines, string filePath)
    {
        if (tramLines == null)
            throw new ArgumentNullException(nameof(tramLines));
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        var features = new List<object>();

        for (int i = 0; i < tramLines.Length; i++)
        {
            var tramLine = tramLines[i];
            if (tramLine == null || tramLine.Length == 0)
                continue;

            var coordinates = tramLine.Select(p => new[] { p.Easting, p.Northing }).ToArray();

            features.Add(new
            {
                type = "Feature",
                properties = new
                {
                    id = i,
                    name = $"Tram Line {i}"
                },
                geometry = new
                {
                    type = "LineString",
                    coordinates = coordinates
                }
            });
        }

        var geoJson = new
        {
            type = "FeatureCollection",
            features = features
        };

        // Ensure directory exists
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(geoJson, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Import tram lines from GeoJSON format
    /// </summary>
    public Position[][] ImportGeoJSON(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        if (!File.Exists(filePath))
            throw new FileNotFoundException("GeoJSON file not found", filePath);

        var json = File.ReadAllText(filePath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var tramLines = new List<Position[]>();

        if (root.TryGetProperty("features", out var features))
        {
            foreach (var feature in features.EnumerateArray())
            {
                if (feature.TryGetProperty("geometry", out var geometry) &&
                    geometry.TryGetProperty("coordinates", out var coordinates))
                {
                    var positions = new List<Position>();

                    foreach (var coord in coordinates.EnumerateArray())
                    {
                        if (coord.GetArrayLength() >= 2)
                        {
                            double easting = coord[0].GetDouble();
                            double northing = coord[1].GetDouble();

                            positions.Add(new Position
                            {
                                Easting = easting,
                                Northing = northing,
                                Altitude = 0,
                                Latitude = 0,
                                Longitude = 0,
                                Zone = 0,
                                Hemisphere = 'N'
                            });
                        }
                    }

                    if (positions.Count > 0)
                    {
                        tramLines.Add(positions.ToArray());
                    }
                }
            }
        }

        return tramLines.ToArray();
    }

    /// <summary>
    /// Export tram lines to KML format
    /// Format: Multiple LineString placemarks
    /// </summary>
    public void ExportKML(Position[][] tramLines, string filePath)
    {
        if (tramLines == null)
            throw new ArgumentNullException(nameof(tramLines));
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        XNamespace kmlNs = "http://www.opengis.net/kml/2.2";

        var kml = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(kmlNs + "kml",
                new XElement(kmlNs + "Document",
                    new XElement(kmlNs + "name", "Tram Lines"),
                    tramLines.Select((tramLine, index) =>
                        new XElement(kmlNs + "Placemark",
                            new XElement(kmlNs + "name", $"Tram Line {index}"),
                            new XElement(kmlNs + "LineString",
                                new XElement(kmlNs + "coordinates",
                                    string.Join(" ", tramLine.Select(p =>
                                        $"{p.Easting:F6},{p.Northing:F6},0"))
                                )
                            )
                        )
                    )
                )
            )
        );

        // Ensure directory exists
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        kml.Save(filePath);
    }

    /// <summary>
    /// Import tram lines from KML format
    /// </summary>
    public Position[][] ImportKML(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        if (!File.Exists(filePath))
            throw new FileNotFoundException("KML file not found", filePath);

        var kml = XDocument.Load(filePath);
        XNamespace kmlNs = "http://www.opengis.net/kml/2.2";

        var tramLines = new List<Position[]>();

        var placemarks = kml.Descendants(kmlNs + "Placemark");

        foreach (var placemark in placemarks)
        {
            var coordinates = placemark.Descendants(kmlNs + "coordinates").FirstOrDefault()?.Value;
            if (string.IsNullOrWhiteSpace(coordinates))
                continue;

            var positions = new List<Position>();
            var points = coordinates.Trim().Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var point in points)
            {
                var parts = point.Split(',');
                if (parts.Length >= 2 &&
                    double.TryParse(parts[0], out double easting) &&
                    double.TryParse(parts[1], out double northing))
                {
                    positions.Add(new Position
                    {
                        Easting = easting,
                        Northing = northing,
                        Altitude = 0,
                        Latitude = 0,
                        Longitude = 0,
                        Zone = 0,
                        Hemisphere = 'N'
                    });
                }
            }

            if (positions.Count > 0)
            {
                tramLines.Add(positions.ToArray());
            }
        }

        return tramLines.ToArray();
    }
}
