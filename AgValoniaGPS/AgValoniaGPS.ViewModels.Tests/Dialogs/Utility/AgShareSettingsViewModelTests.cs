using AgValoniaGPS.ViewModels.Dialogs.Utility;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Utility;

public class AgShareSettingsViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var vm = new AgShareSettingsViewModel();
        Assert.False(vm.EnableAgShare);
        Assert.NotNull(vm.ServerUrl);
        Assert.True(vm.UseSSL);
        Assert.Equal(443, vm.Port);
    }

    [Fact]
    public void ConnectionStatusText_ReflectsState()
    {
        var vm = new AgShareSettingsViewModel();
        Assert.Equal("Not Connected", vm.ConnectionStatusText);
        
        vm.IsConnected = true;
        Assert.Equal("Connected", vm.ConnectionStatusText);
    }

    [Fact]
    public void OnOK_ValidatesRequiredFields()
    {
        var vm = new AgShareSettingsViewModel();
        vm.EnableAgShare = true;
        // ServerUrl is set but Username/Password are empty
        
        vm.OKCommand.Execute(null);
        
        Assert.True(vm.HasError);
    }

    [Fact]
    public void OnOK_AllowsDisabledState()
    {
        var vm = new AgShareSettingsViewModel();
        vm.EnableAgShare = false; // Disabled, no validation needed
        bool? result = null;
        vm.CloseRequested += (s, r) => result = r;
        
        vm.OKCommand.Execute(null);
        
        Assert.False(vm.HasError);
    }
}
