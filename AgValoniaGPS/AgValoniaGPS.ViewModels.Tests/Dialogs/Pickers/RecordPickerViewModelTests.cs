using System.Collections.ObjectModel;
using Xunit;
using AgValoniaGPS.ViewModels.Dialogs.Pickers;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Pickers;

/// <summary>
/// Unit tests for RecordPickerViewModel.
/// </summary>
public class RecordPickerViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultTitle()
    {
        // Arrange & Act
        var viewModel = new RecordPickerViewModel<string>();

        // Assert
        Assert.Equal("Select Record", viewModel.Title);
        Assert.NotNull(viewModel.Items);
    }

    [Fact]
    public void Constructor_WithTitle_SetsTitle()
    {
        // Arrange & Act
        var viewModel = new RecordPickerViewModel<string>("Select Vehicle");

        // Assert
        Assert.Equal("Select Vehicle", viewModel.Title);
    }

    [Fact]
    public void FilterPredicate_DefaultImplementation_UsesToString()
    {
        // Arrange
        var viewModel = new RecordPickerViewModel<string>();
        viewModel.Items = new ObservableCollection<string> { "Apple", "Banana", "Cherry" };

        // Act
        viewModel.SearchText = "ban";

        // Assert
        Assert.Single(viewModel.FilteredItems);
        Assert.Contains("Banana", viewModel.FilteredItems);
    }

    [Fact]
    public void SetFilterPredicate_CustomPredicate_UsesCustomLogic()
    {
        // Arrange
        var viewModel = new RecordPickerViewModel<TestRecord>();
        viewModel.Items = new ObservableCollection<TestRecord>
        {
            new TestRecord { Id = 1, Name = "Test1" },
            new TestRecord { Id = 2, Name = "Test2" },
            new TestRecord { Id = 3, Name = "Other" }
        };

        viewModel.SetFilterPredicate((item, search) => item.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

        // Act
        viewModel.SearchText = "test";

        // Assert
        Assert.Equal(2, viewModel.FilteredItems.Count);
    }

    [Fact]
    public void OnOK_WithoutSelection_SetsError()
    {
        // Arrange
        var viewModel = new RecordPickerViewModel<string>();
        viewModel.Items = new ObservableCollection<string> { "Item1", "Item2" };

        // Act
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
        var viewModel = new RecordPickerViewModel<string>();
        viewModel.Items = new ObservableCollection<string> { "Item1", "Item2" };
        viewModel.SelectedItem = "Item1";

        // Act
        bool closeRequested = false;
        viewModel.CloseRequested += (s, e) => closeRequested = true;
        viewModel.OKCommand.Execute(null);

        // Assert
        Assert.False(viewModel.HasError);
        Assert.True(closeRequested);
    }

    [Fact]
    public void SearchText_EmptyString_ShowsAllItems()
    {
        // Arrange
        var viewModel = new RecordPickerViewModel<string>();
        viewModel.Items = new ObservableCollection<string> { "Apple", "Banana", "Cherry" };

        // Act
        viewModel.SearchText = "";

        // Assert
        Assert.Equal(3, viewModel.FilteredItems.Count);
    }

    private class TestRecord
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
