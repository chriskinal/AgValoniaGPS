using Xunit;
using AgValoniaGPS.ViewModels.Dialogs.Pickers;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Pickers;

/// <summary>
/// Unit tests for DrivePickerViewModel.
/// </summary>
public class DrivePickerViewModelTests
{
    [Fact]
    public void Constructor_LoadsDrives()
    {
        // Arrange & Act
        var viewModel = new DrivePickerViewModel();

        // Assert
        Assert.NotNull(viewModel.Items);
        // Note: The actual count depends on the system drives available
    }

    [Fact]
    public void RefreshDrivesCommand_ReloadsDrives()
    {
        // Arrange
        var viewModel = new DrivePickerViewModel();
        var initialCount = viewModel.Items.Count;

        // Act
        viewModel.RefreshDrivesCommand.Execute(null);

        // Assert
        Assert.Equal(initialCount, viewModel.Items.Count);
    }

    [Fact]
    public void FilterPredicate_MatchesDriveName()
    {
        // Arrange
        var viewModel = new DrivePickerViewModel();
        if (viewModel.Items.Count == 0) return; // Skip if no drives

        var testDrive = viewModel.Items[0];

        // Act
        viewModel.SearchText = testDrive.Name.Substring(0, 1);

        // Assert
        Assert.Contains(viewModel.FilteredItems, item => item.Name.Contains(viewModel.SearchText, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OnOK_WithoutSelection_SetsError()
    {
        // Arrange
        var viewModel = new DrivePickerViewModel();

        // Act - Try to OK without selecting
        bool closeRequested = false;
        viewModel.CloseRequested += (s, e) => closeRequested = true;
        viewModel.OKCommand.Execute(null);

        // Assert
        Assert.True(viewModel.HasError);
        Assert.False(closeRequested);
    }

    [Fact]
    public void OnOK_WithSelection_Succeeds()
    {
        // Arrange
        var viewModel = new DrivePickerViewModel();
        if (viewModel.Items.Count == 0) return; // Skip if no drives

        viewModel.SelectedItem = viewModel.Items[0];

        // Act
        bool closeRequested = false;
        viewModel.CloseRequested += (s, e) => closeRequested = true;
        viewModel.OKCommand.Execute(null);

        // Assert
        Assert.False(viewModel.HasError);
        Assert.True(closeRequested);
    }
}
