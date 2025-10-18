using System;
using System.Collections.Generic;
using System.Linq;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Services.Guidance;

/// <summary>
/// Implements contour recording and guidance operations.
/// Supports real-time contour recording with configurable minimum distance threshold,
/// contour locking with validation, and high-performance guidance calculations.
/// </summary>
/// <remarks>
/// Extracted and refactored from AgOpenGPS CContour.cs with modern patterns:
/// - Event-driven architecture for loose coupling
/// - Interface-based design for testability
/// - Dependency injection compatible
/// - No UI framework dependencies
/// - Performance optimized for 20-25 Hz operation
/// </remarks>
public class ContourService : IContourService
{
    private readonly object _lockObject = new object();
    private ContourLine? _currentContour;
    private double _minDistanceThreshold = 0.5;
    private bool _isRecording = false;
    private bool _isLocked = false;

    /// <summary>
    /// Event fired when contour state changes
    /// </summary>
    public event EventHandler<ContourStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Gets whether a contour is currently being recorded
    /// </summary>
    public bool IsRecording
    {
        get
        {
            lock (_lockObject)
            {
                return _isRecording;
            }
        }
    }

    /// <summary>
    /// Gets whether the current contour is locked for guidance use
    /// </summary>
    public bool IsLocked
    {
        get
        {
            lock (_lockObject)
            {
                return _isLocked;
            }
        }
    }

    /// <summary>
    /// Start recording a new contour line from a starting position
    /// </summary>
    public void StartRecording(Position startPosition, double minDistanceMeters)
    {
        if (startPosition == null)
            throw new ArgumentNullException(nameof(startPosition));

        if (minDistanceMeters < 0)
            throw new ArgumentOutOfRangeException(nameof(minDistanceMeters), "Minimum distance must be non-negative");

        lock (_lockObject)
        {
            if (_isRecording)
                throw new InvalidOperationException("Already recording a contour. Stop or lock the current contour first.");

            _currentContour = new ContourLine
            {
                MinDistanceThreshold = minDistanceMeters,
                IsLocked = false,
                CreatedDate = DateTime.UtcNow
            };

            // Add the starting position
            _currentContour.Points.Add(startPosition);

            _minDistanceThreshold = minDistanceMeters;
            _isRecording = true;
            _isLocked = false;

            // Raise event
            StateChanged?.Invoke(this, new ContourStateChangedEventArgs(
                _currentContour,
                ContourEventType.RecordingStarted,
                _currentContour.Points.Count));
        }
    }

    /// <summary>
    /// Add a new point to the recording contour with offset applied
    /// </summary>
    public void AddPoint(Position currentPosition, double offset)
    {
        if (currentPosition == null)
            throw new ArgumentNullException(nameof(currentPosition));

        lock (_lockObject)
        {
            if (!_isRecording || _currentContour == null)
                throw new InvalidOperationException("Not currently recording a contour. Call StartRecording first.");

            if (_isLocked)
                throw new InvalidOperationException("Cannot add points to a locked contour.");

            // Calculate distance from last recorded point
            var lastPoint = _currentContour.Points[^1];
            double distance = CalculateDistance(lastPoint, currentPosition);

            // Only add point if distance exceeds threshold
            if (distance < _minDistanceThreshold)
            {
                // Skip point - too close to last point
                return;
            }

            // Apply offset to the position
            // For now, we'll record the position as-is
            // Offset could be applied perpendicular to heading if needed in the future
            var recordedPosition = new Position
            {
                Easting = currentPosition.Easting,
                Northing = currentPosition.Northing,
                Altitude = currentPosition.Altitude,
                Latitude = currentPosition.Latitude,
                Longitude = currentPosition.Longitude,
                Zone = currentPosition.Zone,
                Hemisphere = currentPosition.Hemisphere,
                Heading = currentPosition.Heading,
                Speed = currentPosition.Speed
            };

            _currentContour.Points.Add(recordedPosition);

            // Raise event
            StateChanged?.Invoke(this, new ContourStateChangedEventArgs(
                _currentContour,
                ContourEventType.PointAdded,
                _currentContour.Points.Count));
        }
    }

    /// <summary>
    /// Lock the current contour for guidance use
    /// </summary>
    public ContourLine LockContour(string name)
    {
        lock (_lockObject)
        {
            if (!_isRecording || _currentContour == null)
                throw new InvalidOperationException("Not currently recording a contour. Call StartRecording first.");

            if (_isLocked)
                throw new InvalidOperationException("Contour is already locked.");

            // Validate sufficient points
            if (_currentContour.Points.Count < 10)
                throw new ArgumentException(
                    $"Contour must have at least 10 points for reliable following. Current count: {_currentContour.Points.Count}");

            // Set name and lock
            _currentContour.Name = name ?? string.Empty;
            _currentContour.IsLocked = true;
            _isLocked = true;
            _isRecording = false;

            // Validate the contour
            var validation = _currentContour.Validate();
            if (!validation.IsValid)
            {
                // Unlock if validation fails
                _currentContour.IsLocked = false;
                _isLocked = false;
                _isRecording = true;

                throw new ArgumentException(
                    $"Contour validation failed: {string.Join(", ", validation.ErrorMessages)}");
            }

            // Raise event
            StateChanged?.Invoke(this, new ContourStateChangedEventArgs(
                _currentContour,
                ContourEventType.Locked,
                _currentContour.Points.Count));

            return _currentContour;
        }
    }

    /// <summary>
    /// Stop recording the current contour without locking it
    /// </summary>
    public void StopRecording()
    {
        lock (_lockObject)
        {
            _isRecording = false;
            _currentContour = null;
            _isLocked = false;
        }
    }

    /// <summary>
    /// Calculate guidance information from a contour line
    /// </summary>
    public GuidanceLineResult CalculateGuidance(Position currentPosition, double currentHeading, ContourLine contour)
    {
        if (currentPosition == null)
            throw new ArgumentNullException(nameof(currentPosition));

        if (contour == null)
            throw new ArgumentNullException(nameof(contour));

        if (!contour.IsLocked)
            throw new ArgumentException("Contour must be locked before calculating guidance.", nameof(contour));

        if (contour.Points.Count < 2)
            throw new ArgumentException("Contour must have at least 2 points for guidance.", nameof(contour));

        // Find closest point on contour
        int closestIndex = FindClosestPointIndex(currentPosition, contour);
        var closestPoint = contour.Points[closestIndex];

        // Calculate perpendicular distance to contour segment
        double crossTrackError = CalculatePerpendicularDistance(currentPosition, contour, closestIndex);

        // Calculate look-ahead point for smooth following
        const double lookAheadDistance = 5.0; // meters
        int lookAheadIndex = FindLookAheadPoint(closestIndex, lookAheadDistance, contour);
        var lookAheadPoint = contour.Points[lookAheadIndex];

        // Calculate heading to look-ahead point
        double dx = lookAheadPoint.Easting - currentPosition.Easting;
        double dy = lookAheadPoint.Northing - currentPosition.Northing;
        double targetHeading = Math.Atan2(dy, dx) * (180.0 / Math.PI); // Convert to degrees

        // Normalize to 0-360 range
        if (targetHeading < 0) targetHeading += 360.0;

        // Calculate heading error
        double headingError = targetHeading - currentHeading;

        // Normalize heading error to -180 to +180 range
        while (headingError > 180.0) headingError -= 360.0;
        while (headingError < -180.0) headingError += 360.0;

        return new GuidanceLineResult
        {
            CrossTrackError = crossTrackError,
            ClosestPoint = closestPoint,
            HeadingError = headingError,
            DistanceToLine = Math.Abs(crossTrackError),
            ClosestPointIndex = closestIndex
        };
    }

    /// <summary>
    /// Calculate the perpendicular offset from current position to contour path
    /// </summary>
    public double CalculateOffset(Position currentPosition, ContourLine contour)
    {
        if (currentPosition == null)
            throw new ArgumentNullException(nameof(currentPosition));

        if (contour == null)
            throw new ArgumentNullException(nameof(contour));

        if (contour.Points.Count < 2)
            return 0.0;

        // Find closest point on contour
        int closestIndex = FindClosestPointIndex(currentPosition, contour);

        // Calculate perpendicular distance
        return CalculatePerpendicularDistance(currentPosition, contour, closestIndex);
    }

    /// <summary>
    /// Set the locked state of the current contour
    /// </summary>
    public void SetLocked(bool locked)
    {
        lock (_lockObject)
        {
            if (_currentContour == null)
                return;

            bool wasLocked = _isLocked;
            _isLocked = locked;
            _currentContour.IsLocked = locked;

            if (wasLocked != locked)
            {
                // Raise event
                StateChanged?.Invoke(this, new ContourStateChangedEventArgs(
                    _currentContour,
                    locked ? ContourEventType.Locked : ContourEventType.Unlocked,
                    _currentContour.Points.Count));
            }
        }
    }

    /// <summary>
    /// Validate a contour line for correctness
    /// </summary>
    public ValidationResult ValidateContour(ContourLine contour)
    {
        if (contour == null)
            throw new ArgumentNullException(nameof(contour));

        return contour.Validate();
    }

    #region Private Helper Methods

    /// <summary>
    /// Calculate Euclidean distance between two positions
    /// </summary>
    private double CalculateDistance(Position from, Position to)
    {
        double dx = to.Easting - from.Easting;
        double dy = to.Northing - from.Northing;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Find the index of the closest point on the contour to the current position
    /// </summary>
    private int FindClosestPointIndex(Position currentPosition, ContourLine contour)
    {
        int closestIndex = 0;
        double minDistance = double.MaxValue;

        for (int i = 0; i < contour.Points.Count; i++)
        {
            double distance = CalculateDistance(currentPosition, contour.Points[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    /// <summary>
    /// Calculate perpendicular distance from position to contour segment
    /// </summary>
    private double CalculatePerpendicularDistance(Position position, ContourLine contour, int closestIndex)
    {
        // Determine which segment to use (before or after closest point)
        int segmentStart = closestIndex;
        int segmentEnd = closestIndex + 1;

        // Handle edge cases
        if (closestIndex == contour.Points.Count - 1)
        {
            // Last point - use segment before it
            segmentStart = closestIndex - 1;
            segmentEnd = closestIndex;
        }

        if (segmentStart < 0)
        {
            // First point - distance to first point
            return CalculateDistance(position, contour.Points[0]);
        }

        var p1 = contour.Points[segmentStart];
        var p2 = contour.Points[segmentEnd];

        // Calculate perpendicular distance to line segment
        double dx = p2.Easting - p1.Easting;
        double dy = p2.Northing - p1.Northing;
        double lengthSq = dx * dx + dy * dy;

        if (lengthSq < 0.0001) // Segment is essentially a point
            return CalculateDistance(position, p1);

        // Calculate projection parameter
        double t = ((position.Easting - p1.Easting) * dx + (position.Northing - p1.Northing) * dy) / lengthSq;
        t = Math.Max(0, Math.Min(1, t)); // Clamp to segment

        // Calculate closest point on segment
        double projX = p1.Easting + t * dx;
        double projY = p1.Northing + t * dy;

        // Calculate signed distance (positive = right, negative = left)
        double distX = position.Easting - projX;
        double distY = position.Northing - projY;
        double distance = Math.Sqrt(distX * distX + distY * distY);

        // Calculate sign using cross product
        double cross = dx * distY - dy * distX;
        return cross >= 0 ? distance : -distance;
    }

    /// <summary>
    /// Find look-ahead point on contour for smooth following
    /// </summary>
    private int FindLookAheadPoint(int startIndex, double lookAheadDistance, ContourLine contour)
    {
        double accumulatedDistance = 0.0;
        int currentIndex = startIndex;

        // Look forward along contour
        while (currentIndex < contour.Points.Count - 1)
        {
            double segmentLength = CalculateDistance(contour.Points[currentIndex], contour.Points[currentIndex + 1]);
            accumulatedDistance += segmentLength;

            if (accumulatedDistance >= lookAheadDistance)
            {
                return currentIndex + 1;
            }

            currentIndex++;
        }

        // Return last point if we reach the end
        return contour.Points.Count - 1;
    }

    #endregion
}
