using AgValoniaGPS.ViewModels.Base;
using FluentAssertions;
using Xunit;

namespace AgValoniaGPS.ViewModels.Tests.Base;

/// <summary>
/// Unit tests for ViewModelBase class.
/// Tests property change notifications, error handling, and helper methods.
/// </summary>
public class ViewModelBaseTests
{
    private class TestViewModel : ViewModelBase
    {
        private string _testProperty = string.Empty;

        public string TestProperty
        {
            get => _testProperty;
            set => SetProperty(ref _testProperty, value);
        }

        public void PublicSetError(string message) => SetError(message);
        public void PublicClearError() => ClearError();
    }

    [Fact]
    public void IsBusy_DefaultValue_ShouldBeFalse()
    {
        // Arrange & Act
        var viewModel = new TestViewModel();

        // Assert
        viewModel.IsBusy.Should().BeFalse();
    }

    [Fact]
    public void IsBusy_SetToTrue_ShouldRaisePropertyChanged()
    {
        // Arrange
        var viewModel = new TestViewModel();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(viewModel.IsBusy))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.IsBusy = true;

        // Assert
        viewModel.IsBusy.Should().BeTrue();
        propertyChangedRaised.Should().BeTrue();
    }

    [Fact]
    public void ErrorMessage_DefaultValue_ShouldBeEmpty()
    {
        // Arrange & Act
        var viewModel = new TestViewModel();

        // Assert
        viewModel.ErrorMessage.Should().BeEmpty();
        viewModel.HasError.Should().BeFalse();
    }

    [Fact]
    public void ErrorMessage_SetValue_ShouldRaisePropertyChanged()
    {
        // Arrange
        var viewModel = new TestViewModel();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(viewModel.ErrorMessage))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.ErrorMessage = "Test error";

        // Assert
        viewModel.ErrorMessage.Should().Be("Test error");
        viewModel.HasError.Should().BeTrue();
        propertyChangedRaised.Should().BeTrue();
    }

    [Fact]
    public void SetError_ShouldSetErrorMessageAndRaisePropertyChanged()
    {
        // Arrange
        var viewModel = new TestViewModel();
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(viewModel.ErrorMessage))
                propertyChangedCount++;
        };

        // Act
        viewModel.PublicSetError("Error occurred");

        // Assert
        viewModel.ErrorMessage.Should().Be("Error occurred");
        viewModel.HasError.Should().BeTrue();
        propertyChangedCount.Should().Be(1);
    }

    [Fact]
    public void ClearError_ShouldClearErrorMessage()
    {
        // Arrange
        var viewModel = new TestViewModel();
        viewModel.PublicSetError("Error occurred");

        // Act
        viewModel.PublicClearError();

        // Assert
        viewModel.ErrorMessage.Should().BeEmpty();
        viewModel.HasError.Should().BeFalse();
    }

    [Fact]
    public void SetProperty_WithDifferentValue_ShouldUpdateAndRaisePropertyChanged()
    {
        // Arrange
        var viewModel = new TestViewModel();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(viewModel.TestProperty))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.TestProperty = "New Value";

        // Assert
        viewModel.TestProperty.Should().Be("New Value");
        propertyChangedRaised.Should().BeTrue();
    }

    [Fact]
    public void SetProperty_WithSameValue_ShouldNotRaisePropertyChanged()
    {
        // Arrange
        var viewModel = new TestViewModel();
        viewModel.TestProperty = "Initial Value";
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(viewModel.TestProperty))
                propertyChangedCount++;
        };

        // Act
        viewModel.TestProperty = "Initial Value";

        // Assert
        propertyChangedCount.Should().Be(0);
    }

    [Fact]
    public void HasError_WithNullOrWhitespace_ShouldReturnFalse()
    {
        // Arrange
        var viewModel = new TestViewModel();

        // Act & Assert - null
        viewModel.ErrorMessage = null!;
        viewModel.HasError.Should().BeFalse();

        // Act & Assert - empty
        viewModel.ErrorMessage = string.Empty;
        viewModel.HasError.Should().BeFalse();

        // Act & Assert - whitespace
        viewModel.ErrorMessage = "   ";
        viewModel.HasError.Should().BeFalse();
    }

    [Fact]
    public void HasError_WithNonEmptyMessage_ShouldReturnTrue()
    {
        // Arrange
        var viewModel = new TestViewModel();

        // Act
        viewModel.ErrorMessage = "Error";

        // Assert
        viewModel.HasError.Should().BeTrue();
    }
}
