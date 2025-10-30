using System;
using System.Numerics;

namespace AgValoniaGPS.Services.Rendering;

/// <summary>
/// Event arguments for camera state changes.
/// Contains the new camera position, zoom level, and rotation.
/// </summary>
public class CameraChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the new camera position in world coordinates (meters).
    /// </summary>
    public Vector2 Position { get; init; }

    /// <summary>
    /// Gets the new zoom level in meters per pixel.
    /// </summary>
    public float ZoomLevel { get; init; }

    /// <summary>
    /// Gets the new camera rotation in radians.
    /// </summary>
    public float Rotation { get; init; }

    /// <summary>
    /// Initializes a new instance of the CameraChangedEventArgs class.
    /// </summary>
    /// <param name="position">Camera position in world coordinates</param>
    /// <param name="zoomLevel">Zoom level in meters per pixel</param>
    /// <param name="rotation">Camera rotation in radians</param>
    public CameraChangedEventArgs(Vector2 position, float zoomLevel, float rotation)
    {
        Position = position;
        ZoomLevel = zoomLevel;
        Rotation = rotation;
    }
}
