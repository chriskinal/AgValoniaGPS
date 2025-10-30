using System;
using System.Numerics;

namespace AgValoniaGPS.Services.Rendering;

/// <summary>
/// Service interface for managing camera state, transformations, and controls for the OpenGL map view.
/// Handles camera position, zoom, rotation, viewport dimensions, and coordinate conversions.
/// </summary>
public interface ICameraService
{
    /// <summary>
    /// Gets or sets the camera position in world coordinates (meters).
    /// </summary>
    Vector2 Position { get; set; }

    /// <summary>
    /// Gets or sets the zoom level in meters per pixel.
    /// Higher values = zoomed out (more area visible), lower values = zoomed in (less area visible).
    /// </summary>
    float ZoomLevel { get; set; }

    /// <summary>
    /// Gets or sets the camera rotation in radians.
    /// 0 = North up, positive rotation = clockwise.
    /// </summary>
    float Rotation { get; set; }

    /// <summary>
    /// Gets or sets the viewport width in pixels.
    /// </summary>
    int ViewportWidth { get; set; }

    /// <summary>
    /// Gets or sets the viewport height in pixels.
    /// </summary>
    int ViewportHeight { get; set; }

    /// <summary>
    /// Gets or sets whether the camera should automatically follow the vehicle.
    /// When enabled, camera position is updated to match vehicle position.
    /// </summary>
    bool FollowVehicle { get; set; }

    /// <summary>
    /// Gets or sets the current vehicle position in world coordinates.
    /// Used for follow vehicle mode.
    /// </summary>
    Vector2? VehiclePosition { get; set; }

    /// <summary>
    /// Calculates the view matrix that transforms world coordinates to camera space.
    /// Applies camera position translation and rotation.
    /// </summary>
    /// <returns>4x4 view transformation matrix</returns>
    Matrix4x4 GetViewMatrix();

    /// <summary>
    /// Calculates the orthographic projection matrix that transforms camera space to normalized device coordinates (NDC).
    /// Takes into account zoom level and viewport dimensions.
    /// </summary>
    /// <returns>4x4 projection matrix</returns>
    Matrix4x4 GetProjectionMatrix();

    /// <summary>
    /// Calculates the combined view-projection matrix.
    /// This is the product of the projection matrix and view matrix.
    /// </summary>
    /// <returns>4x4 view-projection matrix</returns>
    Matrix4x4 GetViewProjectionMatrix();

    /// <summary>
    /// Converts screen pixel coordinates to world coordinates.
    /// </summary>
    /// <param name="screenPoint">Screen coordinates (0,0 = top-left, Y increases downward)</param>
    /// <returns>World coordinates in meters</returns>
    Vector2 ScreenToWorld(Vector2 screenPoint);

    /// <summary>
    /// Converts world coordinates to screen pixel coordinates.
    /// </summary>
    /// <param name="worldPoint">World coordinates in meters</param>
    /// <returns>Screen coordinates (0,0 = top-left, Y increases downward)</returns>
    Vector2 WorldToScreen(Vector2 worldPoint);

    /// <summary>
    /// Pans the camera by the specified delta in world coordinates.
    /// </summary>
    /// <param name="delta">Movement delta in world coordinates (meters)</param>
    void Pan(Vector2 delta);

    /// <summary>
    /// Zooms the camera by the specified factor.
    /// </summary>
    /// <param name="factor">Zoom factor (e.g., 1.1 = zoom in 10%, 0.9 = zoom out 10%)</param>
    /// <param name="focusPoint">Optional screen point to zoom toward. If null, zooms toward screen center.</param>
    void Zoom(float factor, Vector2? focusPoint = null);

    /// <summary>
    /// Rotates the camera by the specified angle delta.
    /// </summary>
    /// <param name="angleDelta">Rotation delta in radians (positive = clockwise)</param>
    void Rotate(float angleDelta);

    /// <summary>
    /// Adjusts camera position and zoom to fit the specified bounding box in the viewport.
    /// Adds 10% padding around the bounds.
    /// </summary>
    /// <param name="bounds">Bounding box in world coordinates to fit</param>
    void FitBounds(BoundingBox bounds);

    /// <summary>
    /// Centers the camera on the specified world point.
    /// </summary>
    /// <param name="worldPoint">World coordinates to center on (meters)</param>
    void CenterOn(Vector2 worldPoint);

    /// <summary>
    /// Event raised when camera state changes (position, zoom, or rotation).
    /// </summary>
    event EventHandler<CameraChangedEventArgs> CameraChanged;
}
