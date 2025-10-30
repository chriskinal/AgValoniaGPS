using System;
using Xunit;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Services.Rendering;

namespace AgValoniaGPS.Services.Tests.Rendering;

/// <summary>
/// Unit tests for VehicleGeometryService
/// </summary>
public class VehicleGeometryServiceTests
{
    private readonly VehicleGeometryService _service;

    public VehicleGeometryServiceTests()
    {
        _service = new VehicleGeometryService();
    }

    #region GenerateVehicleMesh Tests

    [Fact]
    public void GenerateVehicleMesh_ValidConfiguration_ReturnsCorrectVertexCount()
    {
        // Arrange
        var config = new VehicleConfiguration
        {
            TrackWidth = 2.0,
            Wheelbase = 3.0
        };

        // Act
        var mesh = _service.GenerateVehicleMesh(config, 1.0f, 0.0f, 0.0f);

        // Assert
        // 2 triangles = 6 vertices, 5 floats per vertex (X, Y, R, G, B) = 30 floats
        Assert.Equal(30, mesh.Length);
    }

    [Fact]
    public void GenerateVehicleMesh_ValidConfiguration_HasCorrectDimensions()
    {
        // Arrange
        var config = new VehicleConfiguration
        {
            TrackWidth = 2.0,
            Wheelbase = 4.0
        };

        // Act
        var mesh = _service.GenerateVehicleMesh(config, 1.0f, 1.0f, 1.0f);

        // Assert - check min/max X and Y coordinates
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        for (int i = 0; i < mesh.Length; i += 5)
        {
            float x = mesh[i];
            float y = mesh[i + 1];
            minX = Math.Min(minX, x);
            maxX = Math.Max(maxX, x);
            minY = Math.Min(minY, y);
            maxY = Math.Max(maxY, y);
        }

        float width = maxX - minX;
        float length = maxY - minY;

        Assert.Equal(2.0f, width, 3); // Within 0.001 tolerance
        Assert.Equal(4.0f, length, 3);
    }

    [Fact]
    public void GenerateVehicleMesh_ValidConfiguration_IsCenteredAtOrigin()
    {
        // Arrange
        var config = new VehicleConfiguration
        {
            TrackWidth = 2.0,
            Wheelbase = 4.0
        };

        // Act
        var mesh = _service.GenerateVehicleMesh(config, 1.0f, 1.0f, 1.0f);

        // Assert - calculate center point
        float sumX = 0, sumY = 0;
        int vertexCount = 0;

        for (int i = 0; i < mesh.Length; i += 5)
        {
            sumX += mesh[i];
            sumY += mesh[i + 1];
            vertexCount++;
        }

        float centerX = sumX / vertexCount;
        float centerY = sumY / vertexCount;

        Assert.Equal(0.0f, centerX, 3);
        Assert.Equal(0.0f, centerY, 3);
    }

    [Fact]
    public void GenerateVehicleMesh_ValidConfiguration_AssignsCorrectColors()
    {
        // Arrange
        var config = new VehicleConfiguration
        {
            TrackWidth = 2.0,
            Wheelbase = 3.0
        };
        float r = 0.8f, g = 0.4f, b = 0.2f;

        // Act
        var mesh = _service.GenerateVehicleMesh(config, r, g, b);

        // Assert - all vertices should have the same color
        for (int i = 0; i < mesh.Length; i += 5)
        {
            Assert.Equal(r, mesh[i + 2], 5);
            Assert.Equal(g, mesh[i + 3], 5);
            Assert.Equal(b, mesh[i + 4], 5);
        }
    }

    [Fact]
    public void GenerateVehicleMesh_NullConfiguration_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _service.GenerateVehicleMesh(null!, 1.0f, 0.0f, 0.0f));
    }

    #endregion

    #region GenerateImplementMesh Tests

    [Fact]
    public void GenerateImplementMesh_ValidConfiguration_ReturnsCorrectVertexCount()
    {
        // Arrange
        var toolSettings = new ToolSettings
        {
            ToolWidth = 5.0,
            ToolOffset = 0.0
        };

        // Act
        var mesh = _service.GenerateImplementMesh(toolSettings, 0.0f, 1.0f, 0.0f);

        // Assert
        // 2 triangles = 6 vertices, 5 floats per vertex = 30 floats
        Assert.Equal(30, mesh.Length);
    }

    [Fact]
    public void GenerateImplementMesh_ValidConfiguration_HasCorrectWidth()
    {
        // Arrange
        var toolSettings = new ToolSettings
        {
            ToolWidth = 6.0,
            ToolOffset = 0.0
        };

        // Act
        var mesh = _service.GenerateImplementMesh(toolSettings, 0.0f, 1.0f, 0.0f);

        // Assert - check width
        float minX = float.MaxValue, maxX = float.MinValue;

        for (int i = 0; i < mesh.Length; i += 5)
        {
            float x = mesh[i];
            minX = Math.Min(minX, x);
            maxX = Math.Max(maxX, x);
        }

        float width = maxX - minX;
        Assert.Equal(6.0f, width, 3);
    }

    [Fact]
    public void GenerateImplementMesh_WithOffset_HasCorrectLateralPosition()
    {
        // Arrange
        var toolSettings = new ToolSettings
        {
            ToolWidth = 4.0,
            ToolOffset = 2.0 // 2m offset to the right
        };

        // Act
        var mesh = _service.GenerateImplementMesh(toolSettings, 0.0f, 1.0f, 0.0f);

        // Assert - check center X position
        float sumX = 0;
        int vertexCount = 0;

        for (int i = 0; i < mesh.Length; i += 5)
        {
            sumX += mesh[i];
            vertexCount++;
        }

        float centerX = sumX / vertexCount;
        Assert.Equal(2.0f, centerX, 3); // Should be offset by 2m
    }

    [Fact]
    public void GenerateImplementMesh_ValidConfiguration_AssignsCorrectColors()
    {
        // Arrange
        var toolSettings = new ToolSettings
        {
            ToolWidth = 5.0,
            ToolOffset = 0.0
        };
        float r = 0.3f, g = 0.7f, b = 0.9f;

        // Act
        var mesh = _service.GenerateImplementMesh(toolSettings, r, g, b);

        // Assert
        for (int i = 0; i < mesh.Length; i += 5)
        {
            Assert.Equal(r, mesh[i + 2], 5);
            Assert.Equal(g, mesh[i + 3], 5);
            Assert.Equal(b, mesh[i + 4], 5);
        }
    }

    [Fact]
    public void GenerateImplementMesh_NullConfiguration_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _service.GenerateImplementMesh(null!, 0.0f, 1.0f, 0.0f));
    }

    #endregion

    #region GenerateHeadingArrow Tests

    [Fact]
    public void GenerateHeadingArrow_ValidLength_ReturnsCorrectVertexCount()
    {
        // Arrange
        float length = 3.0f;

        // Act
        var mesh = _service.GenerateHeadingArrow(length, 1.0f, 1.0f, 0.0f);

        // Assert
        // 1 triangle = 3 vertices, 5 floats per vertex = 15 floats
        Assert.Equal(15, mesh.Length);
    }

    [Fact]
    public void GenerateHeadingArrow_ValidLength_PointsForward()
    {
        // Arrange
        float length = 4.0f;

        // Act
        var mesh = _service.GenerateHeadingArrow(length, 1.0f, 1.0f, 0.0f);

        // Assert - tip should be at (0, length)
        // First vertex is the tip
        Assert.Equal(0.0f, mesh[0], 5); // X = 0
        Assert.Equal(length, mesh[1], 5); // Y = length

        // Base vertices should be at Y = 0
        Assert.Equal(0.0f, mesh[6], 5);  // Base left Y
        Assert.Equal(0.0f, mesh[11], 5); // Base right Y
    }

    [Fact]
    public void GenerateHeadingArrow_ValidLength_HasCorrectBaseWidth()
    {
        // Arrange
        float length = 2.5f;
        float expectedBaseWidth = 0.5f;

        // Act
        var mesh = _service.GenerateHeadingArrow(length, 1.0f, 1.0f, 0.0f);

        // Assert - base vertices X coordinates
        float leftX = mesh[5];   // Base left X
        float rightX = mesh[10]; // Base right X
        float baseWidth = rightX - leftX;

        Assert.Equal(expectedBaseWidth, baseWidth, 5);
    }

    [Fact]
    public void GenerateHeadingArrow_ValidLength_AssignsCorrectColors()
    {
        // Arrange
        float length = 3.0f;
        float r = 1.0f, g = 0.0f, b = 0.0f;

        // Act
        var mesh = _service.GenerateHeadingArrow(length, r, g, b);

        // Assert
        for (int i = 0; i < mesh.Length; i += 5)
        {
            Assert.Equal(r, mesh[i + 2], 5);
            Assert.Equal(g, mesh[i + 3], 5);
            Assert.Equal(b, mesh[i + 4], 5);
        }
    }

    [Fact]
    public void GenerateHeadingArrow_ZeroLength_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _service.GenerateHeadingArrow(0.0f, 1.0f, 1.0f, 1.0f));
    }

    [Fact]
    public void GenerateHeadingArrow_NegativeLength_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _service.GenerateHeadingArrow(-1.0f, 1.0f, 1.0f, 1.0f));
    }

    #endregion
}
