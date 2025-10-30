using System;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Services.Rendering;

/// <summary>
/// Implements guidance line geometry generation for OpenGL rendering.
/// Converts guidance lines into GPU-ready vertex arrays.
/// </summary>
public class GuidanceGeometryService : IGuidanceGeometryService
{
    /// <summary>
    /// Generates an AB line extending along the line's heading.
    /// </summary>
    public float[] GenerateABLine(ABLine line, float visibleLength)
    {
        if (line == null)
            throw new ArgumentNullException(nameof(line));

        if (visibleLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(visibleLength), "Visible length must be positive");

        // Use midpoint as origin
        var origin = line.MidPoint;
        var unitVector = line.UnitVector;

        // Calculate start and end points
        double startX = origin.Easting - unitVector.X * visibleLength;
        double startY = origin.Northing - unitVector.Y * visibleLength;
        double endX = origin.Easting + unitVector.X * visibleLength;
        double endY = origin.Northing + unitVector.Y * visibleLength;

        return new float[]
        {
            (float)startX, (float)startY,
            (float)endX, (float)endY
        };
    }

    /// <summary>
    /// Generates curve line geometry from curve points.
    /// </summary>
    public float[] GenerateCurveLine(CurveLine curve)
    {
        if (curve == null)
            throw new ArgumentNullException(nameof(curve));

        if (curve.Points == null || curve.Points.Count < 2)
            throw new ArgumentException("Curve must have at least 2 points", nameof(curve));

        var vertices = new float[curve.Points.Count * 2];
        int index = 0;

        foreach (var point in curve.Points)
        {
            vertices[index++] = (float)point.Easting;
            vertices[index++] = (float)point.Northing;
        }

        return vertices;
    }

    /// <summary>
    /// Generates contour line geometry from contour points.
    /// </summary>
    public float[] GenerateContourLine(ContourLine contour)
    {
        if (contour == null)
            throw new ArgumentNullException(nameof(contour));

        if (contour.Points == null || contour.Points.Count < 2)
            throw new ArgumentException("Contour must have at least 2 points", nameof(contour));

        var vertices = new float[contour.Points.Count * 2];
        int index = 0;

        foreach (var point in contour.Points)
        {
            vertices[index++] = (float)point.Easting;
            vertices[index++] = (float)point.Northing;
        }

        return vertices;
    }

    /// <summary>
    /// Generates cross-track error indicator (line from vehicle to guidance).
    /// </summary>
    public float[] GenerateCrossTrackIndicator(Position vehiclePos, Position closestPoint, double crossTrackError)
    {
        if (vehiclePos == null)
            throw new ArgumentNullException(nameof(vehiclePos));

        if (closestPoint == null)
            throw new ArgumentNullException(nameof(closestPoint));

        return new float[]
        {
            (float)vehiclePos.Easting, (float)vehiclePos.Northing,
            (float)closestPoint.Easting, (float)closestPoint.Northing
        };
    }
}
