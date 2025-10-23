using AgValoniaGPS.ViewModels.Base;
using FluentAssertions;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Base;

/// <summary>
/// Unit tests for DialogViewModelBase class.
/// Tests dialog result handling, OK/Cancel commands, and close event.
/// </summary>
public class DialogViewModelBaseTests
{
    private class TestDialogViewModel : DialogViewModelBase
    {
        public bool CanClose { get; set; } = true;

        protected override bool OnOK()
        {
            if (!CanClose)
            {
                SetError("Cannot close dialog");
                return false;
            }

            return base.OnOK();
        }
    }

    [Fact]
    public void DialogResult_DefaultValue_ShouldBeNull()
    {
        // Arrange & Act
        var viewModel = new TestDialogViewModel();

        // Assert
        viewModel.DialogResult.Should().BeNull();
    }

    [Fact]
    public void OKCommand_ShouldNotBeNull()
    {
        // Arrange & Act
        var viewModel = new TestDialogViewModel();

        // Assert
        viewModel.OKCommand.Should().NotBeNull();
    }

    [Fact]
    public void CancelCommand_ShouldNotBeNull()
    {
        // Arrange & Act
        var viewModel = new TestDialogViewModel();

        // Assert
        viewModel.CancelCommand.Should().NotBeNull();
    }

    [Fact]
    public void OKCommand_Execute_ShouldSetDialogResultToTrue()
    {
        // Arrange
        var viewModel = new TestDialogViewModel();

        // Act
        viewModel.OKCommand.Execute(null);

        // Assert
        viewModel.DialogResult.Should().BeTrue();
    }

    [Fact]
    public void CancelCommand_Execute_ShouldSetDialogResultToFalse()
    {
        // Arrange
        var viewModel = new TestDialogViewModel();

        // Act
        viewModel.CancelCommand.Execute(null);

        // Assert
        viewModel.DialogResult.Should().BeFalse();
    }

    [Fact]
    public void OKCommand_Execute_ShouldRaiseCloseRequestedEvent()
    {
        // Arrange
        var viewModel = new TestDialogViewModel();
        bool? eventResult = null;
        var eventRaised = false;
        viewModel.CloseRequested += (s, result) =>
        {
            eventRaised = true;
            eventResult = result;
        };

        // Act
        viewModel.OKCommand.Execute(null);

        // Assert
        eventRaised.Should().BeTrue();
        eventResult.Should().BeTrue();
    }

    [Fact]
    public void CancelCommand_Execute_ShouldRaiseCloseRequestedEvent()
    {
        // Arrange
        var viewModel = new TestDialogViewModel();
        bool? eventResult = null;
        var eventRaised = false;
        viewModel.CloseRequested += (s, result) =>
        {
            eventRaised = true;
            eventResult = result;
        };

        // Act
        viewModel.CancelCommand.Execute(null);

        // Assert
        eventRaised.Should().BeTrue();
        eventResult.Should().BeFalse();
    }

    [Fact]
    public void OnOK_WhenOverridden_CanPreventDialogClose()
    {
        // Arrange
        var viewModel = new TestDialogViewModel { CanClose = false };
        var closeEventRaised = false;
        viewModel.CloseRequested += (s, result) => closeEventRaised = true;

        // Act
        viewModel.OKCommand.Execute(null);

        // Assert
        viewModel.DialogResult.Should().BeNull(); // Should not be set
        closeEventRaised.Should().BeFalse(); // Event should not be raised
        viewModel.HasError.Should().BeTrue(); // Error should be set
    }

    [Fact]
    public void DialogResult_SetValue_ShouldRaisePropertyChanged()
    {
        // Arrange
        var viewModel = new TestDialogViewModel();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(viewModel.DialogResult))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.DialogResult = true;

        // Assert
        propertyChangedRaised.Should().BeTrue();
    }

    [Fact]
    public void CloseRequested_MultipleSubscribers_ShouldNotifyAll()
    {
        // Arrange
        var viewModel = new TestDialogViewModel();
        var subscriber1Notified = false;
        var subscriber2Notified = false;

        viewModel.CloseRequested += (s, result) => subscriber1Notified = true;
        viewModel.CloseRequested += (s, result) => subscriber2Notified = true;

        // Act
        viewModel.OKCommand.Execute(null);

        // Assert
        subscriber1Notified.Should().BeTrue();
        subscriber2Notified.Should().BeTrue();
    }
}
