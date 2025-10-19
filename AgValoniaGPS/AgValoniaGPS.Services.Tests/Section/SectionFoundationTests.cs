using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Events;
using Xunit;
using SectionModel = AgValoniaGPS.Models.Section.Section;
using SectionState = AgValoniaGPS.Models.Section.SectionState;
using SectionConfiguration = AgValoniaGPS.Models.Section.SectionConfiguration;
using CoverageTriangle = AgValoniaGPS.Models.Section.CoverageTriangle;

namespace AgValoniaGPS.Services.Tests.Section;

/// <summary>
/// Tests for foundation models and enums in section control
/// Focused on critical validation and behavior
/// </summary>
public class SectionFoundationTests
{
    #region SectionConfiguration Validation Tests

    [Fact]
    public void SectionConfiguration_RejectsInvalidSectionCount_Below1()
    {
        // Arrange
        var config = new SectionConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => config.SectionCount = 0);
    }

    [Fact]
    public void SectionConfiguration_RejectsInvalidSectionCount_Above31()
    {
        // Arrange
        var config = new SectionConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => config.SectionCount = 32);
    }

    [Fact]
    public void SectionConfiguration_RejectsNegativeWidths()
    {
        // Arrange
        var config = new SectionConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            config.SectionWidths = new double[] { 2.5, -1.0, 2.5 });
    }

    [Fact]
    public void SectionConfiguration_RejectsWidthsBelowMinimum()
    {
        // Arrange
        var config = new SectionConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            config.SectionWidths = new double[] { 2.5, 0.05, 2.5 });
    }

    [Fact]
    public void SectionConfiguration_RejectsWidthsAboveMaximum()
    {
        // Arrange
        var config = new SectionConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            config.SectionWidths = new double[] { 2.5, 25.0, 2.5 });
    }

    [Fact]
    public void SectionConfiguration_AcceptsValidConfiguration()
    {
        // Arrange & Act
        var config = new SectionConfiguration(5, new double[] { 2.5, 2.0, 3.0, 2.5, 2.0 });

        // Assert
        Assert.Equal(5, config.SectionCount);
        Assert.Equal(12.0, config.TotalWidth, 2);
        Assert.True(config.IsValid());
    }

    #endregion

    #region Section State Transition Tests

    [Fact]
    public void Section_TransitionsFromAutoToManualOn()
    {
        // Arrange
        var section = new SectionModel(0, 2.5)
        {
            State = SectionState.Auto
        };

        // Act
        section.State = SectionState.ManualOn;
        section.IsManualOverride = true;

        // Assert
        Assert.Equal(SectionState.ManualOn, section.State);
        Assert.True(section.IsManualOverride);
    }

    [Fact]
    public void Section_TransitionsFromAutoToOff()
    {
        // Arrange
        var section = new SectionModel(0, 2.5)
        {
            State = SectionState.Auto
        };

        // Act
        section.State = SectionState.Off;

        // Assert
        Assert.Equal(SectionState.Off, section.State);
    }

    #endregion

    #region CoverageTriangle Area Calculation Test

    [Fact]
    public void CoverageTriangle_CalculatesAreaCorrectly()
    {
        // Arrange - Create a triangle with known area
        // Using a right triangle with base = 10m, height = 5m
        // Expected area = 0.5 * 10 * 5 = 25 square meters
        var vertex1 = new Position { Easting = 0, Northing = 0, Latitude = 42.0, Longitude = -93.0, Altitude = 100 };
        var vertex2 = new Position { Easting = 10, Northing = 0, Latitude = 42.0, Longitude = -93.0, Altitude = 100 };
        var vertex3 = new Position { Easting = 0, Northing = 5, Latitude = 42.0, Longitude = -93.0, Altitude = 100 };

        var triangle = new CoverageTriangle(vertex1, vertex2, vertex3, 0);

        // Act
        double area = triangle.CalculateArea();

        // Assert
        Assert.Equal(25.0, area, 2);
    }

    #endregion

    #region EventArgs Tests

    [Fact]
    public void SectionStateChangedEventArgs_CreatesWithValidData()
    {
        // Arrange & Act
        var eventArgs = new SectionStateChangedEventArgs(
            0,
            SectionState.Auto,
            SectionState.ManualOn,
            SectionStateChangeType.ToManualOn);

        // Assert
        Assert.Equal(0, eventArgs.SectionId);
        Assert.Equal(SectionState.Auto, eventArgs.OldState);
        Assert.Equal(SectionState.ManualOn, eventArgs.NewState);
        Assert.Equal(SectionStateChangeType.ToManualOn, eventArgs.ChangeType);
        Assert.True((DateTime.UtcNow - eventArgs.Timestamp).TotalSeconds < 1);
    }

    [Fact]
    public void CoverageMapUpdatedEventArgs_CreatesWithValidData()
    {
        // Arrange & Act
        var eventArgs = new CoverageMapUpdatedEventArgs(5, 125.5);

        // Assert
        Assert.Equal(5, eventArgs.AddedTrianglesCount);
        Assert.Equal(125.5, eventArgs.TotalCoveredArea);
        Assert.True((DateTime.UtcNow - eventArgs.Timestamp).TotalSeconds < 1);
    }

    #endregion
}
