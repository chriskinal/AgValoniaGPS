using Xunit;
using AgValoniaGPS.Models.Section;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Services.Section;

namespace AgValoniaGPS.Services.Tests.Section;

/// <summary>
/// Tests for AnalogSwitchStateService
/// Focus: Switch state changes, event publication
/// </summary>
public class AnalogSwitchStateServiceTests
{
    [Fact]
    public void SetSwitchState_ChangesWorkSwitch_UpdatesStateAndFiresEvent()
    {
        // Arrange
        var service = new AnalogSwitchStateService();
        SwitchStateChangedEventArgs? eventArgs = null;
        service.SwitchStateChanged += (sender, args) => eventArgs = args;

        // Act
        service.SetSwitchState(AnalogSwitchType.WorkSwitch, SwitchState.Active);

        // Assert
        Assert.Equal(SwitchState.Active, service.GetSwitchState(AnalogSwitchType.WorkSwitch));
        Assert.NotNull(eventArgs);
        Assert.Equal(AnalogSwitchType.WorkSwitch, eventArgs.SwitchType);
        Assert.Equal(SwitchState.Inactive, eventArgs.OldState);
        Assert.Equal(SwitchState.Active, eventArgs.NewState);
    }

    [Fact]
    public void SetSwitchState_SameState_DoesNotFireEvent()
    {
        // Arrange
        var service = new AnalogSwitchStateService();
        service.SetSwitchState(AnalogSwitchType.SteerSwitch, SwitchState.Active);

        int eventCount = 0;
        service.SwitchStateChanged += (sender, args) => eventCount++;

        // Act
        service.SetSwitchState(AnalogSwitchType.SteerSwitch, SwitchState.Active);

        // Assert
        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void GetSwitchState_AllSwitches_StartsInactive()
    {
        // Arrange
        var service = new AnalogSwitchStateService();

        // Act & Assert
        Assert.Equal(SwitchState.Inactive, service.GetSwitchState(AnalogSwitchType.WorkSwitch));
        Assert.Equal(SwitchState.Inactive, service.GetSwitchState(AnalogSwitchType.SteerSwitch));
        Assert.Equal(SwitchState.Inactive, service.GetSwitchState(AnalogSwitchType.LockSwitch));
    }
}
