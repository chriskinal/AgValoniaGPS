using AgValoniaGPS.ViewModels.Dialogs.Guidance;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Guidance;

public class TramViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        var vm = new TramViewModel();
        Assert.Equal(TramLineMode.All, vm.TramMode);
        Assert.Equal(1.0, vm.TramSpacing);
        Assert.True(vm.ShowTramLines);
        Assert.NotEmpty(vm.Patterns);
    }

    [Fact]
    public void TramSpacing_ClampsToValidRange()
    {
        var vm = new TramViewModel();
        vm.TramSpacing = 0.5;
        Assert.Equal(1.0, vm.TramSpacing);
        vm.TramSpacing = 15.0;
        Assert.Equal(12.0, vm.TramSpacing);
    }

    [Fact]
    public void ActualSpacing_CalculatesCorrectly()
    {
        var vm = new TramViewModel(implementWidth: 10.0);
        vm.TramSpacing = 3.0;
        Assert.Equal(30.0, vm.ActualSpacing);
    }

    [Fact]
    public void SelectedPattern_UpdatesTramMode()
    {
        var vm = new TramViewModel();
        var pattern = vm.Patterns[2]; // ABOnly pattern
        vm.SelectedPattern = pattern;
        Assert.Equal(TramLineMode.ABOnly, vm.TramMode);
    }
}
