using System;
using System.Diagnostics;
using AgValoniaGPS.Models.Display;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Services;
using AgValoniaGPS.Services.Display;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.Display;

/// <summary>
/// Integration tests for Wave 7 Display & Visualization services.
/// Tests service registration, DI container resolution, end-to-end workflows,
/// and performance benchmarks for all formatter methods.
/// </summary>
[TestFixture]
public class DisplayIntegrationTests
{
    private ServiceProvider _serviceProvider = null!;
    private IDisplayFormatterService _displayFormatter = null!;
    private IFieldStatisticsService _fieldStatistics = null!;

    [SetUp]
    public void SetUp()
    {
        // Build DI container with Wave 7 services
        var services = new ServiceCollection();

        // Register Wave 7 services
        services.AddSingleton<IDisplayFormatterService, DisplayFormatterService>();
        services.AddSingleton<IFieldStatisticsService, FieldStatisticsService>();

        _serviceProvider = services.BuildServiceProvider();

        // Resolve services from container
        _displayFormatter = _serviceProvider.GetRequiredService<IDisplayFormatterService>();
        _fieldStatistics = _serviceProvider.GetRequiredService<IFieldStatisticsService>();
    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider?.Dispose();
    }

    /// <summary>
    /// Integration Test 1: Service resolution from DI container.
    /// Verifies both DisplayFormatterService and FieldStatisticsService
    /// can be resolved from the DI container and are properly registered as singletons.
    /// </summary>
    [Test]
    public void ServiceResolution_BothServices_ResolvableFromDIContainer()
    {
        // Act: Resolve services multiple times
        var formatter1 = _serviceProvider.GetRequiredService<IDisplayFormatterService>();
        var formatter2 = _serviceProvider.GetRequiredService<IDisplayFormatterService>();
        var stats1 = _serviceProvider.GetRequiredService<IFieldStatisticsService>();
        var stats2 = _serviceProvider.GetRequiredService<IFieldStatisticsService>();

        // Assert: Services should not be null
        Assert.That(formatter1, Is.Not.Null, "DisplayFormatterService should resolve");
        Assert.That(stats1, Is.Not.Null, "FieldStatisticsService should resolve");

        // Assert: Same instance should be returned (singleton)
        Assert.That(formatter2, Is.SameAs(formatter1),
            "DisplayFormatterService should be singleton");
        Assert.That(stats2, Is.SameAs(stats1),
            "FieldStatisticsService should be singleton");
    }

    /// <summary>
    /// Integration Test 2: End-to-end workflow combining FieldStatisticsService and DisplayFormatterService.
    /// Verifies that statistics can be calculated and then formatted for display correctly.
    /// </summary>
    [Test]
    public void EndToEndWorkflow_CalculateAndFormatStatistics_FormatsCorrectly()
    {
        // Arrange: Set up field statistics with sample data
        _fieldStatistics.WorkedAreaSquareMeters = 50000.0; // 5 hectares
        _fieldStatistics.UpdateBoundaryArea(null); // Will use default boundary area

        double currentSpeed = 10.0; // 10 km/h
        double toolWidth = 6.0; // 6 meters

        // Act: Calculate statistics and format results
        var appStats = _fieldStatistics.CalculateApplicationStatistics(currentSpeed, toolWidth);

        string formattedArea = _displayFormatter.FormatArea(appStats.TotalAreaCovered, UnitSystem.Metric);
        string formattedWorkRate = _displayFormatter.FormatApplicationRate(appStats.WorkRate, UnitSystem.Metric);
        string formattedPercentage = _displayFormatter.FormatPercentage(appStats.CoveragePercentage);

        // Assert: Formatted values should be valid strings
        Assert.That(formattedArea, Is.Not.Null.And.Not.Empty,
            "Formatted area should not be null or empty");
        Assert.That(formattedWorkRate, Is.Not.Null.And.Not.Empty,
            "Formatted work rate should not be null or empty");
        Assert.That(formattedPercentage, Is.Not.Null.And.Not.Empty,
            "Formatted percentage should not be null or empty");

        // Assert: Formatted area should be "5.00 ha" (50000 m² = 5 ha)
        Assert.That(formattedArea, Is.EqualTo("5.00 ha"),
            "Area should be formatted correctly");

        // Assert: All formatted values should use InvariantCulture (contain '.' not ',')
        Assert.That(formattedArea, Does.Contain("."),
            "Area should use InvariantCulture decimal separator");
        Assert.That(formattedPercentage, Does.Contain("."),
            "Percentage should use InvariantCulture decimal separator");
    }

    /// <summary>
    /// Integration Test 3: Rotating display data integration.
    /// Verifies GetRotatingDisplayData works correctly with all three screens
    /// and produces properly formatted output.
    /// </summary>
    [Test]
    public void RotatingDisplay_AllThreeScreens_ReturnValidData()
    {
        // Arrange: Set up field statistics
        _fieldStatistics.WorkedAreaSquareMeters = 25000.0;
        double currentSpeed = 8.0; // 8 km/h
        double toolWidth = 5.0; // 5 meters

        // Act & Assert: Test all three screens
        for (int screenNumber = 1; screenNumber <= 3; screenNumber++)
        {
            var displayData = _fieldStatistics.GetRotatingDisplayData(screenNumber, currentSpeed, toolWidth);

            Assert.That(displayData, Is.Not.Null,
                $"Screen {screenNumber} should return valid data");
            Assert.That(displayData.CurrentScreen, Is.EqualTo(screenNumber),
                $"CurrentScreen property should match requested screen");

            // Screen-specific assertions
            switch (screenNumber)
            {
                case 1:
                    Assert.That(displayData.AppStats, Is.Not.Null,
                        "Screen 1 should have AppStats populated");
                    break;
                case 2:
                    Assert.That(displayData.FieldName, Is.Not.Null,
                        "Screen 2 should have FieldName populated");
                    break;
                case 3:
                    Assert.That(displayData.GuidanceLineInfo, Is.Not.Null,
                        "Screen 3 should have GuidanceLineInfo populated");
                    break;
            }
        }
    }

    /// <summary>
    /// Performance Benchmark Test: All formatter methods must complete in less than 1ms.
    /// Tests all 11 formatter methods with 1000 iterations each to verify performance targets.
    /// </summary>
    [Test]
    public void PerformanceBenchmark_AllFormatters_CompleteLessThan1ms()
    {
        // Arrange: Sample data for formatters
        const int iterations = 1000;
        const double maxAllowedMilliseconds = 1.0;

        // Test data
        double speed = 5.56; // m/s
        double heading = 45.5;
        GpsFixType fixType = GpsFixType.RtkFixed;
        double age = 0.5;
        double precision = 0.012;
        double error = 0.018;
        double distance = 1250.0;
        double area = 10000.0;
        double time = 2.25;
        double rate = 5.0;
        double percentage = 75.5;
        GuidanceLineType lineType = GuidanceLineType.ABLine;

        var stopwatch = new Stopwatch();

        // Test 1: FormatSpeed
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            _displayFormatter.FormatSpeed(speed, UnitSystem.Metric);
        }
        stopwatch.Stop();
        double avgSpeedMs = stopwatch.Elapsed.TotalMilliseconds / iterations;
        Assert.That(avgSpeedMs, Is.LessThan(maxAllowedMilliseconds),
            $"FormatSpeed average: {avgSpeedMs:F4}ms (should be < {maxAllowedMilliseconds}ms)");

        // Test 2: FormatHeading
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            _displayFormatter.FormatHeading(heading);
        }
        stopwatch.Stop();
        double avgHeadingMs = stopwatch.Elapsed.TotalMilliseconds / iterations;
        Assert.That(avgHeadingMs, Is.LessThan(maxAllowedMilliseconds),
            $"FormatHeading average: {avgHeadingMs:F4}ms (should be < {maxAllowedMilliseconds}ms)");

        // Test 3: FormatGpsQuality
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            _displayFormatter.FormatGpsQuality(fixType, age);
        }
        stopwatch.Stop();
        double avgGpsQualityMs = stopwatch.Elapsed.TotalMilliseconds / iterations;
        Assert.That(avgGpsQualityMs, Is.LessThan(maxAllowedMilliseconds),
            $"FormatGpsQuality average: {avgGpsQualityMs:F4}ms (should be < {maxAllowedMilliseconds}ms)");

        // Test 4: FormatGpsPrecision
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            _displayFormatter.FormatGpsPrecision(precision);
        }
        stopwatch.Stop();
        double avgPrecisionMs = stopwatch.Elapsed.TotalMilliseconds / iterations;
        Assert.That(avgPrecisionMs, Is.LessThan(maxAllowedMilliseconds),
            $"FormatGpsPrecision average: {avgPrecisionMs:F4}ms (should be < {maxAllowedMilliseconds}ms)");

        // Test 5: FormatCrossTrackError
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            _displayFormatter.FormatCrossTrackError(error, UnitSystem.Metric);
        }
        stopwatch.Stop();
        double avgErrorMs = stopwatch.Elapsed.TotalMilliseconds / iterations;
        Assert.That(avgErrorMs, Is.LessThan(maxAllowedMilliseconds),
            $"FormatCrossTrackError average: {avgErrorMs:F4}ms (should be < {maxAllowedMilliseconds}ms)");

        // Test 6: FormatDistance
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            _displayFormatter.FormatDistance(distance, UnitSystem.Metric);
        }
        stopwatch.Stop();
        double avgDistanceMs = stopwatch.Elapsed.TotalMilliseconds / iterations;
        Assert.That(avgDistanceMs, Is.LessThan(maxAllowedMilliseconds),
            $"FormatDistance average: {avgDistanceMs:F4}ms (should be < {maxAllowedMilliseconds}ms)");

        // Test 7: FormatArea
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            _displayFormatter.FormatArea(area, UnitSystem.Metric);
        }
        stopwatch.Stop();
        double avgAreaMs = stopwatch.Elapsed.TotalMilliseconds / iterations;
        Assert.That(avgAreaMs, Is.LessThan(maxAllowedMilliseconds),
            $"FormatArea average: {avgAreaMs:F4}ms (should be < {maxAllowedMilliseconds}ms)");

        // Test 8: FormatTime
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            _displayFormatter.FormatTime(time);
        }
        stopwatch.Stop();
        double avgTimeMs = stopwatch.Elapsed.TotalMilliseconds / iterations;
        Assert.That(avgTimeMs, Is.LessThan(maxAllowedMilliseconds),
            $"FormatTime average: {avgTimeMs:F4}ms (should be < {maxAllowedMilliseconds}ms)");

        // Test 9: FormatApplicationRate
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            _displayFormatter.FormatApplicationRate(rate, UnitSystem.Metric);
        }
        stopwatch.Stop();
        double avgRateMs = stopwatch.Elapsed.TotalMilliseconds / iterations;
        Assert.That(avgRateMs, Is.LessThan(maxAllowedMilliseconds),
            $"FormatApplicationRate average: {avgRateMs:F4}ms (should be < {maxAllowedMilliseconds}ms)");

        // Test 10: FormatPercentage
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            _displayFormatter.FormatPercentage(percentage);
        }
        stopwatch.Stop();
        double avgPercentageMs = stopwatch.Elapsed.TotalMilliseconds / iterations;
        Assert.That(avgPercentageMs, Is.LessThan(maxAllowedMilliseconds),
            $"FormatPercentage average: {avgPercentageMs:F4}ms (should be < {maxAllowedMilliseconds}ms)");

        // Test 11: FormatGuidanceLine
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            _displayFormatter.FormatGuidanceLine(lineType, heading);
        }
        stopwatch.Stop();
        double avgGuidanceLineMs = stopwatch.Elapsed.TotalMilliseconds / iterations;
        Assert.That(avgGuidanceLineMs, Is.LessThan(maxAllowedMilliseconds),
            $"FormatGuidanceLine average: {avgGuidanceLineMs:F4}ms (should be < {maxAllowedMilliseconds}ms)");

        // Log performance summary
        TestContext.WriteLine("\n=== Performance Benchmark Results ===");
        TestContext.WriteLine($"FormatSpeed:            {avgSpeedMs:F4}ms");
        TestContext.WriteLine($"FormatHeading:          {avgHeadingMs:F4}ms");
        TestContext.WriteLine($"FormatGpsQuality:       {avgGpsQualityMs:F4}ms");
        TestContext.WriteLine($"FormatGpsPrecision:     {avgPrecisionMs:F4}ms");
        TestContext.WriteLine($"FormatCrossTrackError:  {avgErrorMs:F4}ms");
        TestContext.WriteLine($"FormatDistance:         {avgDistanceMs:F4}ms");
        TestContext.WriteLine($"FormatArea:             {avgAreaMs:F4}ms");
        TestContext.WriteLine($"FormatTime:             {avgTimeMs:F4}ms");
        TestContext.WriteLine($"FormatApplicationRate:  {avgRateMs:F4}ms");
        TestContext.WriteLine($"FormatPercentage:       {avgPercentageMs:F4}ms");
        TestContext.WriteLine($"FormatGuidanceLine:     {avgGuidanceLineMs:F4}ms");
        TestContext.WriteLine($"Target: < {maxAllowedMilliseconds}ms per operation");
    }
}
