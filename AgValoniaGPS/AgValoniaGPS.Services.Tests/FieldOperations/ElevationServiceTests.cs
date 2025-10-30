using System;
using System.Collections.Generic;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.FieldOperations;
using AgValoniaGPS.Services.FieldOperations;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.FieldOperations;

[TestFixture]
public class ElevationServiceTests
{
    private ElevationService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new ElevationService();
    }

    [Test]
    public void Constructor_InitializesWithDefaultGridResolution()
    {
        // Assert
        Assert.That(_service.GridResolution, Is.EqualTo(5.0));
    }

    [Test]
    public void SetGridResolution_UpdatesResolution()
    {
        // Act
        _service.SetGridResolution(10.0);

        // Assert
        Assert.That(_service.GridResolution, Is.EqualTo(10.0));
    }

    [Test]
    public void SetGridResolution_ThrowsForZeroOrNegative()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.SetGridResolution(0));
        Assert.Throws<ArgumentException>(() => _service.SetGridResolution(-5));
    }

    [Test]
    public void AddElevationPoint_AddsPointToGrid()
    {
        // Arrange
        var position = new Position2D(10.0, 20.0);

        // Act
        _service.AddElevationPoint(position, 100.0);

        // Assert
        Assert.That(_service.PointCount, Is.EqualTo(1));
        Assert.That(_service.MinElevation, Is.EqualTo(100.0));
        Assert.That(_service.MaxElevation, Is.EqualTo(100.0));
        Assert.That(_service.AverageElevation, Is.EqualTo(100.0));
    }

    [Test]
    public void AddElevationPoint_RaisesEvent()
    {
        // Arrange
        var position = new Position2D(10.0, 20.0);
        ElevationPoint? raisedPoint = null;
        _service.ElevationPointAdded += (sender, point) => raisedPoint = point;

        // Act
        _service.AddElevationPoint(position, 100.0);

        // Assert
        Assert.That(raisedPoint, Is.Not.Null);
        Assert.That(raisedPoint.Value.Position, Is.EqualTo(position));
        Assert.That(raisedPoint.Value.Elevation, Is.EqualTo(100.0));
    }

    [Test]
    public void AddElevationPoint_UpdatesStatistics()
    {
        // Act
        _service.AddElevationPoint(new Position2D(0, 0), 100.0);
        _service.AddElevationPoint(new Position2D(5, 0), 150.0);
        _service.AddElevationPoint(new Position2D(10, 0), 200.0);

        // Assert
        Assert.That(_service.PointCount, Is.EqualTo(3));
        Assert.That(_service.MinElevation, Is.EqualTo(100.0));
        Assert.That(_service.MaxElevation, Is.EqualTo(200.0));
        Assert.That(_service.AverageElevation, Is.EqualTo(150.0).Within(0.001));
    }

    [Test]
    public void GetElevationAt_ReturnsExactValue_WhenOnGridPoint()
    {
        // Arrange
        var position = new Position2D(10.0, 20.0);
        _service.AddElevationPoint(position, 100.0);

        // Act
        var elevation = _service.GetElevationAt(position);

        // Assert
        Assert.That(elevation, Is.Not.Null);
        Assert.That(elevation.Value, Is.EqualTo(100.0));
    }

    [Test]
    public void GetElevationAt_ReturnsNull_WhenNoData()
    {
        // Arrange
        var position = new Position2D(10.0, 20.0);

        // Act
        var elevation = _service.GetElevationAt(position);

        // Assert
        Assert.That(elevation, Is.Null);
    }

    [Test]
    public void PositionToGridCell_ConvertsCorrectly()
    {
        // Arrange
        _service.SetGridResolution(5.0);
        var position = new Position2D(12.0, 23.0);

        // Act
        var cell = _service.PositionToGridCell(position);

        // Assert
        Assert.That(cell.X, Is.EqualTo(2)); // 12.0 / 5.0 = 2.4 -> floor = 2
        Assert.That(cell.Y, Is.EqualTo(4)); // 23.0 / 5.0 = 4.6 -> floor = 4
    }

    [Test]
    public void PositionToGridCell_HandlesNegativeCoordinates()
    {
        // Arrange
        _service.SetGridResolution(5.0);
        var position = new Position2D(-12.0, -23.0);

        // Act
        var cell = _service.PositionToGridCell(position);

        // Assert
        Assert.That(cell.X, Is.EqualTo(-3)); // -12.0 / 5.0 = -2.4 -> floor = -3
        Assert.That(cell.Y, Is.EqualTo(-5)); // -23.0 / 5.0 = -4.6 -> floor = -5
    }

    [Test]
    public void GridCellToPosition_ConvertsCorrectly()
    {
        // Arrange
        _service.SetGridResolution(5.0);
        var cell = new GridCell(2, 4);

        // Act
        var position = _service.GridCellToPosition(cell);

        // Assert
        Assert.That(position.Easting, Is.EqualTo(10.0));
        Assert.That(position.Northing, Is.EqualTo(20.0));
    }

    [Test]
    public void InterpolateBilinear_ReturnsNull_WhenInsufficientData()
    {
        // Arrange
        var position = new Position2D(2.5, 2.5);
        _service.AddElevationPoint(new Position2D(0, 0), 100.0);

        // Act
        var elevation = _service.InterpolateBilinear(position);

        // Assert
        Assert.That(elevation, Is.Null);
    }

    [Test]
    public void InterpolateBilinear_InterpolatesCorrectly_WithFourCorners()
    {
        // Arrange - Create a 2x2 grid with known elevations
        _service.SetGridResolution(5.0);

        // Grid corners: (0,0), (5,0), (0,5), (5,5)
        _service.AddElevationPoint(new Position2D(0, 0), 100.0);   // Bottom-left
        _service.AddElevationPoint(new Position2D(5, 0), 110.0);   // Bottom-right
        _service.AddElevationPoint(new Position2D(0, 5), 120.0);   // Top-left
        _service.AddElevationPoint(new Position2D(5, 5), 130.0);   // Top-right

        // Act - Test point in the middle (2.5, 2.5)
        var elevation = _service.InterpolateBilinear(new Position2D(2.5, 2.5));

        // Assert - Should be average of all four corners
        Assert.That(elevation, Is.Not.Null);
        Assert.That(elevation.Value, Is.EqualTo(115.0).Within(0.001));
    }

    [Test]
    public void InterpolateBilinear_InterpolatesCorrectly_AtEdgePoint()
    {
        // Arrange
        _service.SetGridResolution(5.0);

        _service.AddElevationPoint(new Position2D(0, 0), 100.0);
        _service.AddElevationPoint(new Position2D(5, 0), 110.0);
        _service.AddElevationPoint(new Position2D(0, 5), 120.0);
        _service.AddElevationPoint(new Position2D(5, 5), 130.0);

        // Act - Test point on bottom edge (2.5, 0)
        var elevation = _service.InterpolateBilinear(new Position2D(2.5, 0));

        // Assert - Should be average of bottom two corners
        Assert.That(elevation, Is.Not.Null);
        Assert.That(elevation.Value, Is.EqualTo(105.0).Within(0.001));
    }

    [Test]
    public void InterpolateBilinear_InterpolatesCorrectly_AtCorner()
    {
        // Arrange
        _service.SetGridResolution(5.0);

        _service.AddElevationPoint(new Position2D(0, 0), 100.0);
        _service.AddElevationPoint(new Position2D(5, 0), 110.0);
        _service.AddElevationPoint(new Position2D(0, 5), 120.0);
        _service.AddElevationPoint(new Position2D(5, 5), 130.0);

        // Act - Test point at exact corner (0, 0)
        var elevation = _service.InterpolateBilinear(new Position2D(0, 0));

        // Assert - Should be exact corner value
        Assert.That(elevation, Is.Not.Null);
        Assert.That(elevation.Value, Is.EqualTo(100.0).Within(0.001));
    }

    [Test]
    public void InterpolateBilinear_WorksWithDifferentGridResolutions()
    {
        // Arrange
        _service.SetGridResolution(10.0);

        _service.AddElevationPoint(new Position2D(0, 0), 100.0);
        _service.AddElevationPoint(new Position2D(10, 0), 120.0);
        _service.AddElevationPoint(new Position2D(0, 10), 140.0);
        _service.AddElevationPoint(new Position2D(10, 10), 160.0);

        // Act - Test point at (5, 5) - center of grid
        var elevation = _service.InterpolateBilinear(new Position2D(5, 5));

        // Assert - Should be average of all four corners
        Assert.That(elevation, Is.Not.Null);
        Assert.That(elevation.Value, Is.EqualTo(130.0).Within(0.001));
    }

    [Test]
    public void ClearElevationData_RemovesAllPoints()
    {
        // Arrange
        _service.AddElevationPoint(new Position2D(0, 0), 100.0);
        _service.AddElevationPoint(new Position2D(5, 0), 150.0);

        // Act
        _service.ClearElevationData();

        // Assert
        Assert.That(_service.PointCount, Is.EqualTo(0));
        Assert.That(_service.MinElevation, Is.EqualTo(double.MaxValue));
        Assert.That(_service.MaxElevation, Is.EqualTo(double.MinValue));
    }

    [Test]
    public void ClearElevationData_RaisesEvent()
    {
        // Arrange
        _service.AddElevationPoint(new Position2D(0, 0), 100.0);
        bool eventRaised = false;
        _service.ElevationDataCleared += (sender, args) => eventRaised = true;

        // Act
        _service.ClearElevationData();

        // Assert
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void AddElevationPoint_OverwritesExistingGridCell()
    {
        // Arrange
        var position1 = new Position2D(10.0, 20.0);
        var position2 = new Position2D(11.0, 21.0); // Same grid cell with 5.0 resolution

        // Act
        _service.AddElevationPoint(position1, 100.0);
        _service.AddElevationPoint(position2, 150.0);

        // Assert - Should only have 1 point (overwritten)
        Assert.That(_service.PointCount, Is.EqualTo(1));
        var elevation = _service.GetElevationAt(position1);
        Assert.That(elevation, Is.EqualTo(150.0));
    }

    [Test]
    public void ElevationGrid_UpdatesBoundingBox()
    {
        // Act
        _service.AddElevationPoint(new Position2D(10.0, 20.0), 100.0);
        _service.AddElevationPoint(new Position2D(50.0, 60.0), 150.0);
        _service.AddElevationPoint(new Position2D(-5.0, -10.0), 80.0);

        // Assert
        var grid = _service.ElevationGrid;
        Assert.That(grid.MinEasting, Is.EqualTo(-5.0));
        Assert.That(grid.MaxEasting, Is.EqualTo(50.0));
        Assert.That(grid.MinNorthing, Is.EqualTo(-10.0));
        Assert.That(grid.MaxNorthing, Is.EqualTo(60.0));
    }

    [Test]
    public void InterpolateBilinear_HandlesLargeGrid()
    {
        // Arrange - Create a larger grid
        _service.SetGridResolution(5.0);

        for (int x = 0; x <= 20; x += 5)
        {
            for (int y = 0; y <= 20; y += 5)
            {
                // Create a sloped terrain: elevation increases with x and y
                double elevation = 100.0 + x * 0.5 + y * 0.5;
                _service.AddElevationPoint(new Position2D(x, y), elevation);
            }
        }

        // Act - Test interpolation at various points
        var elev1 = _service.InterpolateBilinear(new Position2D(2.5, 2.5));
        var elev2 = _service.InterpolateBilinear(new Position2D(7.5, 7.5));
        var elev3 = _service.InterpolateBilinear(new Position2D(12.5, 12.5));

        // Assert - Check that interpolation follows the slope
        Assert.That(elev1, Is.Not.Null);
        Assert.That(elev2, Is.Not.Null);
        Assert.That(elev3, Is.Not.Null);
        Assert.That(elev2.Value, Is.GreaterThan(elev1.Value));
        Assert.That(elev3.Value, Is.GreaterThan(elev2.Value));
    }

    [Test]
    public void Service_IsThreadSafe()
    {
        // Arrange
        var tasks = new List<System.Threading.Tasks.Task>();
        var random = new Random(42);

        // Act - Add points from multiple threads
        for (int i = 0; i < 10; i++)
        {
            int index = i;
            tasks.Add(System.Threading.Tasks.Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    var position = new Position2D(
                        index * 10.0 + j * 0.1,
                        index * 10.0 + j * 0.1
                    );
                    _service.AddElevationPoint(position, 100.0 + random.Next(0, 100));
                }
            }));
        }

        System.Threading.Tasks.Task.WaitAll(tasks.ToArray());

        // Assert - Should have all points added without exceptions
        Assert.That(_service.PointCount, Is.GreaterThan(0));
        Assert.That(_service.MinElevation, Is.GreaterThanOrEqualTo(100.0));
        Assert.That(_service.MaxElevation, Is.LessThanOrEqualTo(200.0));
    }
}
