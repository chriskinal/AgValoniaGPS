using System;
using System.Collections.Generic;

namespace AgValoniaGPS.Models;

/// <summary>
/// Represents a single boundary polygon (outer or inner)
/// Points are in local coordinates (meters from field origin)
/// </summary>
public class BoundaryPolygon
{
    /// <summary>
    /// Boundary points (Easting, Northing, Heading in local coordinates)
    /// Minimum 3 points required for valid polygon
    /// </summary>
    public List<BoundaryPoint> Points { get; set; } = new List<BoundaryPoint>();

    /// <summary>
    /// Whether this is a drive-through boundary (true) or avoid boundary (false)
    /// </summary>
    public bool IsDriveThrough { get; set; } = false;

    /// <summary>
    /// Area in square meters (calculated from points)
    /// </summary>
    public double AreaSquareMeters
    {
        get
        {
            if (Points.Count < 3) return 0;

            // Shoelace formula for polygon area
            double area = 0;
            for (int i = 0; i < Points.Count; i++)
            {
                int j = (i + 1) % Points.Count;
                area += Points[i].Easting * Points[j].Northing;
                area -= Points[j].Easting * Points[i].Northing;
            }
            return Math.Abs(area) / 2.0;
        }
    }

    /// <summary>
    /// Area in hectares (10,000 square meters = 1 hectare)
    /// </summary>
    public double AreaHectares => AreaSquareMeters / 10000.0;

    /// <summary>
    /// Area in acres (4046.86 square meters = 1 acre)
    /// </summary>
    public double AreaAcres => AreaSquareMeters / 4046.86;

    /// <summary>
    /// Check if this polygon is valid
    /// </summary>
    public bool IsValid => Points.Count >= 3;

    /// <summary>
    /// Check if a point is inside this polygon using ray-casting algorithm
    /// Ported from AOG_Dev CPolygon.IsPointInPolygon()
    /// </summary>
    /// <param name="easting">Point easting coordinate</param>
    /// <param name="northing">Point northing coordinate</param>
    /// <returns>True if point is inside the polygon</returns>
    public bool IsPointInside(double easting, double northing)
    {
        if (Points.Count < 3) return false;

        bool isInside = false;
        int j = Points.Count - 1;

        for (int i = 0; i < Points.Count; i++)
        {
            // Ray-casting algorithm: cast ray from point to infinity
            // Count intersections with polygon edges
            if ((Points[i].Northing > northing) != (Points[j].Northing > northing) &&
                (easting < (Points[j].Easting - Points[i].Easting) *
                (northing - Points[i].Northing) /
                (Points[j].Northing - Points[i].Northing) + Points[i].Easting))
            {
                isInside = !isInside;
            }
            j = i;
        }

        return isInside;
    }
}

/// <summary>
/// Represents a single point in a boundary polygon
/// Coordinates are in local system (meters from field origin)
/// </summary>
public class BoundaryPoint
{
    /// <summary>
    /// Easting (X coordinate) in meters
    /// </summary>
    public double Easting { get; set; }

    /// <summary>
    /// Northing (Y coordinate) in meters
    /// </summary>
    public double Northing { get; set; }

    /// <summary>
    /// Heading/direction at this point in radians
    /// </summary>
    public double Heading { get; set; }

    public BoundaryPoint() { }

    public BoundaryPoint(double easting, double northing, double heading)
    {
        Easting = easting;
        Northing = northing;
        Heading = heading;
    }
}
