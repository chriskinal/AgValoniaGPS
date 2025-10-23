using AgValoniaGPS.ViewModels.Dialogs.Utility;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Utility;

public class SaveOrNotViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var vm = new SaveOrNotViewModel();
        Assert.Equal("Do you want to save your changes?", vm.Message);
        Assert.Equal("Save Changes", vm.Title);
        Assert.NotNull(vm.SaveCommand);
        Assert.NotNull(vm.DontSaveCommand);
    }

    [Fact]
    public void SaveCommand_SetsDialogResultTrue()
    {
        var vm = new SaveOrNotViewModel();
        bool? result = null;
        vm.CloseRequested += (s, r) => result = r;
        
        vm.SaveCommand.Execute().Subscribe();
        
        Assert.True(result);
        Assert.True(vm.DialogResult);
    }

    [Fact]
    public void DontSaveCommand_SetsDialogResultFalse()
    {
        var vm = new SaveOrNotViewModel();
        bool? result = null;
        vm.CloseRequested += (s, r) => result = r;
        
        vm.DontSaveCommand.Execute().Subscribe();
        
        Assert.False(result);
        Assert.False(vm.DialogResult);
    }
}
