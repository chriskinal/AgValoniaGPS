using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgValoniaGPS.Models;
using AgValoniaGPS.Services.UI;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace AgValoniaGPS.Desktop.Services;

/// <summary>
/// Implementation of IDialogService for Avalonia UI.
/// Handles dialog display, file/folder pickers, and message boxes.
/// Thread-safe implementation suitable for use across the application.
/// </summary>
public class DialogService : IDialogService
{
    private readonly object _lock = new();

    /// <summary>
    /// Shows a modal dialog with the specified ViewModel.
    /// </summary>
    public async Task<TResult?> ShowDialogAsync<TViewModel, TResult>(TViewModel viewModel) where TViewModel : class
    {
        if (viewModel == null)
        {
            throw new ArgumentNullException(nameof(viewModel));
        }

        var window = GetMainWindow();
        if (window == null)
        {
            throw new InvalidOperationException("Unable to find the main application window.");
        }

        // Create dialog window
        var dialog = new Window
        {
            DataContext = viewModel,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            ShowInTaskbar = false,
            SizeToContent = SizeToContent.WidthAndHeight
        };

        // Try to find and load the view for this ViewModel
        var viewType = FindViewTypeForViewModel(typeof(TViewModel));
        if (viewType != null)
        {
            var view = Activator.CreateInstance(viewType) as Control;
            if (view != null)
            {
                dialog.Content = view;
            }
        }

        // Show the dialog and await result
        var result = await dialog.ShowDialog<TResult>(window);
        return result;
    }

    /// <summary>
    /// Shows a confirmation dialog with Yes/No buttons.
    /// </summary>
    public async Task<bool> ShowConfirmationAsync(string message, string title = "Confirm")
    {
        var window = GetMainWindow();
        if (window == null)
        {
            return false;
        }

        // Create simple confirmation dialog
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            ShowInTaskbar = false
        };

        var panel = new StackPanel { Margin = new Thickness(20) };

        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 20)
        });

        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Spacing = 10
        };

        var yesButton = new Button { Content = "Yes", Width = 80 };
        yesButton.Click += (s, e) => dialog.Close(true);

        var noButton = new Button { Content = "No", Width = 80 };
        noButton.Click += (s, e) => dialog.Close(false);

        buttonPanel.Children.Add(yesButton);
        buttonPanel.Children.Add(noButton);
        panel.Children.Add(buttonPanel);

        dialog.Content = panel;

        var result = await dialog.ShowDialog<bool?>(window);
        return result ?? false;
    }

    /// <summary>
    /// Shows a message dialog with an OK button.
    /// </summary>
    public async Task ShowMessageAsync(string message, string title = "Message", MessageType type = MessageType.Information)
    {
        var window = GetMainWindow();
        if (window == null)
        {
            return;
        }

        // Create simple message dialog
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            ShowInTaskbar = false
        };

        var panel = new StackPanel { Margin = new Thickness(20) };

        // Add icon based on message type
        var iconText = type switch
        {
            MessageType.Error => "❌ ",
            MessageType.Warning => "⚠️ ",
            MessageType.Success => "✅ ",
            _ => "ℹ️ "
        };

        panel.Children.Add(new TextBlock
        {
            Text = iconText + message,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 20)
        });

        var okButton = new Button
        {
            Content = "OK",
            Width = 80,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
        };
        okButton.Click += (s, e) => dialog.Close();

        panel.Children.Add(okButton);
        dialog.Content = panel;

        await dialog.ShowDialog(window);
    }

    /// <summary>
    /// Shows a file picker dialog using Avalonia's Storage API.
    /// </summary>
    public async Task<string?> ShowFilePickerAsync(string title, string? defaultPath = null, string[]? fileTypes = null)
    {
        var window = GetMainWindow();
        if (window?.StorageProvider == null)
        {
            return null;
        }

        var options = new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        };

        // Set file type filters if provided
        if (fileTypes != null && fileTypes.Length > 0)
        {
            var filters = new List<FilePickerFileType>();
            foreach (var fileType in fileTypes)
            {
                filters.Add(new FilePickerFileType(fileType.ToUpperInvariant())
                {
                    Patterns = new[] { $"*.{fileType}" }
                });
            }
            options.FileTypeFilter = filters;
        }

        // Set suggested start location if provided
        if (!string.IsNullOrWhiteSpace(defaultPath))
        {
            try
            {
                var folder = await window.StorageProvider.TryGetFolderFromPathAsync(defaultPath);
                if (folder != null)
                {
                    options.SuggestedStartLocation = folder;
                }
            }
            catch
            {
                // Ignore invalid path
            }
        }

        var result = await window.StorageProvider.OpenFilePickerAsync(options);
        return result?.FirstOrDefault()?.Path.LocalPath;
    }

    /// <summary>
    /// Shows a folder picker dialog using Avalonia's Storage API.
    /// </summary>
    public async Task<string?> ShowFolderPickerAsync(string title, string? defaultPath = null)
    {
        var window = GetMainWindow();
        if (window?.StorageProvider == null)
        {
            return null;
        }

        var options = new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        };

        // Set suggested start location if provided
        if (!string.IsNullOrWhiteSpace(defaultPath))
        {
            try
            {
                var folder = await window.StorageProvider.TryGetFolderFromPathAsync(defaultPath);
                if (folder != null)
                {
                    options.SuggestedStartLocation = folder;
                }
            }
            catch
            {
                // Ignore invalid path
            }
        }

        var result = await window.StorageProvider.OpenFolderPickerAsync(options);
        return result?.FirstOrDefault()?.Path.LocalPath;
    }

    /// <summary>
    /// Gets the main application window.
    /// </summary>
    private static Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }

    /// <summary>
    /// Finds the View type for a given ViewModel type using naming convention.
    /// Convention: MyViewModel → MyView (strips "ViewModel" suffix and looks for matching View)
    /// </summary>
    private static Type? FindViewTypeForViewModel(Type viewModelType)
    {
        var viewModelName = viewModelType.Name;
        var viewName = viewModelName.Replace("ViewModel", "");

        // Search in the Desktop assembly for the view
        var desktopAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name?.Contains("AgValoniaGPS.Desktop") == true);

        if (desktopAssembly == null)
        {
            return null;
        }

        // Look for the view in typical locations
        var possibleViewNames = new[]
        {
            $"AgValoniaGPS.Desktop.Views.Dialogs.{viewName}",
            $"AgValoniaGPS.Desktop.Views.Dialogs.Pickers.{viewName}",
            $"AgValoniaGPS.Desktop.Views.Dialogs.Input.{viewName}",
            $"AgValoniaGPS.Desktop.Views.Dialogs.Utility.{viewName}",
            $"AgValoniaGPS.Desktop.Views.Dialogs.Fields.{viewName}",
            $"AgValoniaGPS.Desktop.Views.Dialogs.Guidance.{viewName}",
            $"AgValoniaGPS.Desktop.Views.Dialogs.Settings.{viewName}",
            $"AgValoniaGPS.Desktop.Views.{viewName}"
        };

        foreach (var possibleName in possibleViewNames)
        {
            var viewType = desktopAssembly.GetType(possibleName);
            if (viewType != null)
            {
                return viewType;
            }
        }

        return null;
    }
}
