using AgValoniaGPS.ViewModels.Dialogs.Utility;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Utility;

public class ShiftPosViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithZeroOffsets()
    {
        var vm = new ShiftPosViewModel();
        Assert.Equal(0, vm.OffsetX);
        Assert.Equal(0, vm.OffsetY);
    }

    [Fact]
    public void NewX_CalculatesCorrectly()
    {
        var vm = new ShiftPosViewModel(100, 200);
        vm.OffsetX = 10;
        
        Assert.Equal(110, vm.NewX);
    }

    [Fact]
    public void TotalShift_CalculatesDistance()
    {
        var vm = new ShiftPosViewModel();
        vm.OffsetX = 3;
        vm.OffsetY = 4;
        
        Assert.Equal(5, vm.TotalShift, 3);
    }

    [Fact]
    public void OnOK_ValidatesOffsetRange()
    {
        var vm = new ShiftPosViewModel();
        vm.OffsetX = 2000; // Too large
        
        vm.OKCommand.Execute(null);
        
        Assert.True(vm.HasError);
        Assert.False(vm.DialogResult == true);
    }

    [Fact]
    public void ResetOffsets_ClearsValues()
    {
        var vm = new ShiftPosViewModel { OffsetX = 10, OffsetY = 20 };
        
        vm.ResetOffsets();
        
        Assert.Equal(0, vm.OffsetX);
        Assert.Equal(0, vm.OffsetY);
    }
}
