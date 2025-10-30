using System;

namespace AgValoniaGPS.Models.Guidance;

/// <summary>
/// Represents a guidance track (AB line, curve, contour, etc.)
/// Used by TrackManagementService for collection management and switching.
/// </summary>
public class GuidanceTrack
{
    /// <summary>
    /// Unique identifier for this track
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Human-readable name for the track
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mode/type of this track
    /// </summary>
    public TrackMode Mode { get; set; }

    /// <summary>
    /// Reference to the underlying ABLine (if Mode is ABLine)
    /// </summary>
    public ABLine? ABLine { get; set; }

    /// <summary>
    /// Reference to the underlying CurveLine (if Mode is CurveLine)
    /// </summary>
    public CurveLine? CurveLine { get; set; }

    /// <summary>
    /// Reference to the underlying ContourLine (if Mode is Contour)
    /// </summary>
    public ContourLine? ContourLine { get; set; }

    /// <summary>
    /// Reference to the underlying Headline (if Mode is Headline)
    /// </summary>
    public Headline? Headline { get; set; }

    /// <summary>
    /// Whether this track has been worked/covered
    /// </summary>
    public bool IsWorked { get; set; }

    /// <summary>
    /// Perpendicular offset applied to this track (in meters)
    /// Positive = right offset, Negative = left offset
    /// </summary>
    public double NudgeOffset { get; set; }

    /// <summary>
    /// Date and time this track was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time this track was last used/selected
    /// </summary>
    public DateTime? LastUsedDate { get; set; }

    /// <summary>
    /// Whether this track is currently active/selected
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Distance from vehicle to this track (calculated dynamically)
    /// </summary>
    public double DistanceFromVehicle { get; set; }

    /// <summary>
    /// Color for rendering this track (ARGB format)
    /// </summary>
    public uint Color { get; set; } = 0xFF00FF00; // Default: Green

    /// <summary>
    /// Gets a reference to the underlying guidance object based on Mode
    /// </summary>
    public object? GetUnderlyingGuidance()
    {
        return Mode switch
        {
            TrackMode.ABLine => ABLine,
            TrackMode.CurveLine => CurveLine,
            TrackMode.Contour => ContourLine,
            TrackMode.Headline => Headline,
            _ => null
        };
    }

    /// <summary>
    /// Creates a GuidanceTrack from an ABLine
    /// </summary>
    public static GuidanceTrack FromABLine(int id, ABLine abLine)
    {
        return new GuidanceTrack
        {
            Id = id,
            Name = abLine.Name,
            Mode = TrackMode.ABLine,
            ABLine = abLine,
            CreatedDate = abLine.CreatedDate,
            NudgeOffset = abLine.NudgeOffset
        };
    }

    /// <summary>
    /// Creates a GuidanceTrack from a CurveLine
    /// </summary>
    public static GuidanceTrack FromCurveLine(int id, CurveLine curveLine)
    {
        return new GuidanceTrack
        {
            Id = id,
            Name = curveLine.Name,
            Mode = TrackMode.CurveLine,
            CurveLine = curveLine,
            CreatedDate = curveLine.CreatedDate
        };
    }

    /// <summary>
    /// Creates a GuidanceTrack from a ContourLine
    /// </summary>
    public static GuidanceTrack FromContourLine(int id, ContourLine contourLine)
    {
        return new GuidanceTrack
        {
            Id = id,
            Name = contourLine.Name,
            Mode = TrackMode.Contour,
            ContourLine = contourLine,
            CreatedDate = contourLine.CreatedDate
        };
    }

    /// <summary>
    /// Creates a GuidanceTrack from a Headline
    /// </summary>
    public static GuidanceTrack FromHeadline(int id, Headline headline)
    {
        return new GuidanceTrack
        {
            Id = id,
            Name = headline.Name,
            Mode = TrackMode.Headline,
            Headline = headline,
            CreatedDate = headline.CreatedAt,
            NudgeOffset = headline.MoveDistance
        };
    }
}
