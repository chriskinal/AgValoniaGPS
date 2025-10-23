using AgValoniaGPS.ViewModels.Dialogs.Utility;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Utility;

public class WebCamViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var vm = new WebCamViewModel();
        Assert.False(vm.IsStreaming);
        Assert.NotNull(vm.StartCommand);
        Assert.NotNull(vm.StopCommand);
        Assert.Contains("not implemented", vm.PlaceholderMessage);
    }

    [Fact]
    public void StatusText_ReflectsStreamingState()
    {
        var vm = new WebCamViewModel();
        Assert.Contains("stopped", vm.StatusText);
        
        vm.IsStreaming = true;
        Assert.Contains("streaming", vm.StatusText);
    }
}
