using System.Globalization;
using System.Text;
using System.Text.Json;
using AgValoniaGPS.Models;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Service for reading and writing headland files in multiple formats.
/// Supports AgOpenGPS .txt, GeoJSON, and KML formats.
/// </summary>
public class HeadlandFileService : IHeadlandFileService
{
    private const string HeadlandFileName = "Headland.txt";

    /// <inheritdoc/>
    public Position[][]? LoadHeadlandsAgOpenGPS(string fieldDirectory)
    {
        if (string.IsNullOrWhiteSpace(fieldDirectory))
            throw new ArgumentException("Field directory must be specified", nameof(fieldDirectory));

        var filePath = Path.Combine(fieldDirectory, HeadlandFileName);

        if (!File.Exists(filePath))
            return null;

        try
        {
            var passes = new List<Position[]>();
            var currentPass = new List<Position>();
            int currentPassNumber = -1;

            using var reader = new StreamReader(filePath);
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                if (string.IsNullOrEmpty(line))
                    continue;

                // Check for pass separator
                if (line.StartsWith("$HeadlandPass"))
                {
                    // Save previous pass if any
                    if (currentPass.Count > 0)
                    {
                        passes.Add(currentPass.ToArray());
                        currentPass.Clear();
                    }

                    // Parse pass number
                    var parts = line.Split(',');
                    if (parts.Length > 1 && int.TryParse(parts[1], out int passNum))
                    {
                        currentPassNumber = passNum;
                    }
                    continue;
                }

                // Skip legacy format header
                if (line.StartsWith("$"))
                    continue;

                // Parse coordinate line: easting,northing or easting,northing,heading
                var coords = line.Split(',');
                if (coords.Length >= 2)
                {
                    if (double.TryParse(coords[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double easting) &&
                        double.TryParse(coords[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double northing))
                    {
                        double heading = 0;
                        if (coords.Length >= 3)
                        {
                            double.TryParse(coords[2], NumberStyles.Float, CultureInfo.InvariantCulture, out heading);
                        }

                        currentPass.Add(new Position
                        {
                            Easting = easting,
                            Northing = northing,
                            Heading = heading
                        });
                    }
                }
            }

            // Add final pass
            if (currentPass.Count > 0)
            {
                passes.Add(currentPass.ToArray());
            }

            return passes.Count > 0 ? passes.ToArray() : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading headlands from {filePath}: {ex.Message}");
            return null;
        }
    }

    /// <inheritdoc/>
    public void SaveHeadlandsAgOpenGPS(Position[][] headlands, string fieldDirectory)
    {
        if (headlands == null)
            throw new ArgumentNullException(nameof(headlands));
        if (string.IsNullOrWhiteSpace(fieldDirectory))
            throw new ArgumentException("Field directory must be specified", nameof(fieldDirectory));

        if (!Directory.Exists(fieldDirectory))
            Directory.CreateDirectory(fieldDirectory);

        var filePath = Path.Combine(fieldDirectory, HeadlandFileName);

        try
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

            for (int passNum = 0; passNum < headlands.Length; passNum++)
            {
                var pass = headlands[passNum];

                // Write pass separator
                writer.WriteLine($"$HeadlandPass,{passNum}");

                // Write points
                foreach (var point in pass)
                {
                    writer.WriteLine($"{point.Easting:F2},{point.Northing:F2},{point.Heading:F2}");
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save headlands to {filePath}", ex);
        }
    }

    /// <inheritdoc/>
    public Position[][]? LoadHeadlandsGeoJSON(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path must be specified", nameof(filePath));

        if (!File.Exists(filePath))
            return null;

        try
        {
            var json = File.ReadAllText(filePath);
            var doc = JsonDocument.Parse(json);

            var passes = new List<Position[]>();

            // Parse GeoJSON FeatureCollection with MultiPolygon
            if (doc.RootElement.TryGetProperty("type", out var typeElement) &&
                typeElement.GetString() == "FeatureCollection")
            {
                if (doc.RootElement.TryGetProperty("features", out var features))
                {
                    foreach (var feature in features.EnumerateArray())
                    {
                        if (feature.TryGetProperty("geometry", out var geometry) &&
                            geometry.TryGetProperty("coordinates", out var coordinates))
                        {
                            var pass = ParseGeoJSONCoordinates(coordinates);
                            if (pass != null && pass.Length > 0)
                            {
                                passes.Add(pass);
                            }
                        }
                    }
                }
            }

            return passes.Count > 0 ? passes.ToArray() : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading GeoJSON headlands from {filePath}: {ex.Message}");
            return null;
        }
    }

    /// <inheritdoc/>
    public void SaveHeadlandsGeoJSON(Position[][] headlands, string filePath)
    {
        if (headlands == null)
            throw new ArgumentNullException(nameof(headlands));
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path must be specified", nameof(filePath));

        var dirPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        try
        {
            var features = new List<object>();

            for (int i = 0; i < headlands.Length; i++)
            {
                var pass = headlands[i];
                var coordinates = new List<List<double>>();

                foreach (var point in pass)
                {
                    // GeoJSON uses [longitude, latitude] but we're using UTM [easting, northing]
                    coordinates.Add(new List<double> { point.Easting, point.Northing });
                }

                // Close the polygon
                if (coordinates.Count > 0)
                {
                    coordinates.Add(coordinates[0]);
                }

                var feature = new
                {
                    type = "Feature",
                    geometry = new
                    {
                        type = "Polygon",
                        coordinates = new[] { coordinates }
                    },
                    properties = new
                    {
                        passNumber = i,
                        name = $"Headland Pass {i}"
                    }
                };

                features.Add(feature);
            }

            var geoJson = new
            {
                type = "FeatureCollection",
                features = features
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(geoJson, options);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save GeoJSON headlands to {filePath}", ex);
        }
    }

    /// <inheritdoc/>
    public Position[][]? LoadHeadlandsKML(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path must be specified", nameof(filePath));

        if (!File.Exists(filePath))
            return null;

        // Simplified KML parsing - full implementation would use XML parser
        // For now, return null to indicate not implemented
        Console.WriteLine("KML import not yet implemented");
        return null;
    }

    /// <inheritdoc/>
    public void SaveHeadlandsKML(Position[][] headlands, string filePath)
    {
        if (headlands == null)
            throw new ArgumentNullException(nameof(headlands));
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path must be specified", nameof(filePath));

        var dirPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<kml xmlns=\"http://www.opengis.net/kml/2.2\">");
            sb.AppendLine("  <Document>");
            sb.AppendLine("    <name>Headlands</name>");

            for (int i = 0; i < headlands.Length; i++)
            {
                var pass = headlands[i];

                sb.AppendLine("    <Placemark>");
                sb.AppendLine($"      <name>Headland Pass {i}</name>");
                sb.AppendLine("      <Polygon>");
                sb.AppendLine("        <outerBoundaryIs>");
                sb.AppendLine("          <LinearRing>");
                sb.AppendLine("            <coordinates>");

                foreach (var point in pass)
                {
                    // KML uses lon,lat,altitude but we're using UTM easting,northing
                    // In a real implementation, would convert UTM to lat/lon
                    sb.AppendLine($"              {point.Easting:F6},{point.Northing:F6},0");
                }

                // Close the polygon
                if (pass.Length > 0)
                {
                    sb.AppendLine($"              {pass[0].Easting:F6},{pass[0].Northing:F6},0");
                }

                sb.AppendLine("            </coordinates>");
                sb.AppendLine("          </LinearRing>");
                sb.AppendLine("        </outerBoundaryIs>");
                sb.AppendLine("      </Polygon>");
                sb.AppendLine("    </Placemark>");
            }

            sb.AppendLine("  </Document>");
            sb.AppendLine("</kml>");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save KML headlands to {filePath}", ex);
        }
    }

    /// <summary>
    /// Parse GeoJSON coordinate array into Position array
    /// </summary>
    private Position[]? ParseGeoJSONCoordinates(JsonElement coordinates)
    {
        var positions = new List<Position>();

        try
        {
            // Handle Polygon coordinates: array of rings (we take first ring)
            if (coordinates.ValueKind == JsonValueKind.Array)
            {
                var firstRing = coordinates.EnumerateArray().FirstOrDefault();
                if (firstRing.ValueKind == JsonValueKind.Array)
                {
                    foreach (var coord in firstRing.EnumerateArray())
                    {
                        if (coord.ValueKind == JsonValueKind.Array)
                        {
                            var coordArray = coord.EnumerateArray().ToArray();
                            if (coordArray.Length >= 2)
                            {
                                positions.Add(new Position
                                {
                                    Easting = coordArray[0].GetDouble(),
                                    Northing = coordArray[1].GetDouble()
                                });
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing GeoJSON coordinates: {ex.Message}");
        }

        return positions.Count > 0 ? positions.ToArray() : null;
    }
}
