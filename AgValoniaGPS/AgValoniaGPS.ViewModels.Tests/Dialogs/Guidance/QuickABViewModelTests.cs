using AgValoniaGPS.Models;
using AgValoniaGPS.ViewModels.Dialogs.Guidance;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Guidance;

public class QuickABViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        var vm = new QuickABViewModel();
        Assert.NotNull(vm.CurrentPosition);
        Assert.True(vm.UseCurrentHeading);
        Assert.Equal(0.0, vm.HeadingOffset);
    }

    [Fact]
    public void AdjustHeadingCommand_UpdatesHeading()
    {
        var vm = new QuickABViewModel();
        vm.CurrentHeading = 45.0;
        vm.AdjustHeadingCommand.Execute(5.0);
        Assert.Equal(5.0, vm.HeadingOffset);
        Assert.Equal(50.0, vm.ABLineHeading);
    }

    [Fact]
    public void UseCurrentHeadingCommand_ResetsOffset()
    {
        var vm = new QuickABViewModel();
        vm.HeadingOffset = 15.0;
        vm.UseCurrentHeadingCommand.Execute(null);
        Assert.Equal(0.0, vm.HeadingOffset);
        Assert.True(vm.UseCurrentHeading);
    }

    [Fact]
    public void CurrentHeading_NormalizesTo360()
    {
        var vm = new QuickABViewModel();
        vm.CurrentHeading = 370.0;
        Assert.Equal(10.0, vm.CurrentHeading);
    }
}
