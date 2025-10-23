using System.Collections.Generic;
using AgValoniaGPS.Models;
using AgValoniaGPS.ViewModels.Dialogs.Guidance;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Guidance;

public class SmoothABViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var viewModel = new SmoothABViewModel();

        // Assert
        Assert.NotNull(viewModel);
        Assert.Equal(1.0, viewModel.SmoothingTolerance);
        Assert.Equal(0, viewModel.OriginalPointCount);
        Assert.False(viewModel.HasPreview);
    }

    [Fact]
    public void Constructor_InitializesWithProvidedPoints()
    {
        // Arrange
        var points = new List<Position>
        {
            new Position { Latitude = 42.0, Longitude = -93.6 },
            new Position { Latitude = 42.001, Longitude = -93.601 },
            new Position { Latitude = 42.002, Longitude = -93.602 }
        };

        // Act
        var viewModel = new SmoothABViewModel(points);

        // Assert
        Assert.Equal(3, viewModel.OriginalPointCount);
        Assert.Equal(3, viewModel.RecordedPoints.Count);
    }

    [Fact]
    public void SmoothingTolerance_ClampsToValidRange()
    {
        // Arrange
        var viewModel = new SmoothABViewModel();

        // Act & Assert - Too low
        viewModel.SmoothingTolerance = -1.0;
        Assert.Equal(0.1, viewModel.SmoothingTolerance);

        // Act & Assert - Too high
        viewModel.SmoothingTolerance = 15.0;
        Assert.Equal(10.0, viewModel.SmoothingTolerance);

        // Act & Assert - Valid
        viewModel.SmoothingTolerance = 5.0;
        Assert.Equal(5.0, viewModel.SmoothingTolerance);
    }

    [Fact]
    public void SmoothingTolerance_Change_ClearsPreview()
    {
        // Arrange
        var points = CreateTestPoints();
        var viewModel = new SmoothABViewModel(points);

        // Pre-condition: Generate preview
        viewModel.PreviewSmoothingCommand.Execute(null);
        Assert.True(viewModel.HasPreview);

        // Act
        viewModel.SmoothingTolerance = 2.0;

        // Assert
        Assert.False(viewModel.HasPreview);
    }

    [Fact]
    public void ReductionPercentage_CalculatesCorrectly()
    {
        // Arrange
        var viewModel = new SmoothABViewModel();

        // Act
        viewModel.OriginalPointCount = 100;
        viewModel.SmoothedPointCount = 75;

        // Assert - Should calculate 25% reduction
        Assert.Equal(25.0, viewModel.ReductionPercentage);
    }

    [Fact]
    public void PreviewSmoothingCommand_CanExecute_WithEnoughPoints()
    {
        // Arrange
        var points = CreateTestPoints();
        var viewModel = new SmoothABViewModel(points);

        // Act
        var canExecute = viewModel.PreviewSmoothingCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void PreviewSmoothingCommand_CannotExecute_WithTooFewPoints()
    {
        // Arrange
        var points = new List<Position>
        {
            new Position { Latitude = 42.0, Longitude = -93.6 },
            new Position { Latitude = 42.001, Longitude = -93.601 }
        };
        var viewModel = new SmoothABViewModel(points);

        // Act
        var canExecute = viewModel.PreviewSmoothingCommand.CanExecute(null);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public void ApplySmoothingCommand_CanExecute_WhenPreviewExists()
    {
        // Arrange
        var points = CreateTestPoints();
        var viewModel = new SmoothABViewModel(points);
        viewModel.PreviewSmoothingCommand.Execute(null);

        // Act
        var canExecute = viewModel.ApplySmoothingCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void ResetCommand_RestoresOriginalPoints()
    {
        // Arrange
        var points = CreateTestPoints();
        var viewModel = new SmoothABViewModel(points);
        var originalCount = viewModel.OriginalPointCount;

        // Modify the recorded points
        viewModel.RecordedPoints.Clear();

        // Act
        viewModel.ResetCommand.Execute(null);

        // Assert
        Assert.Equal(originalCount, viewModel.RecordedPoints.Count);
        Assert.False(viewModel.HasPreview);
    }

    [Fact]
    public void FormattedProperties_ReturnCorrectStrings()
    {
        // Arrange
        var viewModel = new SmoothABViewModel();
        viewModel.OriginalPointCount = 100;
        viewModel.SmoothedPointCount = 75;

        // Act & Assert
        Assert.Equal("100 points", viewModel.OriginalPointCountDisplay);
        Assert.Equal("75 points", viewModel.SmoothedPointCountDisplay);
        Assert.Equal("25.0% reduction", viewModel.ReductionPercentageDisplay);
    }

    private List<Position> CreateTestPoints()
    {
        var points = new List<Position>();
        for (int i = 0; i < 10; i++)
        {
            points.Add(new Position { Latitude = 42.0 + i * 0.0001, Longitude = -93.6 + i * 0.0001 });
        }
        return points;
    }
}
