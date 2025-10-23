using AgValoniaGPS.ViewModels.Dialogs.Utility;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Utility;

public class PanViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var vm = new PanViewModel();
        Assert.Equal(0, vm.PanOffsetX);
        Assert.Equal(0, vm.PanOffsetY);
        Assert.Equal(100.0, vm.PanStep);
        Assert.NotNull(vm.PanUpCommand);
        Assert.NotNull(vm.PanDownCommand);
        Assert.NotNull(vm.CenterCommand);
    }

    [Fact]
    public void PanUpCommand_DecreasesOffsetY()
    {
        var vm = new PanViewModel();
        vm.PanUpCommand.Execute().Subscribe();
        Assert.Equal(-100, vm.PanOffsetY);
    }

    [Fact]
    public void PanRightCommand_IncreasesOffsetX()
    {
        var vm = new PanViewModel();
        vm.PanRightCommand.Execute().Subscribe();
        Assert.Equal(100, vm.PanOffsetX);
    }

    [Fact]
    public void CenterCommand_ResetsOffsets()
    {
        var vm = new PanViewModel { PanOffsetX = 200, PanOffsetY = 150 };
        
        vm.CenterCommand.Execute().Subscribe();
        
        Assert.Equal(0, vm.PanOffsetX);
        Assert.Equal(0, vm.PanOffsetY);
    }

    [Fact]
    public void ResetCommand_ResetsEverything()
    {
        var vm = new PanViewModel { PanOffsetX = 200, PanOffsetY = 150, PanStep = 50 };
        
        vm.ResetCommand.Execute().Subscribe();
        
        Assert.Equal(0, vm.PanOffsetX);
        Assert.Equal(0, vm.PanOffsetY);
        Assert.Equal(100, vm.PanStep);
    }
}
