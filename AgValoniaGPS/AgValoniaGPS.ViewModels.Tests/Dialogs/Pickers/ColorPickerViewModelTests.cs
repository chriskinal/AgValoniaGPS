using Xunit;
using Avalonia.Media;
using AgValoniaGPS.ViewModels.Dialogs.Pickers;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Pickers;

/// <summary>
/// Unit tests for ColorPickerViewModel.
/// </summary>
public class ColorPickerViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithWhiteColor()
    {
        // Arrange & Act
        var viewModel = new ColorPickerViewModel();

        // Assert
        Assert.Equal(Colors.White, viewModel.SelectedColor);
        Assert.Equal(255, viewModel.Red);
        Assert.Equal(255, viewModel.Green);
        Assert.Equal(255, viewModel.Blue);
        Assert.Equal("#FFFFFF", viewModel.HexValue);
    }

    [Fact]
    public void Constructor_WithInitialColor_InitializesCorrectly()
    {
        // Arrange & Act
        var viewModel = new ColorPickerViewModel(Colors.Red);

        // Assert
        Assert.Equal(Colors.Red, viewModel.SelectedColor);
        Assert.Equal(255, viewModel.Red);
        Assert.Equal(0, viewModel.Green);
        Assert.Equal(0, viewModel.Blue);
        Assert.Equal("#FF0000", viewModel.HexValue);
    }

    [Fact]
    public void SetRed_UpdatesColorAndHex()
    {
        // Arrange
        var viewModel = new ColorPickerViewModel();

        // Act
        viewModel.Red = 128;

        // Assert
        Assert.Equal(128, viewModel.SelectedColor.R);
        Assert.Equal("#80FFFF", viewModel.HexValue);
    }

    [Fact]
    public void SetGreen_UpdatesColorAndHex()
    {
        // Arrange
        var viewModel = new ColorPickerViewModel();

        // Act
        viewModel.Green = 64;

        // Assert
        Assert.Equal(64, viewModel.SelectedColor.G);
        Assert.Equal("#FF40FF", viewModel.HexValue);
    }

    [Fact]
    public void SetBlue_UpdatesColorAndHex()
    {
        // Arrange
        var viewModel = new ColorPickerViewModel();

        // Act
        viewModel.Blue = 32;

        // Assert
        Assert.Equal(32, viewModel.SelectedColor.B);
        Assert.Equal("#FFFF20", viewModel.HexValue);
    }

    [Fact]
    public void SetHexValue_ValidHex_UpdatesColorAndRGB()
    {
        // Arrange
        var viewModel = new ColorPickerViewModel();

        // Act
        viewModel.HexValue = "#00FF00";

        // Assert
        Assert.Equal(0, viewModel.Red);
        Assert.Equal(255, viewModel.Green);
        Assert.Equal(0, viewModel.Blue);
        Assert.Equal(Color.FromRgb(0, 255, 0), viewModel.SelectedColor);
    }

    [Fact]
    public void SetHexValue_InvalidHex_SetsError()
    {
        // Arrange
        var viewModel = new ColorPickerViewModel();

        // Act
        viewModel.HexValue = "GGGGGG";

        // Assert
        Assert.True(viewModel.HasError);
        Assert.Contains("Invalid hex color format", viewModel.ErrorMessage);
    }

    [Fact]
    public void SetSelectedColor_UpdatesRGBAndHex()
    {
        // Arrange
        var viewModel = new ColorPickerViewModel();

        // Act
        viewModel.SelectedColor = Colors.Blue;

        // Assert
        Assert.Equal(0, viewModel.Red);
        Assert.Equal(0, viewModel.Green);
        Assert.Equal(255, viewModel.Blue);
        Assert.Equal("#0000FF", viewModel.HexValue);
    }

    [Fact]
    public void SelectColorCommand_UpdatesSelectedColor()
    {
        // Arrange
        var viewModel = new ColorPickerViewModel();

        // Act
        viewModel.SelectColorCommand.Execute(Colors.Green);

        // Assert
        Assert.Equal(Colors.Green, viewModel.SelectedColor);
    }

    [Fact]
    public void OnOK_ClearsErrorAndReturnsTrue()
    {
        // Arrange
        var viewModel = new ColorPickerViewModel();
        viewModel.HexValue = "invalid"; // Set an error
        viewModel.HexValue = "#FF0000"; // Clear it

        // Act
        viewModel.OKCommand.Execute(null);

        // Assert
        Assert.False(viewModel.HasError);
        Assert.True(viewModel.DialogResult == true);
    }
}
