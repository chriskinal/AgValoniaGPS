using AgValoniaGPS.Models;
using AgValoniaGPS.ViewModels.Dialogs.FieldManagement;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.FieldManagement;

public class BndToolViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDrawMode()
    {
        // Arrange & Act
        var vm = new BndToolViewModel();

        // Assert
        Assert.Equal(BoundaryToolMode.Draw, vm.CurrentMode);
        Assert.True(vm.IsDrawMode);
        Assert.False(vm.IsEraseMode);
        Assert.Equal(1.0, vm.SimplifyTolerance);
    }

    [Fact]
    public void SetDrawMode_UpdatesCurrentMode()
    {
        // Arrange
        var vm = new BndToolViewModel();
        vm.SetEraseModeCommand.Execute(null);

        // Act
        vm.SetDrawModeCommand.Execute(null);

        // Assert
        Assert.Equal(BoundaryToolMode.Draw, vm.CurrentMode);
        Assert.True(vm.IsDrawMode);
    }

    [Fact]
    public void SetSimplifyTolerance_WithinRange_UpdatesValue()
    {
        // Arrange
        var vm = new BndToolViewModel();

        // Act
        vm.SimplifyTolerance = 2.5;

        // Assert
        Assert.Equal(2.5, vm.SimplifyTolerance);
    }

    [Fact]
    public void SetSimplifyTolerance_OutOfRange_DoesNotUpdate()
    {
        // Arrange
        var vm = new BndToolViewModel();
        var original = vm.SimplifyTolerance;

        // Act
        vm.SimplifyTolerance = 15.0; // Above max

        // Assert
        Assert.Equal(original, vm.SimplifyTolerance);
    }

    [Fact]
    public void IncrementPointCount_IncreasesCount()
    {
        // Arrange
        var vm = new BndToolViewModel();
        Assert.Equal(0, vm.DrawingPointCount);

        // Act
        vm.IncrementPointCount();
        vm.IncrementPointCount();

        // Assert
        Assert.Equal(2, vm.DrawingPointCount);
    }

    [Fact]
    public void UndoCommand_DecreasesPointCount()
    {
        // Arrange
        var vm = new BndToolViewModel();
        vm.IncrementPointCount();
        vm.IncrementPointCount();

        // Act
        vm.UndoCommand.Execute(null);

        // Assert
        Assert.Equal(1, vm.DrawingPointCount);
    }

    [Fact]
    public void ClearCommand_ResetsPointCount()
    {
        // Arrange
        var vm = new BndToolViewModel();
        vm.IncrementPointCount();
        vm.IncrementPointCount();

        // Act
        vm.ClearCommand.Execute(null);

        // Assert
        Assert.Equal(0, vm.DrawingPointCount);
    }
}
