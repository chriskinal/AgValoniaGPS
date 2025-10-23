using AgValoniaGPS.ViewModels.Dialogs.Utility;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Utility;

public class SavingViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var vm = new SavingViewModel();
        Assert.Equal("Saving...", vm.Message);
        Assert.Equal(0, vm.Progress);
        Assert.True(vm.IsIndeterminate);
    }

    [Fact]
    public void UpdateProgress_SetsProgressAndMessage()
    {
        var vm = new SavingViewModel();
        
        vm.UpdateProgress(50, "Halfway done");
        
        Assert.Equal(50, vm.Progress);
        Assert.Equal("Halfway done", vm.Message);
        Assert.False(vm.IsIndeterminate);
    }

    [Fact]
    public void Progress_SetsIsIndeterminateFalse()
    {
        var vm = new SavingViewModel();
        Assert.True(vm.IsIndeterminate);
        
        vm.Progress = 75;
        
        Assert.False(vm.IsIndeterminate);
    }
}
