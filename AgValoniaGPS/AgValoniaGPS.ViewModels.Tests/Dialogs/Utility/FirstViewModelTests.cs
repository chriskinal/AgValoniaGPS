using AgValoniaGPS.ViewModels.Dialogs.Utility;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Utility;

public class FirstViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var vm = new FirstViewModel();
        Assert.False(vm.AcceptedTerms);
        Assert.False(vm.AcceptedLicense);
        Assert.Equal(0, vm.CurrentPage);
        Assert.True(vm.IsWelcomePage);
    }

    [Fact]
    public void NextPage_AdvancesPage()
    {
        var vm = new FirstViewModel();
        Assert.Equal(0, vm.CurrentPage);
        
        vm.NextPage();
        
        Assert.Equal(1, vm.CurrentPage);
        Assert.True(vm.IsLicensePage);
    }

    [Fact]
    public void PreviousPage_GoesBack()
    {
        var vm = new FirstViewModel { CurrentPage = 1 };
        
        vm.PreviousPage();
        
        Assert.Equal(0, vm.CurrentPage);
    }

    [Fact]
    public void OnOK_RequiresBothAcceptances()
    {
        var vm = new FirstViewModel();
        vm.AcceptedTerms = true;
        // AcceptedLicense still false
        
        vm.OKCommand.Execute(null);
        
        Assert.True(vm.HasError);
    }

    [Fact]
    public void OnOK_SucceedsWhenBothAccepted()
    {
        var vm = new FirstViewModel();
        vm.AcceptedTerms = true;
        vm.AcceptedLicense = true;
        bool? result = null;
        vm.CloseRequested += (s, r) => result = r;
        
        vm.OKCommand.Execute(null);
        
        Assert.False(vm.HasError);
    }
}
