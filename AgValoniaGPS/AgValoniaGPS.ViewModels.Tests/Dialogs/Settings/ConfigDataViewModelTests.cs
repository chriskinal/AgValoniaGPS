using AgValoniaGPS.ViewModels.Dialogs.Settings;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Settings;

/// <summary>
/// Tests for ConfigDataViewModel (placeholder).
/// </summary>
public class ConfigDataViewModelTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Arrange & Act
        var vm = new ConfigDataViewModel();

        // Assert
        Assert.NotNull(vm.PlaceholderMessage);
        Assert.NotNull(vm.OKCommand);
        Assert.NotNull(vm.CancelCommand);
    }

    [Fact]
    public void PlaceholderMessage_CanBeSet()
    {
        // Arrange
        var vm = new ConfigDataViewModel();
        var newMessage = "New placeholder message";

        // Act
        vm.PlaceholderMessage = newMessage;

        // Assert
        Assert.Equal(newMessage, vm.PlaceholderMessage);
    }
}
