using AgValoniaGPS.ViewModels.Dialogs.FieldManagement;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.FieldManagement;

public class FieldDirViewModelTests
{
    [Fact]
    public void Constructor_InitializesCollections()
    {
        var vm = new FieldDirViewModel();
        Assert.NotNull(vm.Directories);
        Assert.NotNull(vm.FieldsInDirectory);
    }

    [Fact]
    public void SetSelectedDirectory_UpdatesProperty()
    {
        var vm = new FieldDirViewModel();
        vm.SelectedDirectory = "/test/path";
        Assert.Equal("/test/path", vm.SelectedDirectory);
    }
}
