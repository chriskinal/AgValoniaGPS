using AgValoniaGPS.Models;
using AgValoniaGPS.ViewModels.Dialogs.Guidance;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Guidance;

public class GridViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var viewModel = new GridViewModel();

        // Assert
        Assert.NotNull(viewModel);
        Assert.Equal(10.0, viewModel.GridSpacing);
        Assert.Equal(0.0, viewModel.GridAngle);
        Assert.True(viewModel.ShowGrid);
        Assert.Equal(5, viewModel.NumberOfLines);
    }

    [Fact]
    public void GridSpacing_ClampsToValidRange()
    {
        // Arrange
        var viewModel = new GridViewModel();

        // Act & Assert - Too low
        viewModel.GridSpacing = 0.5;
        Assert.Equal(1.0, viewModel.GridSpacing);

        // Act & Assert - Too high
        viewModel.GridSpacing = 150.0;
        Assert.Equal(100.0, viewModel.GridSpacing);

        // Act & Assert - Valid
        viewModel.GridSpacing = 25.0;
        Assert.Equal(25.0, viewModel.GridSpacing);
    }

    [Fact]
    public void GridAngle_NormalizesToValidRange()
    {
        // Arrange
        var viewModel = new GridViewModel();

        // Act & Assert - Wraps around
        viewModel.GridAngle = 370.0;
        Assert.Equal(10.0, viewModel.GridAngle);

        // Act & Assert - Negative wraps around
        viewModel.GridAngle = -10.0;
        Assert.Equal(350.0, viewModel.GridAngle);

        // Act & Assert - Valid
        viewModel.GridAngle = 45.0;
        Assert.Equal(45.0, viewModel.GridAngle);
    }

    [Fact]
    public void RotateGridCommand_AdjustsGridAngle()
    {
        // Arrange
        var viewModel = new GridViewModel();
        viewModel.GridAngle = 45.0;

        // Act
        viewModel.RotateGridCommand.Execute(15.0);

        // Assert
        Assert.Equal(60.0, viewModel.GridAngle);
    }

    [Fact]
    public void RotateGridCommand_NegativeRotation()
    {
        // Arrange
        var viewModel = new GridViewModel();
        viewModel.GridAngle = 45.0;

        // Act
        viewModel.RotateGridCommand.Execute(-15.0);

        // Assert
        Assert.Equal(30.0, viewModel.GridAngle);
    }

    [Fact]
    public void NumberOfLines_ClampsToValidRange()
    {
        // Arrange
        var viewModel = new GridViewModel();

        // Act & Assert - Too low
        viewModel.NumberOfLines = 0;
        Assert.Equal(1, viewModel.NumberOfLines);

        // Act & Assert - Too high
        viewModel.NumberOfLines = 25;
        Assert.Equal(20, viewModel.NumberOfLines);

        // Act & Assert - Valid
        viewModel.NumberOfLines = 10;
        Assert.Equal(10, viewModel.NumberOfLines);
    }

    [Fact]
    public void TotalLinesDisplay_CalculatesCorrectly()
    {
        // Arrange
        var viewModel = new GridViewModel();

        // Act
        viewModel.NumberOfLines = 5;

        // Assert - Should be (5*2 + 1) = 11 lines total
        Assert.Equal("11 lines total", viewModel.TotalLinesDisplay);
    }

    [Fact]
    public void GridOrigin_UpdatesHasOrigin()
    {
        // Arrange
        var viewModel = new GridViewModel();

        // Act
        viewModel.GridOrigin = new Position { Latitude = 42.0, Longitude = -93.6 };

        // Assert
        Assert.True(viewModel.HasOrigin);
        Assert.NotNull(viewModel.GridOrigin);
    }

    [Fact]
    public void ApplyGridCommand_CanExecute_WhenOriginSet()
    {
        // Arrange
        var viewModel = new GridViewModel();
        viewModel.GridOrigin = new Position { Latitude = 42.0, Longitude = -93.6 };

        // Act
        var canExecute = viewModel.ApplyGridCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void ApplyGridCommand_CannotExecute_WhenOriginNull()
    {
        // Arrange
        var viewModel = new GridViewModel();
        viewModel.GridOrigin = null;

        // Act
        var canExecute = viewModel.ApplyGridCommand.CanExecute(null);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public void FormattedProperties_ReturnCorrectStrings()
    {
        // Arrange
        var viewModel = new GridViewModel();

        // Act
        viewModel.GridSpacing = 15.5;
        viewModel.GridAngle = 45.5;
        viewModel.GridOrigin = new Position { Latitude = 42.123456, Longitude = -93.654321 };

        // Assert
        Assert.Equal("15.5 m", viewModel.GridSpacingFormatted);
        Assert.Equal("45.5°", viewModel.GridAngleFormatted);
        Assert.Contains("42.123456", viewModel.GridOriginFormatted);
        Assert.Contains("-93.654321", viewModel.GridOriginFormatted);
    }

    [Fact]
    public void ShowGrid_CanBeToggled()
    {
        // Arrange
        var viewModel = new GridViewModel();

        // Act & Assert
        Assert.True(viewModel.ShowGrid);
        viewModel.ShowGrid = false;
        Assert.False(viewModel.ShowGrid);
    }
}
