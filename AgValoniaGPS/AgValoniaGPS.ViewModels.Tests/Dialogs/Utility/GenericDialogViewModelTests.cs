using AgValoniaGPS.ViewModels.Dialogs.Utility;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Utility;

public class GenericDialogViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        // Arrange & Act
        var vm = new GenericDialogViewModel();

        // Assert
        Assert.Equal("Dialog", vm.Title);
        Assert.Equal(string.Empty, vm.Message);
        Assert.Equal("OK", vm.OKButtonText);
        Assert.Equal("Cancel", vm.CancelButtonText);
        Assert.True(vm.ShowCancelButton);
        Assert.NotNull(vm.OKCommand);
        Assert.NotNull(vm.CancelCommand);
    }

    [Fact]
    public void Constructor_WithTitleAndMessage_SetsProperties()
    {
        // Arrange & Act
        var vm = new GenericDialogViewModel("Test Title", "Test Message");

        // Assert
        Assert.Equal("Test Title", vm.Title);
        Assert.Equal("Test Message", vm.Message);
    }

    [Fact]
    public void Constructor_WithFullCustomization_SetsAllProperties()
    {
        // Arrange & Act
        var vm = new GenericDialogViewModel("Title", "Message", "Yes", "No", false);

        // Assert
        Assert.Equal("Title", vm.Title);
        Assert.Equal("Message", vm.Message);
        Assert.Equal("Yes", vm.OKButtonText);
        Assert.Equal("No", vm.CancelButtonText);
        Assert.False(vm.ShowCancelButton);
    }

    [Fact]
    public void Properties_CanBeSetAndGet()
    {
        // Arrange
        var vm = new GenericDialogViewModel();

        // Act
        vm.Title = "New Title";
        vm.Message = "New Message";
        vm.OKButtonText = "Accept";
        vm.CancelButtonText = "Decline";
        vm.ShowCancelButton = false;

        // Assert
        Assert.Equal("New Title", vm.Title);
        Assert.Equal("New Message", vm.Message);
        Assert.Equal("Accept", vm.OKButtonText);
        Assert.Equal("Decline", vm.CancelButtonText);
        Assert.False(vm.ShowCancelButton);
    }
}
