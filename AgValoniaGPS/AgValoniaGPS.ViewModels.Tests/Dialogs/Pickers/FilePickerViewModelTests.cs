using System;
using System.IO;
using Xunit;
using AgValoniaGPS.ViewModels.Dialogs.Pickers;

namespace AgValoniaGPS.ViewModels.Tests.Dialogs.Pickers;

/// <summary>
/// Unit tests for FilePickerViewModel.
/// </summary>
public class FilePickerViewModelTests
{
    [Fact]
    public void Constructor_InitializesToMyDocuments()
    {
        // Arrange & Act
        var viewModel = new FilePickerViewModel();

        // Assert
        Assert.Equal(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), viewModel.CurrentPath);
        Assert.NotNull(viewModel.Files);
    }

    [Fact]
    public void Constructor_WithFilterExtension_SetsFilter()
    {
        // Arrange & Act
        var viewModel = new FilePickerViewModel(".txt");

        // Assert
        Assert.Equal(".txt", viewModel.FilterExtension);
    }

    [Fact]
    public void NavigateUp_WhenPossible_ChangesPath()
    {
        // Arrange
        var viewModel = new FilePickerViewModel();
        var startPath = viewModel.CurrentPath;

        // Act
        if (viewModel.CanNavigateUp)
        {
            viewModel.NavigateUpCommand.Execute(null);

            // Assert
            Assert.NotEqual(startPath, viewModel.CurrentPath);
        }
    }

    [Fact]
    public void NavigateTo_ValidPath_ChangesCurrentPath()
    {
        // Arrange
        var viewModel = new FilePickerViewModel();
        var testPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        // Act
        viewModel.NavigateToCommand.Execute(testPath);

        // Assert
        Assert.Equal(testPath, viewModel.CurrentPath);
    }

    [Fact]
    public void SelectFile_Directory_NavigatesInto()
    {
        // Arrange
        var viewModel = new FilePickerViewModel();
        var directory = viewModel.Files.FirstOrDefault(f => f.IsDirectory);

        if (directory != null)
        {
            // Act
            viewModel.SelectFileCommand.Execute(directory);

            // Assert
            Assert.Equal(directory.FullPath, viewModel.CurrentPath);
        }
    }

    [Fact]
    public void SelectFile_File_SetsSelectedFile()
    {
        // Arrange
        var viewModel = new FilePickerViewModel();
        var file = viewModel.Files.FirstOrDefault(f => !f.IsDirectory);

        if (file != null)
        {
            // Act
            viewModel.SelectFileCommand.Execute(file);

            // Assert
            Assert.Equal(file, viewModel.SelectedFile);
            Assert.Equal(file.FullPath, viewModel.SelectedFilePath);
        }
    }

    [Fact]
    public void OnOK_WithoutSelection_SetsError()
    {
        // Arrange
        var viewModel = new FilePickerViewModel();

        // Act
        bool closeRequested = false;
        viewModel.CloseRequested += (s, e) => closeRequested = true;
        viewModel.OKCommand.Execute(null);

        // Assert
        Assert.True(viewModel.HasError);
        Assert.False(closeRequested);
    }

    [Fact]
    public void FilterExtension_FiltersFiles()
    {
        // Arrange
        var viewModel = new FilePickerViewModel("*.json");

        // Act
        viewModel.FilterExtension = ".json";

        // Assert
        var nonDirectoryFiles = viewModel.Files.Where(f => !f.IsDirectory);
        foreach (var file in nonDirectoryFiles)
        {
            Assert.EndsWith(".json", file.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
