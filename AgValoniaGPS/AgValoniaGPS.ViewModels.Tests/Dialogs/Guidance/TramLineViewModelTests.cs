using AgValoniaGPS.ViewModels.Dialogs.Guidance;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Guidance;

public class TramLineViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        var vm = new TramLineViewModel();
        Assert.Equal(1, vm.TramLineNumber);
        Assert.Equal(0.0, vm.OffsetDistance);
        Assert.True(vm.IsActive);
    }

    [Fact]
    public void TramLineNumber_ClampsToValidRange()
    {
        var vm = new TramLineViewModel();
        vm.TramLineNumber = 0;
        Assert.Equal(1, vm.TramLineNumber);
        vm.TramLineNumber = 100;
        Assert.Equal(99, vm.TramLineNumber);
    }

    [Fact]
    public void OffsetDistance_CanBeSetAndRetrieved()
    {
        var vm = new TramLineViewModel();
        vm.OffsetDistance = 25.5;
        Assert.Equal(25.5, vm.OffsetDistance);
    }

    [Fact]
    public void ToggleActiveCommand_TogglesState()
    {
        var vm = new TramLineViewModel();
        Assert.True(vm.IsActive);
        vm.ToggleActiveCommand.Execute(null);
        Assert.False(vm.IsActive);
    }
}
