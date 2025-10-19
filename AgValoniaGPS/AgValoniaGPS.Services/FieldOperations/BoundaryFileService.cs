using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using AgValoniaGPS.Models;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Implementation of boundary file I/O service supporting AgOpenGPS, GeoJSON, and KML formats
/// </summary>
public class BoundaryFileService : IBoundaryFileService
{
    public Position[] LoadFromAgOpenGPS(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        if (!File.Exists(filePath))
            return Array.Empty<Position>();

        try
        {
            var positions = new List<Position>();
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    if (double.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double easting) &&
                        double.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double northing))
                    {
                        positions.Add(new Position
                        {
                            Easting = easting,
                            Northing = northing,
                            Latitude = 0,  // Will be calculated from UTM if needed
                            Longitude = 0,
                            Altitude = 0,
                            Zone = 0,
                            Hemisphere = 'N',
                            Heading = 0,
                            Speed = 0
                        });
                    }
                }
            }

            return positions.ToArray();
        }
        catch (Exception)
        {
            return Array.Empty<Position>();
        }
    }

    public void SaveToAgOpenGPS(Position[] boundary, string filePath)
    {
        if (boundary == null)
            throw new ArgumentNullException(nameof(boundary));
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        // Create directory if it doesn't exist
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var sb = new StringBuilder();
        foreach (var position in boundary)
        {
            sb.AppendLine($"{position.Easting.ToString("F2", CultureInfo.InvariantCulture)},{position.Northing.ToString("F2", CultureInfo.InvariantCulture)}");
        }

        File.WriteAllText(filePath, sb.ToString());
    }

    public Position[] LoadFromGeoJSON(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        if (!File.Exists(filePath))
            return Array.Empty<Position>();

        try
        {
            var json = File.ReadAllText(filePath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Handle both Feature and FeatureCollection
            JsonElement geometry;
            if (root.TryGetProperty("type", out var typeElement))
            {
                var type = typeElement.GetString();
                if (type == "FeatureCollection")
                {
                    // Get first feature
                    if (root.TryGetProperty("features", out var features) && features.GetArrayLength() > 0)
                    {
                        geometry = features[0].GetProperty("geometry");
                    }
                    else
                    {
                        return Array.Empty<Position>();
                    }
                }
                else if (type == "Feature")
                {
                    geometry = root.GetProperty("geometry");
                }
                else
                {
                    return Array.Empty<Position>();
                }
            }
            else
            {
                return Array.Empty<Position>();
            }

            // Parse coordinates
            if (geometry.TryGetProperty("coordinates", out var coordinates) && coordinates.GetArrayLength() > 0)
            {
                var ring = coordinates[0]; // First ring (outer boundary)
                var positions = new List<Position>();

                foreach (var coord in ring.EnumerateArray())
                {
                    if (coord.GetArrayLength() >= 2)
                    {
                        double longitude = coord[0].GetDouble();
                        double latitude = coord[1].GetDouble();

                        // Convert WGS84 to UTM (simplified - assumes all points in same zone)
                        // In production, use proper coordinate conversion library
                        var (easting, northing, zone, hemisphere) = ConvertWGS84ToUTM(latitude, longitude);

                        positions.Add(new Position
                        {
                            Latitude = latitude,
                            Longitude = longitude,
                            Easting = easting,
                            Northing = northing,
                            Zone = zone,
                            Hemisphere = hemisphere,
                            Altitude = 0,
                            Heading = 0,
                            Speed = 0
                        });
                    }
                }

                return positions.ToArray();
            }

            return Array.Empty<Position>();
        }
        catch (Exception)
        {
            return Array.Empty<Position>();
        }
    }

    public void SaveToGeoJSON(Position[] boundary, string filePath)
    {
        if (boundary == null)
            throw new ArgumentNullException(nameof(boundary));
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        // Create directory if it doesn't exist
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Build coordinates array
        var coordinates = new List<double[]>();
        foreach (var position in boundary)
        {
            // Convert UTM to WGS84 if needed
            double lat, lon;
            if (position.Latitude == 0 && position.Longitude == 0)
            {
                (lat, lon) = ConvertUTMToWGS84(position.Easting, position.Northing, position.Zone, position.Hemisphere);
            }
            else
            {
                lat = position.Latitude;
                lon = position.Longitude;
            }

            coordinates.Add(new[] { lon, lat });
        }

        // Close the ring
        if (coordinates.Count > 0)
        {
            coordinates.Add(coordinates[0]);
        }

        // Build GeoJSON structure
        var geoJson = new
        {
            type = "Feature",
            geometry = new
            {
                type = "Polygon",
                coordinates = new[] { coordinates }
            },
            properties = new
            {
                name = "Field Boundary",
                area = 0.0 // Could calculate area here
            }
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(geoJson, options);
        File.WriteAllText(filePath, json);
    }

    public Position[] LoadFromKML(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        if (!File.Exists(filePath))
            return Array.Empty<Position>();

        try
        {
            var doc = XDocument.Load(filePath);
            XNamespace kmlNs = "http://www.opengis.net/kml/2.2";

            // Find coordinates element
            var coordinatesElement = doc.Descendants(kmlNs + "coordinates").FirstOrDefault();
            if (coordinatesElement == null)
                return Array.Empty<Position>();

            var positions = new List<Position>();
            var coordText = coordinatesElement.Value.Trim();
            var coordPairs = coordText.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var coordPair in coordPairs)
            {
                var parts = coordPair.Split(',');
                if (parts.Length >= 2)
                {
                    if (double.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double longitude) &&
                        double.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double latitude))
                    {
                        var (easting, northing, zone, hemisphere) = ConvertWGS84ToUTM(latitude, longitude);

                        positions.Add(new Position
                        {
                            Latitude = latitude,
                            Longitude = longitude,
                            Easting = easting,
                            Northing = northing,
                            Zone = zone,
                            Hemisphere = hemisphere,
                            Altitude = 0,
                            Heading = 0,
                            Speed = 0
                        });
                    }
                }
            }

            return positions.ToArray();
        }
        catch (Exception)
        {
            return Array.Empty<Position>();
        }
    }

    public void SaveToKML(Position[] boundary, string filePath)
    {
        if (boundary == null)
            throw new ArgumentNullException(nameof(boundary));
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        // Create directory if it doesn't exist
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        XNamespace kmlNs = "http://www.opengis.net/kml/2.2";
        var coordinatesText = new StringBuilder();

        foreach (var position in boundary)
        {
            // Convert UTM to WGS84 if needed
            double lat, lon;
            if (position.Latitude == 0 && position.Longitude == 0)
            {
                (lat, lon) = ConvertUTMToWGS84(position.Easting, position.Northing, position.Zone, position.Hemisphere);
            }
            else
            {
                lat = position.Latitude;
                lon = position.Longitude;
            }

            coordinatesText.Append($"{lon.ToString("F8", CultureInfo.InvariantCulture)},{lat.ToString("F8", CultureInfo.InvariantCulture)},0 ");
        }

        // Close the ring
        if (boundary.Length > 0)
        {
            var first = boundary[0];
            double lat, lon;
            if (first.Latitude == 0 && first.Longitude == 0)
            {
                (lat, lon) = ConvertUTMToWGS84(first.Easting, first.Northing, first.Zone, first.Hemisphere);
            }
            else
            {
                lat = first.Latitude;
                lon = first.Longitude;
            }
            coordinatesText.Append($"{lon.ToString("F8", CultureInfo.InvariantCulture)},{lat.ToString("F8", CultureInfo.InvariantCulture)},0");
        }

        var kml = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(kmlNs + "kml",
                new XElement(kmlNs + "Document",
                    new XElement(kmlNs + "name", "Field Boundary"),
                    new XElement(kmlNs + "Placemark",
                        new XElement(kmlNs + "name", "Boundary"),
                        new XElement(kmlNs + "Polygon",
                            new XElement(kmlNs + "outerBoundaryIs",
                                new XElement(kmlNs + "LinearRing",
                                    new XElement(kmlNs + "coordinates", coordinatesText.ToString())
                                )
                            )
                        )
                    )
                )
            )
        );

        kml.Save(filePath);
    }

    /// <summary>
    /// Simplified WGS84 to UTM conversion (for demonstration purposes)
    /// </summary>
    /// <remarks>
    /// In production, use a proper coordinate transformation library like ProjNet or similar
    /// </remarks>
    private (double easting, double northing, int zone, char hemisphere) ConvertWGS84ToUTM(double latitude, double longitude)
    {
        // Simplified conversion - just use a basic approximation
        // In production, use proper UTM conversion
        int zone = (int)Math.Floor((longitude + 180) / 6) + 1;
        char hemisphere = latitude >= 0 ? 'N' : 'S';

        // Very simplified conversion (not accurate for production use)
        double easting = (longitude + 180) * 111320.0;
        double northing = latitude * 110540.0;

        return (easting, northing, zone, hemisphere);
    }

    /// <summary>
    /// Simplified UTM to WGS84 conversion (for demonstration purposes)
    /// </summary>
    /// <remarks>
    /// In production, use a proper coordinate transformation library like ProjNet or similar
    /// </remarks>
    private (double latitude, double longitude) ConvertUTMToWGS84(double easting, double northing, int zone, char hemisphere)
    {
        // Very simplified conversion (not accurate for production use)
        double longitude = (easting / 111320.0) - 180;
        double latitude = northing / 110540.0;

        if (hemisphere == 'S')
            latitude = -latitude;

        return (latitude, longitude);
    }
}
