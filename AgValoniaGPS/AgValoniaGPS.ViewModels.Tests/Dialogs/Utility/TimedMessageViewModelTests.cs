using AgValoniaGPS.ViewModels.Dialogs.Utility;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Utility;

public class TimedMessageViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithMessageAndDuration()
    {
        var vm = new TimedMessageViewModel("Test", 10);
        Assert.Equal("Test", vm.Message);
        Assert.Equal(10, vm.TotalSeconds);
        Assert.Equal(10, vm.SecondsRemaining);
    }

    [Fact]
    public void CountdownText_FormatsCorrectly()
    {
        var vm = new TimedMessageViewModel("Test", 5);
        Assert.Contains("5 seconds", vm.CountdownText);
        
        vm.SecondsRemaining = 1;
        Assert.Contains("1 second", vm.CountdownText);
    }

    [Fact]
    public void CloseNowCommand_StopsTimerAndCloses()
    {
        var vm = new TimedMessageViewModel("Test", 10);
        bool? result = null;
        vm.CloseRequested += (s, r) => result = r;
        
        vm.CloseNowCommand.Execute().Subscribe();
        
        Assert.False(result);
    }
}
