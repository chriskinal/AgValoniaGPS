using AgValoniaGPS.ViewModels.Dialogs.Utility;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Utility;

public class AboutViewModelTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        var vm = new AboutViewModel();
        
        Assert.Equal("AgValoniaGPS", vm.ApplicationName);
        Assert.NotNull(vm.Version);
        Assert.NotNull(vm.Copyright);
        Assert.NotNull(vm.Description);
        Assert.NotNull(vm.License);
        Assert.NotNull(vm.Website);
        Assert.NotNull(vm.OpenWebsiteCommand);
    }

    [Fact]
    public void FullVersionString_IncludesVersion()
    {
        var vm = new AboutViewModel();
        Assert.Contains("Version", vm.FullVersionString);
    }
}
