using AgValoniaGPS.ViewModels.Dialogs.Guidance;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Guidance;

public class HeadAcheViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        var vm = new HeadAcheViewModel();
        Assert.Equal(HeadlandMode.Auto, vm.CurrentMode);
        Assert.Equal(1, vm.CurrentPass);
        Assert.False(vm.IsInHeadland);
    }

    [Fact]
    public void SetModeCommand_ChangesMode()
    {
        var vm = new HeadAcheViewModel();
        vm.SetModeCommand.Execute(HeadlandMode.Manual);
        Assert.Equal(HeadlandMode.Manual, vm.CurrentMode);
        Assert.True(vm.IsManualMode);
    }

    [Fact]
    public void NextPassCommand_IncrementsPass()
    {
        var vm = new HeadAcheViewModel(maxPasses: 4);
        vm.NextPassCommand.Execute(null);
        Assert.Equal(2, vm.CurrentPass);
    }

    [Fact]
    public void UpdateHeadlandStatus_UpdatesProperties()
    {
        var vm = new HeadAcheViewModel();
        vm.UpdateHeadlandStatus(true, 5.5);
        Assert.True(vm.IsInHeadland);
        Assert.Equal(5.5, vm.DistanceToHeadland);
    }
}
