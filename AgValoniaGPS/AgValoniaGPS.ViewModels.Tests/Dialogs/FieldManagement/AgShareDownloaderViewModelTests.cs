using AgValoniaGPS.ViewModels.Dialogs.FieldManagement;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.FieldManagement;

public class AgShareDownloaderViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var vm = new AgShareDownloaderViewModel();
        Assert.NotNull(vm.AvailableFields);
        Assert.False(vm.IsConnected);
        Assert.Equal(0, vm.DownloadProgress);
    }
}
