using System;
using System.Numerics;
using AgValoniaGPS.Services.Rendering;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.Rendering;

[TestFixture]
public class CameraServiceTests
{
    private CameraService _cameraService;

    [SetUp]
    public void SetUp()
    {
        _cameraService = new CameraService();
    }

    #region Property Tests

    [Test]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Assert
        Assert.That(_cameraService.Position, Is.EqualTo(Vector2.Zero));
        Assert.That(_cameraService.ZoomLevel, Is.EqualTo(1.0f));
        Assert.That(_cameraService.Rotation, Is.EqualTo(0.0f));
        Assert.That(_cameraService.ViewportWidth, Is.EqualTo(800));
        Assert.That(_cameraService.ViewportHeight, Is.EqualTo(600));
        Assert.That(_cameraService.FollowVehicle, Is.False);
        Assert.That(_cameraService.VehiclePosition, Is.Null);
    }

    [Test]
    public void SetPosition_UpdatesPosition_AndFiresEvent()
    {
        // Arrange
        var newPosition = new Vector2(100, 200);
        bool eventFired = false;
        CameraChangedEventArgs eventArgs = null;

        _cameraService.CameraChanged += (sender, e) =>
        {
            eventFired = true;
            eventArgs = e;
        };

        // Act
        _cameraService.Position = newPosition;

        // Assert
        Assert.That(_cameraService.Position, Is.EqualTo(newPosition));
        Assert.That(eventFired, Is.True);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs.Position, Is.EqualTo(newPosition));
        Assert.That(eventArgs.ZoomLevel, Is.EqualTo(1.0f));
        Assert.That(eventArgs.Rotation, Is.EqualTo(0.0f));
    }

    [Test]
    public void SetPosition_SameValue_DoesNotFireEvent()
    {
        // Arrange
        _cameraService.Position = new Vector2(100, 200);
        bool eventFired = false;
        _cameraService.CameraChanged += (sender, e) => eventFired = true;

        // Act
        _cameraService.Position = new Vector2(100, 200);

        // Assert
        Assert.That(eventFired, Is.False);
    }

    [Test]
    public void SetZoomLevel_UpdatesZoomLevel_AndFiresEvent()
    {
        // Arrange
        float newZoom = 2.5f;
        bool eventFired = false;
        CameraChangedEventArgs eventArgs = null;

        _cameraService.CameraChanged += (sender, e) =>
        {
            eventFired = true;
            eventArgs = e;
        };

        // Act
        _cameraService.ZoomLevel = newZoom;

        // Assert
        Assert.That(_cameraService.ZoomLevel, Is.EqualTo(newZoom));
        Assert.That(eventFired, Is.True);
        Assert.That(eventArgs.ZoomLevel, Is.EqualTo(newZoom));
    }

    [Test]
    public void SetZoomLevel_ClampsToBounds()
    {
        // Test min clamp
        _cameraService.ZoomLevel = 0.001f;
        Assert.That(_cameraService.ZoomLevel, Is.EqualTo(CameraService.MinZoomLevel));

        // Test max clamp
        _cameraService.ZoomLevel = 200.0f;
        Assert.That(_cameraService.ZoomLevel, Is.EqualTo(CameraService.MaxZoomLevel));
    }

    [Test]
    public void SetRotation_UpdatesRotation_AndFiresEvent()
    {
        // Arrange
        float newRotation = MathF.PI / 4;
        bool eventFired = false;
        CameraChangedEventArgs eventArgs = null;

        _cameraService.CameraChanged += (sender, e) =>
        {
            eventFired = true;
            eventArgs = e;
        };

        // Act
        _cameraService.Rotation = newRotation;

        // Assert
        Assert.That(_cameraService.Rotation, Is.EqualTo(newRotation));
        Assert.That(eventFired, Is.True);
        Assert.That(eventArgs.Rotation, Is.EqualTo(newRotation));
    }

    [Test]
    public void SetViewportWidth_UpdatesWidth()
    {
        // Act
        _cameraService.ViewportWidth = 1920;

        // Assert
        Assert.That(_cameraService.ViewportWidth, Is.EqualTo(1920));
    }

    [Test]
    public void SetViewportHeight_UpdatesHeight()
    {
        // Act
        _cameraService.ViewportHeight = 1080;

        // Assert
        Assert.That(_cameraService.ViewportHeight, Is.EqualTo(1080));
    }

    [Test]
    public void SetViewportWidth_InvalidValue_DoesNotUpdate()
    {
        // Act
        _cameraService.ViewportWidth = -100;

        // Assert
        Assert.That(_cameraService.ViewportWidth, Is.EqualTo(800)); // Stays at default
    }

    #endregion

    #region Follow Vehicle Tests

    [Test]
    public void SetVehiclePosition_FollowVehicleEnabled_UpdatesCameraPosition()
    {
        // Arrange
        _cameraService.FollowVehicle = true;
        var vehiclePos = new Vector2(500, 600);

        // Act
        _cameraService.VehiclePosition = vehiclePos;

        // Assert
        Assert.That(_cameraService.Position, Is.EqualTo(vehiclePos));
        Assert.That(_cameraService.VehiclePosition, Is.EqualTo(vehiclePos));
    }

    [Test]
    public void SetVehiclePosition_FollowVehicleDisabled_DoesNotUpdateCameraPosition()
    {
        // Arrange
        _cameraService.FollowVehicle = false;
        var originalPosition = _cameraService.Position;
        var vehiclePos = new Vector2(500, 600);

        // Act
        _cameraService.VehiclePosition = vehiclePos;

        // Assert
        Assert.That(_cameraService.Position, Is.EqualTo(originalPosition));
        Assert.That(_cameraService.VehiclePosition, Is.EqualTo(vehiclePos));
    }

    #endregion

    #region Matrix Calculation Tests

    [Test]
    public void GetViewMatrix_AtOriginWithZeroRotation_ReturnsIdentity()
    {
        // Arrange
        _cameraService.Position = Vector2.Zero;
        _cameraService.Rotation = 0.0f;

        // Act
        var viewMatrix = _cameraService.GetViewMatrix();

        // Assert
        Assert.That(viewMatrix, Is.EqualTo(Matrix4x4.Identity).Within(0.0001f));
    }

    [Test]
    public void GetViewMatrix_WithTranslation_TranslatesCorrectly()
    {
        // Arrange
        _cameraService.Position = new Vector2(100, 200);
        _cameraService.Rotation = 0.0f;

        // Act
        var viewMatrix = _cameraService.GetViewMatrix();

        // Transform a test point
        var worldPoint = new Vector4(100, 200, 0, 1);
        var cameraPoint = Vector4.Transform(worldPoint, viewMatrix);

        // Assert - camera position should map to origin in camera space
        Assert.That(cameraPoint.X, Is.EqualTo(0).Within(0.001f));
        Assert.That(cameraPoint.Y, Is.EqualTo(0).Within(0.001f));
    }

    [Test]
    public void GetViewMatrix_WithRotation_RotatesCorrectly()
    {
        // Arrange
        _cameraService.Position = Vector2.Zero;
        _cameraService.Rotation = MathF.PI / 2; // 90 degrees

        // Act
        var viewMatrix = _cameraService.GetViewMatrix();

        // Transform a test point (1, 0) should become (0, -1) after 90° CCW view rotation
        var worldPoint = new Vector4(1, 0, 0, 1);
        var cameraPoint = Vector4.Transform(worldPoint, viewMatrix);

        // Assert
        Assert.That(cameraPoint.X, Is.EqualTo(0).Within(0.001f));
        Assert.That(cameraPoint.Y, Is.EqualTo(-1).Within(0.001f));
    }

    [Test]
    public void GetProjectionMatrix_CreatesOrthographicMatrix()
    {
        // Arrange
        _cameraService.ViewportWidth = 800;
        _cameraService.ViewportHeight = 600;
        _cameraService.ZoomLevel = 1.0f;

        // Act
        var projMatrix = _cameraService.GetProjectionMatrix();

        // Assert - verify it's a valid matrix (not zero or NaN)
        Assert.That(float.IsNaN(projMatrix.M11), Is.False);
        Assert.That(float.IsNaN(projMatrix.M22), Is.False);
        Assert.That(projMatrix.M11, Is.Not.EqualTo(0));
        Assert.That(projMatrix.M22, Is.Not.EqualTo(0));
    }

    [Test]
    public void GetProjectionMatrix_DifferentZoomLevels_ScalesCorrectly()
    {
        // Arrange
        _cameraService.ViewportWidth = 800;
        _cameraService.ViewportHeight = 600;

        // Act
        _cameraService.ZoomLevel = 1.0f;
        var proj1 = _cameraService.GetProjectionMatrix();

        _cameraService.ZoomLevel = 2.0f;
        var proj2 = _cameraService.GetProjectionMatrix();

        // Assert - M11 and M22 should scale inversely with zoom
        // (higher zoom = more area visible = smaller scale factor)
        Assert.That(proj2.M11, Is.LessThan(proj1.M11));
        Assert.That(proj2.M22, Is.LessThan(proj1.M22));
    }

    [Test]
    public void GetViewProjectionMatrix_CombinesViewAndProjection()
    {
        // Arrange
        _cameraService.Position = new Vector2(100, 100);
        _cameraService.Rotation = 0.0f;
        _cameraService.ZoomLevel = 1.0f;

        // Act
        var viewMatrix = _cameraService.GetViewMatrix();
        var projMatrix = _cameraService.GetProjectionMatrix();
        var vpMatrix = _cameraService.GetViewProjectionMatrix();

        // Calculate expected result
        var expectedVP = Matrix4x4.Multiply(viewMatrix, projMatrix);

        // Assert
        Assert.That(vpMatrix, Is.EqualTo(expectedVP).Within(0.0001f));
    }

    #endregion

    #region Coordinate Transformation Tests

    [Test]
    public void ScreenToWorld_CenterScreen_MapsToCameraPosition()
    {
        // Arrange
        _cameraService.Position = new Vector2(1000, 2000);
        _cameraService.ViewportWidth = 800;
        _cameraService.ViewportHeight = 600;
        _cameraService.ZoomLevel = 1.0f;
        _cameraService.Rotation = 0.0f;

        // Center of screen
        var screenCenter = new Vector2(400, 300);

        // Act
        var worldPoint = _cameraService.ScreenToWorld(screenCenter);

        // Assert - screen center should map to camera position
        Assert.That(worldPoint.X, Is.EqualTo(1000).Within(0.1f));
        Assert.That(worldPoint.Y, Is.EqualTo(2000).Within(0.1f));
    }

    [Test]
    public void WorldToScreen_CameraPosition_MapsToCenterScreen()
    {
        // Arrange
        _cameraService.Position = new Vector2(1000, 2000);
        _cameraService.ViewportWidth = 800;
        _cameraService.ViewportHeight = 600;
        _cameraService.ZoomLevel = 1.0f;
        _cameraService.Rotation = 0.0f;

        // Act
        var screenPoint = _cameraService.WorldToScreen(_cameraService.Position);

        // Assert - camera position should map to screen center
        Assert.That(screenPoint.X, Is.EqualTo(400).Within(0.1f));
        Assert.That(screenPoint.Y, Is.EqualTo(300).Within(0.1f));
    }

    [Test]
    public void CoordinateConversion_RoundTrip_IsAccurate()
    {
        // Arrange
        _cameraService.Position = new Vector2(500, 750);
        _cameraService.ViewportWidth = 1920;
        _cameraService.ViewportHeight = 1080;
        _cameraService.ZoomLevel = 0.5f;
        _cameraService.Rotation = 0.0f;

        var originalWorld = new Vector2(1234.5f, 5678.9f);

        // Act - round trip conversion
        var screen = _cameraService.WorldToScreen(originalWorld);
        var backToWorld = _cameraService.ScreenToWorld(screen);

        // Assert - should be accurate within small epsilon
        Assert.That(backToWorld.X, Is.EqualTo(originalWorld.X).Within(0.01f));
        Assert.That(backToWorld.Y, Is.EqualTo(originalWorld.Y).Within(0.01f));
    }

    [Test]
    public void CoordinateConversion_WithRotation_WorksCorrectly()
    {
        // Arrange
        _cameraService.Position = Vector2.Zero;
        _cameraService.ViewportWidth = 800;
        _cameraService.ViewportHeight = 600;
        _cameraService.ZoomLevel = 1.0f;
        _cameraService.Rotation = MathF.PI / 4; // 45 degrees

        var worldPoint = new Vector2(100, 100);

        // Act - round trip conversion
        var screen = _cameraService.WorldToScreen(worldPoint);
        var backToWorld = _cameraService.ScreenToWorld(screen);

        // Assert - rotation should not break round-trip accuracy
        Assert.That(backToWorld.X, Is.EqualTo(worldPoint.X).Within(0.1f));
        Assert.That(backToWorld.Y, Is.EqualTo(worldPoint.Y).Within(0.1f));
    }

    [Test]
    public void CoordinateConversion_WithZoom_ScalesCorrectly()
    {
        // Arrange
        _cameraService.Position = Vector2.Zero;
        _cameraService.ViewportWidth = 800;
        _cameraService.ViewportHeight = 600;
        _cameraService.Rotation = 0.0f;

        // Test with different zoom levels
        _cameraService.ZoomLevel = 1.0f;
        var world1 = new Vector2(400, 0); // 400m from center
        var screen1 = _cameraService.WorldToScreen(world1);

        _cameraService.ZoomLevel = 2.0f;
        var screen2 = _cameraService.WorldToScreen(world1);

        // Assert - with 2x zoom (more m/px), same world distance should be half as many pixels
        Assert.That(MathF.Abs(screen2.X - 400), Is.LessThan(MathF.Abs(screen1.X - 400)));
    }

    [Test]
    public void ScreenToWorld_CornerPoints_MapCorrectly()
    {
        // Arrange
        _cameraService.Position = Vector2.Zero;
        _cameraService.ViewportWidth = 800;
        _cameraService.ViewportHeight = 600;
        _cameraService.ZoomLevel = 1.0f;
        _cameraService.Rotation = 0.0f;

        // Act - convert all four corners
        var topLeft = _cameraService.ScreenToWorld(new Vector2(0, 0));
        var topRight = _cameraService.ScreenToWorld(new Vector2(800, 0));
        var bottomLeft = _cameraService.ScreenToWorld(new Vector2(0, 600));
        var bottomRight = _cameraService.ScreenToWorld(new Vector2(800, 600));

        // Assert - corners should be at expected positions
        // Top-left should be negative X, positive Y
        Assert.That(topLeft.X, Is.LessThan(0));
        Assert.That(topLeft.Y, Is.GreaterThan(0));

        // Top-right should be positive X, positive Y
        Assert.That(topRight.X, Is.GreaterThan(0));
        Assert.That(topRight.Y, Is.GreaterThan(0));

        // Bottom-left should be negative X, negative Y
        Assert.That(bottomLeft.X, Is.LessThan(0));
        Assert.That(bottomLeft.Y, Is.LessThan(0));

        // Bottom-right should be positive X, negative Y
        Assert.That(bottomRight.X, Is.GreaterThan(0));
        Assert.That(bottomRight.Y, Is.LessThan(0));
    }

    #endregion

    #region Camera Control Tests

    [Test]
    public void Pan_UpdatesPosition()
    {
        // Arrange
        var originalPosition = _cameraService.Position;
        var delta = new Vector2(50, 75);

        // Act
        _cameraService.Pan(delta);

        // Assert
        Assert.That(_cameraService.Position, Is.EqualTo(originalPosition + delta));
    }

    [Test]
    public void Pan_FiresCameraChangedEvent()
    {
        // Arrange
        bool eventFired = false;
        _cameraService.CameraChanged += (s, e) => eventFired = true;

        // Act
        _cameraService.Pan(new Vector2(10, 20));

        // Assert
        Assert.That(eventFired, Is.True);
    }

    [Test]
    public void Zoom_WithoutFocusPoint_ChangesZoomLevel()
    {
        // Arrange
        _cameraService.ZoomLevel = 1.0f;
        float factor = 1.5f;

        // Act
        _cameraService.Zoom(factor);

        // Assert
        Assert.That(_cameraService.ZoomLevel, Is.EqualTo(1.5f).Within(0.001f));
    }

    [Test]
    public void Zoom_WithoutFocusPoint_DoesNotChangePosition()
    {
        // Arrange
        _cameraService.Position = new Vector2(100, 200);
        var originalPosition = _cameraService.Position;

        // Act
        _cameraService.Zoom(1.5f);

        // Assert
        Assert.That(_cameraService.Position, Is.EqualTo(originalPosition));
    }

    [Test]
    public void Zoom_WithFocusPoint_KeepsFocusPointStationary()
    {
        // Arrange
        _cameraService.Position = Vector2.Zero;
        _cameraService.ViewportWidth = 800;
        _cameraService.ViewportHeight = 600;
        _cameraService.ZoomLevel = 1.0f;
        _cameraService.Rotation = 0.0f;

        // Focus on a specific screen point
        var focusScreen = new Vector2(600, 300); // Right of center
        var focusWorldBefore = _cameraService.ScreenToWorld(focusScreen);

        // Act
        _cameraService.Zoom(0.5f, focusScreen); // Zoom in 2x

        // Get world position of same screen point after zoom
        var focusWorldAfter = _cameraService.ScreenToWorld(focusScreen);

        // Assert - focus point should remain at same world position (within tolerance)
        Assert.That(focusWorldAfter.X, Is.EqualTo(focusWorldBefore.X).Within(1.0f));
        Assert.That(focusWorldAfter.Y, Is.EqualTo(focusWorldBefore.Y).Within(1.0f));
    }

    [Test]
    public void Zoom_ClampsToMinMax()
    {
        // Test min clamp
        _cameraService.ZoomLevel = 0.1f;
        _cameraService.Zoom(0.01f); // Try to zoom way in
        Assert.That(_cameraService.ZoomLevel, Is.EqualTo(CameraService.MinZoomLevel));

        // Test max clamp
        _cameraService.ZoomLevel = 50.0f;
        _cameraService.Zoom(10.0f); // Try to zoom way out
        Assert.That(_cameraService.ZoomLevel, Is.EqualTo(CameraService.MaxZoomLevel));
    }

    [Test]
    public void Zoom_FiresCameraChangedEvent()
    {
        // Arrange
        bool eventFired = false;
        _cameraService.CameraChanged += (s, e) => eventFired = true;

        // Act
        _cameraService.Zoom(1.5f);

        // Assert
        Assert.That(eventFired, Is.True);
    }

    [Test]
    public void Rotate_UpdatesRotation()
    {
        // Arrange
        _cameraService.Rotation = 0.0f;
        float delta = MathF.PI / 4;

        // Act
        _cameraService.Rotate(delta);

        // Assert
        Assert.That(_cameraService.Rotation, Is.EqualTo(delta).Within(0.001f));
    }

    [Test]
    public void Rotate_NormalizesAngle()
    {
        // Test wrapping positive
        _cameraService.Rotation = MathF.PI * 1.5f;
        _cameraService.Rotate(MathF.PI); // Should wrap to PI/2
        Assert.That(_cameraService.Rotation, Is.EqualTo(MathF.PI / 2).Within(0.001f));

        // Test wrapping negative
        _cameraService.Rotation = MathF.PI / 2;
        _cameraService.Rotate(-MathF.PI); // Should wrap to 3*PI/2
        Assert.That(_cameraService.Rotation, Is.EqualTo(MathF.PI * 1.5f).Within(0.001f));
    }

    [Test]
    public void Rotate_FiresCameraChangedEvent()
    {
        // Arrange
        bool eventFired = false;
        _cameraService.CameraChanged += (s, e) => eventFired = true;

        // Act
        _cameraService.Rotate(MathF.PI / 4);

        // Assert
        Assert.That(eventFired, Is.True);
    }

    [Test]
    public void FitBounds_CentersOnBounds()
    {
        // Arrange
        var bounds = new BoundingBox(100, 200, 300, 400);

        // Act
        _cameraService.FitBounds(bounds);

        // Assert
        Assert.That(_cameraService.Position, Is.EqualTo(bounds.Center));
    }

    [Test]
    public void FitBounds_CalculatesCorrectZoom()
    {
        // Arrange
        _cameraService.ViewportWidth = 800;
        _cameraService.ViewportHeight = 600;

        // Bounds: 200m wide, 100m tall
        var bounds = new BoundingBox(0, 0, 200, 100);

        // Act
        _cameraService.FitBounds(bounds);

        // Assert - zoom should fit the larger dimension (width) with 10% padding
        // Expected zoom = 200m / 800px * 1.1 = 0.275 m/px
        float expectedZoom = 200f / 800f * 1.1f;
        Assert.That(_cameraService.ZoomLevel, Is.EqualTo(expectedZoom).Within(0.001f));
    }

    [Test]
    public void FitBounds_FiresCameraChangedEvent()
    {
        // Arrange
        bool eventFired = false;
        _cameraService.CameraChanged += (s, e) => eventFired = true;
        var bounds = new BoundingBox(0, 0, 100, 100);

        // Act
        _cameraService.FitBounds(bounds);

        // Assert
        Assert.That(eventFired, Is.True);
    }

    [Test]
    public void CenterOn_UpdatesPosition()
    {
        // Arrange
        var targetPosition = new Vector2(500, 750);

        // Act
        _cameraService.CenterOn(targetPosition);

        // Assert
        Assert.That(_cameraService.Position, Is.EqualTo(targetPosition));
    }

    [Test]
    public void CenterOn_FiresCameraChangedEvent()
    {
        // Arrange
        bool eventFired = false;
        _cameraService.CameraChanged += (s, e) => eventFired = true;

        // Act
        _cameraService.CenterOn(new Vector2(100, 200));

        // Assert
        Assert.That(eventFired, Is.True);
    }

    #endregion

    #region BoundingBox Tests

    [Test]
    public void BoundingBox_CalculatesWidthAndHeight()
    {
        // Arrange
        var bounds = new BoundingBox(10, 20, 110, 70);

        // Assert
        Assert.That(bounds.Width, Is.EqualTo(100));
        Assert.That(bounds.Height, Is.EqualTo(50));
    }

    [Test]
    public void BoundingBox_CalculatesCenter()
    {
        // Arrange
        var bounds = new BoundingBox(0, 0, 100, 200);

        // Assert
        Assert.That(bounds.Center, Is.EqualTo(new Vector2(50, 100)));
    }

    [Test]
    public void BoundingBox_FromCenter_CreatesCorrectBounds()
    {
        // Arrange
        var center = new Vector2(100, 200);
        float width = 50;
        float height = 30;

        // Act
        var bounds = BoundingBox.FromCenter(center, width, height);

        // Assert
        Assert.That(bounds.MinX, Is.EqualTo(75));
        Assert.That(bounds.MaxX, Is.EqualTo(125));
        Assert.That(bounds.MinY, Is.EqualTo(185));
        Assert.That(bounds.MaxY, Is.EqualTo(215));
        Assert.That(bounds.Center, Is.EqualTo(center));
    }

    #endregion

    #region CameraChangedEventArgs Tests

    [Test]
    public void CameraChangedEventArgs_InitializesCorrectly()
    {
        // Arrange
        var position = new Vector2(100, 200);
        float zoomLevel = 1.5f;
        float rotation = MathF.PI / 4;

        // Act
        var eventArgs = new CameraChangedEventArgs(position, zoomLevel, rotation);

        // Assert
        Assert.That(eventArgs.Position, Is.EqualTo(position));
        Assert.That(eventArgs.ZoomLevel, Is.EqualTo(zoomLevel));
        Assert.That(eventArgs.Rotation, Is.EqualTo(rotation));
    }

    #endregion
}
