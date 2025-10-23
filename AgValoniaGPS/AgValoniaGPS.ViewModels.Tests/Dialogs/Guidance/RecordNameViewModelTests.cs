using System;
using AgValoniaGPS.ViewModels.Dialogs.Guidance;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Guidance;

public class RecordNameViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var viewModel = new RecordNameViewModel();

        // Assert
        Assert.NotNull(viewModel);
        Assert.Equal(string.Empty, viewModel.RecordingName);
        Assert.Equal(string.Empty, viewModel.Description);
        Assert.Equal(0, viewModel.PointCount);
        Assert.Equal(0.0, viewModel.TotalDistance);
        Assert.False(viewModel.IsValid);
    }

    [Fact]
    public void Constructor_InitializesWithProvidedValues()
    {
        // Arrange & Act
        var viewModel = new RecordNameViewModel(pointCount: 100, totalDistance: 523.7);

        // Assert
        Assert.Equal(100, viewModel.PointCount);
        Assert.Equal(523.7, viewModel.TotalDistance);
    }

    [Fact]
    public void RecordingName_ValidName_SetsIsValidTrue()
    {
        // Arrange
        var viewModel = new RecordNameViewModel();

        // Act
        viewModel.RecordingName = "Test Recording 123";

        // Assert
        Assert.True(viewModel.IsValid);
        Assert.False(viewModel.HasError);
    }

    [Fact]
    public void RecordingName_EmptyName_SetsIsValidFalse()
    {
        // Arrange
        var viewModel = new RecordNameViewModel();

        // Act
        viewModel.RecordingName = "";

        // Assert
        Assert.False(viewModel.IsValid);
        Assert.True(viewModel.HasError);
    }

    [Fact]
    public void RecordingName_TooLong_SetsIsValidFalse()
    {
        // Arrange
        var viewModel = new RecordNameViewModel();

        // Act
        viewModel.RecordingName = new string('A', 51); // 51 characters

        // Assert
        Assert.False(viewModel.IsValid);
        Assert.True(viewModel.HasError);
    }

    [Fact]
    public void RecordingName_InvalidCharacters_SetsIsValidFalse()
    {
        // Arrange
        var viewModel = new RecordNameViewModel();

        // Act
        viewModel.RecordingName = "Test@Recording!";

        // Assert
        Assert.False(viewModel.IsValid);
        Assert.True(viewModel.HasError);
    }

    [Fact]
    public void RecordingName_ValidWithHyphenAndUnderscore_SetsIsValidTrue()
    {
        // Arrange
        var viewModel = new RecordNameViewModel();

        // Act
        viewModel.RecordingName = "Test_Recording-123";

        // Assert
        Assert.True(viewModel.IsValid);
        Assert.False(viewModel.HasError);
    }

    [Fact]
    public void RecordedDateFormatted_ReturnsFormattedString()
    {
        // Arrange
        var viewModel = new RecordNameViewModel();
        var testDate = new DateTime(2025, 10, 23, 14, 30, 45);

        // Act
        viewModel.RecordedDate = testDate;

        // Assert
        Assert.Equal("2025-10-23 14:30:45", viewModel.RecordedDateFormatted);
    }

    [Fact]
    public void PointCountDisplay_ReturnsFormattedString()
    {
        // Arrange
        var viewModel = new RecordNameViewModel(pointCount: 42);

        // Act
        var display = viewModel.PointCountDisplay;

        // Assert
        Assert.Equal("42 points", display);
    }

    [Fact]
    public void TotalDistanceFormatted_ReturnsFormattedString()
    {
        // Arrange
        var viewModel = new RecordNameViewModel(totalDistance: 1234.5);

        // Act
        var formatted = viewModel.TotalDistanceFormatted;

        // Assert
        Assert.Equal("1234.5 m", formatted);
    }

    [Fact]
    public void SaveRecordingCommand_CanExecute_WhenValid()
    {
        // Arrange
        var viewModel = new RecordNameViewModel();
        viewModel.RecordingName = "Valid Name";

        // Act
        var canExecute = viewModel.SaveRecordingCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void SaveRecordingCommand_CannotExecute_WhenInvalid()
    {
        // Arrange
        var viewModel = new RecordNameViewModel();
        viewModel.RecordingName = ""; // Invalid

        // Act
        var canExecute = viewModel.SaveRecordingCommand.CanExecute(null);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public void Description_CanBeSetAndRetrieved()
    {
        // Arrange
        var viewModel = new RecordNameViewModel();

        // Act
        viewModel.Description = "Test description for the recording";

        // Assert
        Assert.Equal("Test description for the recording", viewModel.Description);
    }
}
