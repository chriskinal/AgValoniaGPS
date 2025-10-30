using System;
using System.Collections.Generic;
using AgValoniaGPS.Models;

namespace AgValoniaGPS.Services;

/// <summary>
/// Mathematical utility methods for geometric calculations, distance, and angular operations.
/// </summary>
public static class MathUtilities
{
    #region Angular Operations

    /// <summary>
    /// Converts radians to degrees.
    /// </summary>
    /// <param name="radians">Angle in radians</param>
    /// <returns>Angle in degrees</returns>
    public static double ToDegrees(double radians)
    {
        return radians * MathConstants.RadiansToDegrees;
    }

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    /// <param name="degrees">Angle in degrees</param>
    /// <returns>Angle in radians</returns>
    public static double ToRadians(double degrees)
    {
        return degrees * MathConstants.DegreesToRadians;
    }

    /// <summary>
    /// Calculates the absolute angle difference between two angles (range 0 to π).
    /// Accounts for wraparound at 2π.
    /// </summary>
    /// <param name="angle1">First angle in radians</param>
    /// <param name="angle2">Second angle in radians</param>
    /// <returns>Absolute difference in radians (0 to π)</returns>
    public static double AngleDifference(double angle1, double angle2)
    {
        double diff = Math.Abs(angle1 - angle2);
        if (diff > Math.PI)
        {
            diff = MathConstants.TwoPI - diff;
        }
        return diff;
    }

    /// <summary>
    /// Normalizes an angle to the range [0, 2π).
    /// </summary>
    /// <param name="angle">Angle in radians</param>
    /// <returns>Normalized angle in range [0, 2π)</returns>
    public static double NormalizeAngle(double angle)
    {
        angle %= MathConstants.TwoPI;
        if (angle < 0)
        {
            angle += MathConstants.TwoPI;
        }
        return angle;
    }

    /// <summary>
    /// Normalizes an angle to the range [-π, π).
    /// </summary>
    /// <param name="angle">Angle in radians</param>
    /// <returns>Normalized angle in range [-π, π)</returns>
    public static double NormalizeAnglePi(double angle)
    {
        while (angle > Math.PI)
        {
            angle -= MathConstants.TwoPI;
        }
        while (angle < -Math.PI)
        {
            angle += MathConstants.TwoPI;
        }
        return angle;
    }

    #endregion

    #region Distance Calculations

    /// <summary>
    /// Calculates the Euclidean distance between two points.
    /// </summary>
    /// <param name="east1">Easting of first point</param>
    /// <param name="north1">Northing of first point</param>
    /// <param name="east2">Easting of second point</param>
    /// <param name="north2">Northing of second point</param>
    /// <returns>Distance in meters</returns>
    public static double Distance(double east1, double north1, double east2, double north2)
    {
        double deltaEast = east1 - east2;
        double deltaNorth = north1 - north2;
        return Math.Sqrt(deltaEast * deltaEast + deltaNorth * deltaNorth);
    }

    /// <summary>
    /// Calculates the Euclidean distance between two positions.
    /// </summary>
    /// <param name="pos1">First position</param>
    /// <param name="pos2">Second position</param>
    /// <returns>Distance in meters</returns>
    public static double Distance(Position pos1, Position pos2)
    {
        double deltaEast = pos1.Easting - pos2.Easting;
        double deltaNorth = pos1.Northing - pos2.Northing;
        return Math.Sqrt(deltaEast * deltaEast + deltaNorth * deltaNorth);
    }

    /// <summary>
    /// Calculates the squared distance between two points (faster, no square root).
    /// Useful for comparisons where exact distance isn't needed.
    /// </summary>
    /// <param name="east1">Easting of first point</param>
    /// <param name="north1">Northing of first point</param>
    /// <param name="east2">Easting of second point</param>
    /// <param name="north2">Northing of second point</param>
    /// <returns>Squared distance</returns>
    public static double DistanceSquared(double east1, double north1, double east2, double north2)
    {
        double deltaEast = east1 - east2;
        double deltaNorth = north1 - north2;
        return deltaEast * deltaEast + deltaNorth * deltaNorth;
    }

    /// <summary>
    /// Calculates the squared distance between two positions (faster, no square root).
    /// </summary>
    /// <param name="pos1">First position</param>
    /// <param name="pos2">Second position</param>
    /// <returns>Squared distance</returns>
    public static double DistanceSquared(Position pos1, Position pos2)
    {
        double deltaEast = pos1.Easting - pos2.Easting;
        double deltaNorth = pos1.Northing - pos2.Northing;
        return deltaEast * deltaEast + deltaNorth * deltaNorth;
    }

    #endregion

    #region Geometric Tests

    /// <summary>
    /// Checks if a point lies within the range between two points on a line segment.
    /// Uses inner product to determine if the point projects onto the segment.
    /// </summary>
    /// <param name="startEast">Start point easting</param>
    /// <param name="startNorth">Start point northing</param>
    /// <param name="endEast">End point easting</param>
    /// <param name="endNorth">End point northing</param>
    /// <param name="pointEast">Test point easting</param>
    /// <param name="pointNorth">Test point northing</param>
    /// <returns>True if point is within range of the segment</returns>
    public static bool IsPointInRangeBetweenAB(
        double startEast, double startNorth,
        double endEast, double endNorth,
        double pointEast, double pointNorth)
    {
        double dx = endEast - startEast;
        double dy = endNorth - startNorth;
        double innerProduct = (pointEast - startEast) * dx + (pointNorth - startNorth) * dy;
        double segmentLengthSquared = dx * dx + dy * dy;

        return innerProduct >= 0 && innerProduct <= segmentLengthSquared;
    }

    /// <summary>
    /// Checks if a point lies within the range between two positions on a line segment.
    /// </summary>
    /// <param name="start">Start position</param>
    /// <param name="end">End position</param>
    /// <param name="point">Test point</param>
    /// <returns>True if point is within range of the segment</returns>
    public static bool IsPointInRangeBetweenAB(Position start, Position end, Position point)
    {
        return IsPointInRangeBetweenAB(
            start.Easting, start.Northing,
            end.Easting, end.Northing,
            point.Easting, point.Northing);
    }

    #endregion

    #region Catmull-Rom Spline Interpolation

    /// <summary>
    /// Calculates a point on a Catmull-Rom spline.
    /// This creates smooth curves through control points.
    /// </summary>
    /// <param name="t">Interpolation parameter (0 to 1) between p1 and p2</param>
    /// <param name="p0">Point before the segment</param>
    /// <param name="p1">Start of segment</param>
    /// <param name="p2">End of segment</param>
    /// <param name="p3">Point after the segment</param>
    /// <returns>Interpolated position on the curve</returns>
    public static Position CatmullRom(double t, Position p0, Position p1, Position p2, Position p3)
    {
        double t2 = t * t;
        double t3 = t2 * t;

        double q1 = -t3 + 2.0 * t2 - t;
        double q2 = 3.0 * t3 - 5.0 * t2 + 2.0;
        double q3 = -3.0 * t3 + 4.0 * t2 + t;
        double q4 = t3 - t2;

        double easting = 0.5 * (p0.Easting * q1 + p1.Easting * q2 + p2.Easting * q3 + p3.Easting * q4);
        double northing = 0.5 * (p0.Northing * q1 + p1.Northing * q2 + p2.Northing * q3 + p3.Northing * q4);

        return new Position
        {
            Easting = easting,
            Northing = northing,
            Zone = p1.Zone,
            Hemisphere = p1.Hemisphere
        };
    }

    /// <summary>
    /// Generates a smooth curve through multiple points using Catmull-Rom spline.
    /// </summary>
    /// <param name="controlPoints">List of control points</param>
    /// <param name="segmentsPerSpan">Number of points to generate between each pair of control points</param>
    /// <returns>List of interpolated points forming a smooth curve</returns>
    public static List<Position> CatmullRomPath(List<Position> controlPoints, int segmentsPerSpan = 10)
    {
        if (controlPoints == null || controlPoints.Count < 2)
        {
            return new List<Position>();
        }

        var result = new List<Position>();

        if (controlPoints.Count == 2)
        {
            // Not enough points for spline, return linear interpolation
            result.AddRange(controlPoints);
            return result;
        }

        // For each segment between control points
        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            // Get the four control points for this segment
            Position p0 = i == 0 ? controlPoints[0] : controlPoints[i - 1];
            Position p1 = controlPoints[i];
            Position p2 = controlPoints[i + 1];
            Position p3 = (i + 2 < controlPoints.Count) ? controlPoints[i + 2] : controlPoints[i + 1];

            // Generate points along this segment
            for (int j = 0; j < segmentsPerSpan; j++)
            {
                double t = (double)j / segmentsPerSpan;
                Position interpolated = CatmullRom(t, p0, p1, p2, p3);
                result.Add(interpolated);
            }
        }

        // Add the final point
        result.Add(controlPoints[controlPoints.Count - 1]);

        return result;
    }

    #endregion

    #region Raycasting

    /// <summary>
    /// Performs a raycast from origin in the direction of heading to find intersection with line segment.
    /// </summary>
    /// <param name="rayOriginEast">Ray origin easting</param>
    /// <param name="rayOriginNorth">Ray origin northing</param>
    /// <param name="rayDirEast">Ray direction easting component</param>
    /// <param name="rayDirNorth">Ray direction northing component</param>
    /// <param name="segAEast">Segment point A easting</param>
    /// <param name="segANorth">Segment point A northing</param>
    /// <param name="segBEast">Segment point B easting</param>
    /// <param name="segBNorth">Segment point B northing</param>
    /// <param name="intersectionEast">Intersection point easting (if found)</param>
    /// <param name="intersectionNorth">Intersection point northing (if found)</param>
    /// <returns>True if ray intersects the segment</returns>
    public static bool TryRaySegmentIntersection(
        double rayOriginEast, double rayOriginNorth,
        double rayDirEast, double rayDirNorth,
        double segAEast, double segANorth,
        double segBEast, double segBNorth,
        out double intersectionEast, out double intersectionNorth)
    {
        intersectionEast = 0;
        intersectionNorth = 0;

        double dx = segBEast - segAEast;
        double dy = segBNorth - segANorth;

        double det = -rayDirEast * dy + dx * rayDirNorth;

        // Check if parallel
        if (Math.Abs(det) < MathConstants.Epsilon)
        {
            return false;
        }

        double s = (-dy * (segAEast - rayOriginEast) + dx * (segANorth - rayOriginNorth)) / det;
        double t = (rayDirEast * (segANorth - rayOriginNorth) - rayDirNorth * (segAEast - rayOriginEast)) / det;

        // Check if intersection is valid (ray forward, within segment)
        if (s >= 0 && t >= 0 && t <= 1)
        {
            intersectionEast = rayOriginEast + s * rayDirEast;
            intersectionNorth = rayOriginNorth + s * rayDirNorth;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Finds the closest intersection point where a ray from the origin (in the direction of heading)
    /// intersects a polygon boundary.
    /// </summary>
    /// <param name="origin">Ray origin position with heading</param>
    /// <param name="polygon">Polygon boundary points</param>
    /// <param name="intersectionEast">Intersection point easting (if found)</param>
    /// <param name="intersectionNorth">Intersection point northing (if found)</param>
    /// <returns>True if ray intersects the polygon</returns>
    public static bool RaycastToPolygon(
        Position origin,
        List<Position> polygon,
        out double intersectionEast,
        out double intersectionNorth)
    {
        intersectionEast = 0;
        intersectionNorth = 0;

        if (polygon == null || polygon.Count < 3)
        {
            return false;
        }

        double rayDirEast = Math.Sin(origin.Heading);
        double rayDirNorth = Math.Cos(origin.Heading);

        double minDist = double.MaxValue;
        bool found = false;

        // Check each edge of the polygon
        for (int i = 0; i < polygon.Count; i++)
        {
            Position p1 = polygon[i];
            Position p2 = polygon[(i + 1) % polygon.Count];

            if (TryRaySegmentIntersection(
                origin.Easting, origin.Northing,
                rayDirEast, rayDirNorth,
                p1.Easting, p1.Northing,
                p2.Easting, p2.Northing,
                out double hitEast, out double hitNorth))
            {
                double dist = Distance(origin.Easting, origin.Northing, hitEast, hitNorth);
                if (dist < minDist)
                {
                    minDist = dist;
                    intersectionEast = hitEast;
                    intersectionNorth = hitNorth;
                    found = true;
                }
            }
        }

        return found;
    }

    #endregion

    #region Vector Operations

    /// <summary>
    /// Calculates the 2D cross product (scalar z-component) of two vectors.
    /// Returns positive if b is counter-clockwise from a, negative if clockwise.
    /// </summary>
    /// <param name="aEast">First vector easting component</param>
    /// <param name="aNorth">First vector northing component</param>
    /// <param name="bEast">Second vector easting component</param>
    /// <param name="bNorth">Second vector northing component</param>
    /// <returns>Scalar cross product (z-component)</returns>
    public static double Cross(double aEast, double aNorth, double bEast, double bNorth)
    {
        return aEast * bNorth - aNorth * bEast;
    }

    /// <summary>
    /// Calculates the 2D cross product of two Position2D vectors.
    /// </summary>
    /// <param name="a">First vector</param>
    /// <param name="b">Second vector</param>
    /// <returns>Scalar cross product (z-component)</returns>
    public static double Cross(Position2D a, Position2D b)
    {
        return a.Easting * b.Northing - a.Northing * b.Easting;
    }

    /// <summary>
    /// Calculates the dot product of two vectors.
    /// </summary>
    /// <param name="aEast">First vector easting component</param>
    /// <param name="aNorth">First vector northing component</param>
    /// <param name="bEast">Second vector easting component</param>
    /// <param name="bNorth">Second vector northing component</param>
    /// <returns>Dot product</returns>
    public static double Dot(double aEast, double aNorth, double bEast, double bNorth)
    {
        return aEast * bEast + aNorth * bNorth;
    }

    /// <summary>
    /// Calculates the dot product of two Position2D vectors.
    /// </summary>
    /// <param name="a">First vector</param>
    /// <param name="b">Second vector</param>
    /// <returns>Dot product</returns>
    public static double Dot(Position2D a, Position2D b)
    {
        return a.Easting * b.Easting + a.Northing * b.Northing;
    }

    /// <summary>
    /// Calculates the heading angle from a vector in the XZ plane (easting/northing).
    /// Returns angle in radians from north (0) rotating clockwise.
    /// </summary>
    /// <param name="east">Easting component</param>
    /// <param name="north">Northing component</param>
    /// <returns>Heading in radians (0 to 2π)</returns>
    public static double HeadingXZ(double east, double north)
    {
        return Math.Atan2(east, north);
    }

    /// <summary>
    /// Calculates the heading angle from a Position2D vector.
    /// </summary>
    /// <param name="vector">Vector to calculate heading from</param>
    /// <returns>Heading in radians (0 to 2π)</returns>
    public static double HeadingXZ(Position2D vector)
    {
        return Math.Atan2(vector.Easting, vector.Northing);
    }

    /// <summary>
    /// Calculates the magnitude (length) of a vector.
    /// </summary>
    /// <param name="east">Easting component</param>
    /// <param name="north">Northing component</param>
    /// <returns>Vector magnitude</returns>
    public static double Magnitude(double east, double north)
    {
        return Math.Sqrt(east * east + north * north);
    }

    /// <summary>
    /// Normalizes a vector to unit length.
    /// Returns zero vector if input length is near zero.
    /// </summary>
    /// <param name="east">Easting component</param>
    /// <param name="north">Northing component</param>
    /// <param name="outEast">Normalized easting component</param>
    /// <param name="outNorth">Normalized northing component</param>
    /// <returns>True if normalization succeeded, false if vector was too small</returns>
    public static bool TryNormalize(double east, double north, out double outEast, out double outNorth)
    {
        double length = Math.Sqrt(east * east + north * north);
        if (length < MathConstants.Epsilon)
        {
            outEast = 0;
            outNorth = 0;
            return false;
        }
        outEast = east / length;
        outNorth = north / length;
        return true;
    }

    /// <summary>
    /// Returns a vector perpendicular to the input (90 degrees counter-clockwise).
    /// </summary>
    /// <param name="east">Input easting</param>
    /// <param name="north">Input northing</param>
    /// <param name="outEast">Perpendicular easting</param>
    /// <param name="outNorth">Perpendicular northing</param>
    public static void PerpendicularLeft(double east, double north, out double outEast, out double outNorth)
    {
        outEast = -north;
        outNorth = east;
    }

    /// <summary>
    /// Returns a Position2D perpendicular to the input (90 degrees counter-clockwise).
    /// </summary>
    /// <param name="vector">Input vector</param>
    /// <returns>Perpendicular vector</returns>
    public static Position2D PerpendicularLeft(Position2D vector)
    {
        return new Position2D(-vector.Northing, vector.Easting);
    }

    /// <summary>
    /// Returns a vector perpendicular to the input (90 degrees clockwise).
    /// </summary>
    /// <param name="east">Input easting</param>
    /// <param name="north">Input northing</param>
    /// <param name="outEast">Perpendicular easting</param>
    /// <param name="outNorth">Perpendicular northing</param>
    public static void PerpendicularRight(double east, double north, out double outEast, out double outNorth)
    {
        outEast = north;
        outNorth = -east;
    }

    /// <summary>
    /// Returns a Position2D perpendicular to the input (90 degrees clockwise).
    /// </summary>
    /// <param name="vector">Input vector</param>
    /// <returns>Perpendicular vector</returns>
    public static Position2D PerpendicularRight(Position2D vector)
    {
        return new Position2D(vector.Northing, -vector.Easting);
    }

    /// <summary>
    /// Projects a point onto a line segment defined by two points.
    /// Returns the closest point on the segment and the parametric t value.
    /// </summary>
    /// <param name="segmentStart">Segment start position</param>
    /// <param name="segmentEnd">Segment end position</param>
    /// <param name="point">Point to project</param>
    /// <param name="t">Parametric value (0 = start, 1 = end, clamped to [0,1])</param>
    /// <returns>Closest point on the segment</returns>
    public static Position2D ProjectOnSegment(Position2D segmentStart, Position2D segmentEnd, Position2D point, out double t)
    {
        Position2D ab = new Position2D(
            segmentEnd.Easting - segmentStart.Easting,
            segmentEnd.Northing - segmentStart.Northing
        );

        double abLengthSquared = ab.Easting * ab.Easting + ab.Northing * ab.Northing;

        // Handle degenerate segment (start == end)
        if (abLengthSquared < MathConstants.Epsilon)
        {
            t = 0;
            return segmentStart;
        }

        Position2D ap = new Position2D(
            point.Easting - segmentStart.Easting,
            point.Northing - segmentStart.Northing
        );

        double dotProduct = ap.Easting * ab.Easting + ap.Northing * ab.Northing;
        t = Clamp(dotProduct / abLengthSquared, 0.0, 1.0);

        return new Position2D(
            segmentStart.Easting + ab.Easting * t,
            segmentStart.Northing + ab.Northing * t
        );
    }

    /// <summary>
    /// Checks if a point lies on a line segment (within segment bounds).
    /// </summary>
    /// <param name="segmentStart">Segment start position</param>
    /// <param name="segmentEnd">Segment end position</param>
    /// <param name="point">Point to test</param>
    /// <returns>True if point projects onto the segment (t in [0,1])</returns>
    public static bool IsPointOnSegment(Position2D segmentStart, Position2D segmentEnd, Position2D point)
    {
        Position2D ab = new Position2D(
            segmentEnd.Easting - segmentStart.Easting,
            segmentEnd.Northing - segmentStart.Northing
        );

        double abLengthSquared = ab.Easting * ab.Easting + ab.Northing * ab.Northing;

        if (abLengthSquared < MathConstants.Epsilon)
        {
            return false;
        }

        Position2D ap = new Position2D(
            point.Easting - segmentStart.Easting,
            point.Northing - segmentStart.Northing
        );

        double dotProduct = ap.Easting * ab.Easting + ap.Northing * ab.Northing;
        double t = dotProduct / abLengthSquared;

        return t >= 0 && t <= 1;
    }

    #endregion

    #region Clamping and Interpolation

    /// <summary>
    /// Clamps a value between minimum and maximum bounds.
    /// </summary>
    /// <param name="value">Value to clamp</param>
    /// <param name="min">Minimum bound</param>
    /// <param name="max">Maximum bound</param>
    /// <returns>Clamped value</returns>
    public static double Clamp(double value, double min, double max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    /// <summary>
    /// Linear interpolation between two values.
    /// </summary>
    /// <param name="a">Start value</param>
    /// <param name="b">End value</param>
    /// <param name="t">Interpolation factor (0 to 1)</param>
    /// <returns>Interpolated value</returns>
    public static double Lerp(double a, double b, double t)
    {
        return a + (b - a) * t;
    }

    #endregion
}
