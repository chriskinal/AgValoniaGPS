using AgValoniaGPS.ViewModels.Dialogs.Settings;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Settings;

/// <summary>
/// Tests for ConfigHelpViewModel (placeholder).
/// </summary>
public class ConfigHelpViewModelTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Arrange & Act
        var vm = new ConfigHelpViewModel();

        // Assert
        Assert.NotNull(vm.PlaceholderMessage);
        Assert.NotNull(vm.OKCommand);
        Assert.NotNull(vm.CancelCommand);
    }

    [Fact]
    public void PlaceholderMessage_CanBeSet()
    {
        // Arrange
        var vm = new ConfigHelpViewModel();
        var newMessage = "New placeholder message";

        // Act
        vm.PlaceholderMessage = newMessage;

        // Assert
        Assert.Equal(newMessage, vm.PlaceholderMessage);
    }
}
