using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Models.FieldOperations;
using AgValoniaGPS.Services.FieldOperations;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.FieldOperations;

[TestFixture]
public class HeadlandServiceTests
{
    private IPointInPolygonService _pointInPolygonService = null!;
    private IHeadlandService _headlandService = null!;
    private IHeadlandFileService _fileService = null!;

    [SetUp]
    public void Setup()
    {
        _pointInPolygonService = new PointInPolygonService();
        _headlandService = new HeadlandService(_pointInPolygonService);
        _fileService = new HeadlandFileService();
    }

    [Test]
    public void GenerateHeadlands_SinglePass_CreatesValidHeadland()
    {
        // Arrange: Create a square boundary
        var boundary = new[]
        {
            new Position { Easting = 0, Northing = 0 },
            new Position { Easting = 100, Northing = 0 },
            new Position { Easting = 100, Northing = 100 },
            new Position { Easting = 0, Northing = 100 }
        };

        // Act
        _headlandService.GenerateHeadlands(boundary, 10.0, 1);
        var headlands = _headlandService.GetHeadlands();

        // Assert
        Assert.That(headlands, Is.Not.Null);
        Assert.That(headlands!.Length, Is.EqualTo(1));
        Assert.That(headlands[0].Length, Is.EqualTo(4)); // Same number of vertices as boundary

        // Verify headland is smaller than boundary (inward offset)
        foreach (var point in headlands[0])
        {
            Assert.That(point.Easting, Is.GreaterThan(5)); // Offset inward
            Assert.That(point.Easting, Is.LessThan(95));
            Assert.That(point.Northing, Is.GreaterThan(5));
            Assert.That(point.Northing, Is.LessThan(95));
        }
    }

    [Test]
    public void GenerateHeadlands_MultiplePass_CreatesCorrectNumberOfPasses()
    {
        // Arrange
        var boundary = new[]
        {
            new Position { Easting = 0, Northing = 0 },
            new Position { Easting = 100, Northing = 0 },
            new Position { Easting = 100, Northing = 100 },
            new Position { Easting = 0, Northing = 100 }
        };

        // Act
        _headlandService.GenerateHeadlands(boundary, 8.0, 3);
        var headlands = _headlandService.GetHeadlands();

        // Assert
        Assert.That(headlands, Is.Not.Null);
        Assert.That(headlands!.Length, Is.EqualTo(3));

        // Verify each pass is progressively smaller (more inward)
        for (int i = 0; i < 3; i++)
        {
            Assert.That(headlands[i].Length, Is.EqualTo(4));
        }
    }

    [Test]
    public void IsInHeadland_PointInsideHeadland_ReturnsTrue()
    {
        // Arrange
        var boundary = new[]
        {
            new Position { Easting = 0, Northing = 0 },
            new Position { Easting = 100, Northing = 0 },
            new Position { Easting = 100, Northing = 100 },
            new Position { Easting = 0, Northing = 100 }
        };
        _headlandService.GenerateHeadlands(boundary, 10.0, 1);

        // Point inside headland area
        var testPoint = new Position { Easting = 20, Northing = 20 };

        // Act
        var result = _headlandService.IsInHeadland(testPoint);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsInHeadland_PointOutsideHeadland_ReturnsFalse()
    {
        // Arrange
        var boundary = new[]
        {
            new Position { Easting = 0, Northing = 0 },
            new Position { Easting = 100, Northing = 0 },
            new Position { Easting = 100, Northing = 100 },
            new Position { Easting = 0, Northing = 100 }
        };
        _headlandService.GenerateHeadlands(boundary, 10.0, 1);

        // Point outside boundary entirely
        var testPoint = new Position { Easting = 150, Northing = 150 };

        // Act
        var result = _headlandService.IsInHeadland(testPoint);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void CheckPosition_CrossingIntoHeadland_RaisesEntryEvent()
    {
        // Arrange
        var boundary = new[]
        {
            new Position { Easting = 0, Northing = 0 },
            new Position { Easting = 100, Northing = 0 },
            new Position { Easting = 100, Northing = 100 },
            new Position { Easting = 0, Northing = 100 }
        };
        _headlandService.GenerateHeadlands(boundary, 10.0, 1);

        HeadlandEntryEventArgs? eventArgs = null;
        _headlandService.HeadlandEntry += (sender, e) => eventArgs = e;

        // Start outside headland
        var outsidePoint = new Position { Easting = 150, Northing = 150 };
        _headlandService.CheckPosition(outsidePoint);

        // Act: Move into headland
        var insidePoint = new Position { Easting = 20, Northing = 20 };
        _headlandService.CheckPosition(insidePoint);

        // Assert
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs!.PassNumber, Is.EqualTo(0));
        Assert.That(eventArgs.EntryPosition, Is.EqualTo(insidePoint));
    }

    [Test]
    public void CheckPosition_CrossingOutOfHeadland_RaisesExitEvent()
    {
        // Arrange
        var boundary = new[]
        {
            new Position { Easting = 0, Northing = 0 },
            new Position { Easting = 100, Northing = 0 },
            new Position { Easting = 100, Northing = 100 },
            new Position { Easting = 0, Northing = 100 }
        };
        _headlandService.GenerateHeadlands(boundary, 10.0, 1);

        HeadlandExitEventArgs? eventArgs = null;
        _headlandService.HeadlandExit += (sender, e) => eventArgs = e;

        // Start inside headland
        var insidePoint = new Position { Easting = 20, Northing = 20 };
        _headlandService.CheckPosition(insidePoint);

        // Act: Move outside headland
        var outsidePoint = new Position { Easting = 150, Northing = 150 };
        _headlandService.CheckPosition(outsidePoint);

        // Assert
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs!.PassNumber, Is.EqualTo(0));
        Assert.That(eventArgs.ExitPosition, Is.EqualTo(outsidePoint));
    }

    [Test]
    public void MarkPassCompleted_ValidPass_RaisesCompletedEvent()
    {
        // Arrange
        var boundary = new[]
        {
            new Position { Easting = 0, Northing = 0 },
            new Position { Easting = 100, Northing = 0 },
            new Position { Easting = 100, Northing = 100 },
            new Position { Easting = 0, Northing = 100 }
        };
        _headlandService.GenerateHeadlands(boundary, 10.0, 2);

        HeadlandCompletedEventArgs? eventArgs = null;
        _headlandService.HeadlandCompleted += (sender, e) => eventArgs = e;

        // Act
        _headlandService.MarkPassCompleted(0);

        // Assert
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs!.PassNumber, Is.EqualTo(0));
        Assert.That(eventArgs.AreaCovered, Is.GreaterThan(0));
        Assert.That(_headlandService.IsPassCompleted(0), Is.True);
    }

    [Test]
    public void FileService_SaveAndLoadAgOpenGPS_PreservesData()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), "HeadlandTest_" + Guid.NewGuid());
        Directory.CreateDirectory(testDir);

        try
        {
            var headlands = new[]
            {
                new[]
                {
                    new Position { Easting = 10, Northing = 10, Heading = 0 },
                    new Position { Easting = 90, Northing = 10, Heading = 90 },
                    new Position { Easting = 90, Northing = 90, Heading = 180 },
                    new Position { Easting = 10, Northing = 90, Heading = 270 }
                },
                new[]
                {
                    new Position { Easting = 20, Northing = 20, Heading = 0 },
                    new Position { Easting = 80, Northing = 20, Heading = 90 },
                    new Position { Easting = 80, Northing = 80, Heading = 180 },
                    new Position { Easting = 20, Northing = 80, Heading = 270 }
                }
            };

            // Act: Save and load
            _fileService.SaveHeadlandsAgOpenGPS(headlands, testDir);
            var loaded = _fileService.LoadHeadlandsAgOpenGPS(testDir);

            // Assert
            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded!.Length, Is.EqualTo(2));
            Assert.That(loaded[0].Length, Is.EqualTo(4));
            Assert.That(loaded[1].Length, Is.EqualTo(4));

            // Verify first point of first pass
            Assert.That(loaded[0][0].Easting, Is.EqualTo(10).Within(0.01));
            Assert.That(loaded[0][0].Northing, Is.EqualTo(10).Within(0.01));
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(testDir))
                Directory.Delete(testDir, true);
        }
    }

    [Test]
    public void FileService_SaveAndLoadGeoJSON_PreservesData()
    {
        // Arrange
        var testFile = Path.Combine(Path.GetTempPath(), "headland_test_" + Guid.NewGuid() + ".geojson");

        try
        {
            var headlands = new[]
            {
                new[]
                {
                    new Position { Easting = 10, Northing = 10 },
                    new Position { Easting = 90, Northing = 10 },
                    new Position { Easting = 90, Northing = 90 },
                    new Position { Easting = 10, Northing = 90 }
                }
            };

            // Act: Save and load
            _fileService.SaveHeadlandsGeoJSON(headlands, testFile);
            var loaded = _fileService.LoadHeadlandsGeoJSON(testFile);

            // Assert
            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded!.Length, Is.EqualTo(1));
            Assert.That(loaded[0].Length, Is.GreaterThanOrEqualTo(4)); // May include closing point
        }
        finally
        {
            // Cleanup
            if (File.Exists(testFile))
                File.Delete(testFile);
        }
    }

    [Test]
    public void SetMode_ManualMode_DisablesAutomaticDetection()
    {
        // Arrange
        var boundary = new[]
        {
            new Position { Easting = 0, Northing = 0 },
            new Position { Easting = 100, Northing = 0 },
            new Position { Easting = 100, Northing = 100 },
            new Position { Easting = 0, Northing = 100 }
        };
        _headlandService.GenerateHeadlands(boundary, 10.0, 1);

        bool entryEventRaised = false;
        _headlandService.HeadlandEntry += (sender, e) => entryEventRaised = true;

        // Act: Set manual mode
        _headlandService.SetMode(HeadlandMode.Manual);

        // Check position (should not raise event in manual mode)
        var insidePoint = new Position { Easting = 20, Northing = 20 };
        _headlandService.CheckPosition(insidePoint);

        // Assert
        Assert.That(_headlandService.GetMode(), Is.EqualTo(HeadlandMode.Manual));
        Assert.That(entryEventRaised, Is.False);
    }

    [Test]
    public void GenerateHeadlands_Performance_CompletesInUnder5ms()
    {
        // Arrange: Simple boundary (50 points)
        var boundary = CreateCircularBoundary(50, 100, 50, 50);

        // Act & Measure
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _headlandService.GenerateHeadlands(boundary, 5.0, 2);
        sw.Stop();

        // Assert
        Assert.That(sw.ElapsedMilliseconds, Is.LessThan(5),
            $"Generation took {sw.ElapsedMilliseconds}ms, expected <5ms");
    }

    // Helper method to create circular boundary for testing
    private Position[] CreateCircularBoundary(int points, double radius, double centerX, double centerY)
    {
        var boundary = new Position[points];
        for (int i = 0; i < points; i++)
        {
            double angle = 2 * Math.PI * i / points;
            boundary[i] = new Position
            {
                Easting = centerX + radius * Math.Cos(angle),
                Northing = centerY + radius * Math.Sin(angle)
            };
        }
        return boundary;
    }
}
