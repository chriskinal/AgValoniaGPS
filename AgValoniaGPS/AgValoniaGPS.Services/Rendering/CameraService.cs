using System;
using System.Numerics;

namespace AgValoniaGPS.Services.Rendering;

/// <summary>
/// Service implementation for managing camera state, transformations, and controls.
/// Handles camera position, zoom, rotation, viewport dimensions, coordinate conversions,
/// and camera control operations (pan, zoom, rotate, fit bounds, center).
/// </summary>
public class CameraService : ICameraService
{
    private Vector2 _position;
    private float _zoomLevel;
    private float _rotation;
    private int _viewportWidth;
    private int _viewportHeight;
    private bool _followVehicle;
    private Vector2? _vehiclePosition;

    /// <summary>
    /// Minimum allowed zoom level in meters per pixel (max zoom in).
    /// </summary>
    public const float MinZoomLevel = 0.01f;

    /// <summary>
    /// Maximum allowed zoom level in meters per pixel (max zoom out).
    /// </summary>
    public const float MaxZoomLevel = 100.0f;

    /// <summary>
    /// Initializes a new instance of the CameraService class with default values.
    /// Default position: (0, 0), zoom: 1.0 m/px, rotation: 0 rad, viewport: 800x600.
    /// </summary>
    public CameraService()
    {
        _position = Vector2.Zero;
        _zoomLevel = 1.0f;
        _rotation = 0.0f;
        _viewportWidth = 800;
        _viewportHeight = 600;
        _followVehicle = false;
        _vehiclePosition = null;
    }

    /// <inheritdoc />
    public Vector2 Position
    {
        get => _position;
        set
        {
            if (_position != value)
            {
                _position = value;
                OnCameraChanged();
            }
        }
    }

    /// <inheritdoc />
    public float ZoomLevel
    {
        get => _zoomLevel;
        set
        {
            float clampedZoom = Math.Clamp(value, MinZoomLevel, MaxZoomLevel);
            if (_zoomLevel != clampedZoom)
            {
                _zoomLevel = clampedZoom;
                OnCameraChanged();
            }
        }
    }

    /// <inheritdoc />
    public float Rotation
    {
        get => _rotation;
        set
        {
            if (_rotation != value)
            {
                _rotation = value;
                OnCameraChanged();
            }
        }
    }

    /// <inheritdoc />
    public int ViewportWidth
    {
        get => _viewportWidth;
        set
        {
            if (_viewportWidth != value && value > 0)
            {
                _viewportWidth = value;
                // Viewport changes don't trigger CameraChanged event
                // as they are typically driven by window resize
            }
        }
    }

    /// <inheritdoc />
    public int ViewportHeight
    {
        get => _viewportHeight;
        set
        {
            if (_viewportHeight != value && value > 0)
            {
                _viewportHeight = value;
                // Viewport changes don't trigger CameraChanged event
            }
        }
    }

    /// <inheritdoc />
    public bool FollowVehicle
    {
        get => _followVehicle;
        set => _followVehicle = value;
    }

    /// <inheritdoc />
    public Vector2? VehiclePosition
    {
        get => _vehiclePosition;
        set
        {
            _vehiclePosition = value;

            // If follow vehicle is enabled and we have a vehicle position, update camera
            if (_followVehicle && _vehiclePosition.HasValue)
            {
                Position = _vehiclePosition.Value;
            }
        }
    }

    /// <inheritdoc />
    public event EventHandler<CameraChangedEventArgs> CameraChanged;

    /// <inheritdoc />
    public Matrix4x4 GetViewMatrix()
    {
        // View matrix transforms world coordinates to camera space
        // Order of operations: Translate to -Position, then Rotate by -Rotation

        // Create translation matrix to move world to camera origin
        Matrix4x4 translation = Matrix4x4.CreateTranslation(-_position.X, -_position.Y, 0);

        // Create rotation matrix around Z axis (negative rotation for view)
        Matrix4x4 rotation = Matrix4x4.CreateRotationZ(-_rotation);

        // Combine: ViewMatrix = Rotation * Translation
        return Matrix4x4.Multiply(translation, rotation);
    }

    /// <inheritdoc />
    public Matrix4x4 GetProjectionMatrix()
    {
        // Orthographic projection matrix for 2D rendering
        // Maps camera space to normalized device coordinates (NDC)

        // Calculate visible area in world coordinates
        float widthInWorld = _viewportWidth * _zoomLevel;
        float heightInWorld = _viewportHeight * _zoomLevel;

        // Create orthographic projection
        // For 2D, we use near=-1.0 and far=1.0
        return Matrix4x4.CreateOrthographic(widthInWorld, heightInWorld, -1.0f, 1.0f);
    }

    /// <inheritdoc />
    public Matrix4x4 GetViewProjectionMatrix()
    {
        // Combined view-projection matrix
        // Order: ViewProjection = Projection * View
        return Matrix4x4.Multiply(GetViewMatrix(), GetProjectionMatrix());
    }

    /// <inheritdoc />
    public Vector2 ScreenToWorld(Vector2 screenPoint)
    {
        // Convert screen pixel coordinates to world coordinates

        // Step 1: Convert screen (0,0 at top-left) to NDC (-1 to +1)
        // Screen: (0, 0) = top-left, (width, height) = bottom-right
        // NDC: (-1, -1) = bottom-left, (1, 1) = top-right
        float ndcX = (screenPoint.X / _viewportWidth) * 2.0f - 1.0f;
        float ndcY = 1.0f - (screenPoint.Y / _viewportHeight) * 2.0f;  // Flip Y

        // Step 2: Apply inverse view-projection matrix
        Matrix4x4 vpMatrix = GetViewProjectionMatrix();
        if (!Matrix4x4.Invert(vpMatrix, out Matrix4x4 vpInverse))
        {
            // If matrix is not invertible, return camera position as fallback
            return _position;
        }

        // Transform NDC to world coordinates
        Vector4 worldPos = Vector4.Transform(new Vector4(ndcX, ndcY, 0, 1), vpInverse);

        return new Vector2(worldPos.X, worldPos.Y);
    }

    /// <inheritdoc />
    public Vector2 WorldToScreen(Vector2 worldPoint)
    {
        // Convert world coordinates to screen pixel coordinates

        // Step 1: Apply view-projection matrix to transform to clip space
        Vector4 clipPos = Vector4.Transform(
            new Vector4(worldPoint.X, worldPoint.Y, 0, 1),
            GetViewProjectionMatrix()
        );

        // Step 2: Convert to NDC (clip space to NDC by dividing by W)
        float ndcX = clipPos.X / clipPos.W;
        float ndcY = clipPos.Y / clipPos.W;

        // Step 3: Convert NDC to screen coordinates
        // NDC: (-1, -1) = bottom-left, (1, 1) = top-right
        // Screen: (0, 0) = top-left, (width, height) = bottom-right
        float screenX = (ndcX + 1.0f) * 0.5f * _viewportWidth;
        float screenY = (1.0f - ndcY) * 0.5f * _viewportHeight;  // Flip Y

        return new Vector2(screenX, screenY);
    }

    /// <inheritdoc />
    public void Pan(Vector2 delta)
    {
        Position += delta;
    }

    /// <inheritdoc />
    public void Zoom(float factor, Vector2? focusPoint = null)
    {
        if (focusPoint.HasValue)
        {
            // Zoom toward a specific screen point (e.g., mouse cursor)

            // Convert focus point to world coordinates at current zoom
            Vector2 worldFocus = ScreenToWorld(focusPoint.Value);

            // Calculate offset from camera to focus point
            Vector2 offset = worldFocus - _position;

            // Update zoom level
            float newZoomLevel = Math.Clamp(_zoomLevel * factor, MinZoomLevel, MaxZoomLevel);

            // Adjust position to keep focus point stationary on screen
            // When zooming in (factor < 1), we want to move toward the focus point
            // When zooming out (factor > 1), we want to move away from the focus point
            _position = worldFocus - offset * (newZoomLevel / _zoomLevel);

            _zoomLevel = newZoomLevel;
        }
        else
        {
            // Simple zoom without focus point (zoom toward screen center)
            ZoomLevel *= factor;
        }

        OnCameraChanged();
    }

    /// <inheritdoc />
    public void Rotate(float angleDelta)
    {
        _rotation += angleDelta;

        // Normalize rotation to [0, 2π)
        while (_rotation < 0)
        {
            _rotation += MathF.PI * 2;
        }
        while (_rotation >= MathF.PI * 2)
        {
            _rotation -= MathF.PI * 2;
        }

        OnCameraChanged();
    }

    /// <inheritdoc />
    public void FitBounds(BoundingBox bounds)
    {
        // Center camera on bounds center
        _position = bounds.Center;

        // Calculate zoom to fit bounds (with 10% padding)
        float paddingFactor = 1.1f;
        float zoomX = bounds.Width / _viewportWidth * paddingFactor;
        float zoomY = bounds.Height / _viewportHeight * paddingFactor;

        // Use the larger zoom to ensure entire bounds fit
        _zoomLevel = Math.Max(zoomX, zoomY);
        _zoomLevel = Math.Clamp(_zoomLevel, MinZoomLevel, MaxZoomLevel);

        OnCameraChanged();
    }

    /// <inheritdoc />
    public void CenterOn(Vector2 worldPoint)
    {
        Position = worldPoint;
    }

    /// <summary>
    /// Gets the visible bounds of the camera in world coordinates.
    /// Used for frustum culling to skip rendering objects outside the view.
    /// </summary>
    /// <returns>A bounding box representing the visible area in world space</returns>
    public BoundingBox GetVisibleBounds()
    {
        // Calculate the visible area in world coordinates
        float halfWidth = _viewportWidth * _zoomLevel / 2.0f;
        float halfHeight = _viewportHeight * _zoomLevel / 2.0f;

        // TODO: Account for rotation (requires transforming corners)
        // For now, use axis-aligned bounding box
        return new BoundingBox(
            _position.X - halfWidth,
            _position.Y - halfHeight,
            _position.X + halfWidth,
            _position.Y + halfHeight
        );
    }

    /// <summary>
    /// Raises the CameraChanged event with current camera state.
    /// </summary>
    private void OnCameraChanged()
    {
        CameraChanged?.Invoke(this, new CameraChangedEventArgs(_position, _zoomLevel, _rotation));
    }
}
