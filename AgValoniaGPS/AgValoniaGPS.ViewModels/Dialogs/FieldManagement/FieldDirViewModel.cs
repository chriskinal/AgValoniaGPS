using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using AgValoniaGPS.Models;
using AgValoniaGPS.Services;
using AgValoniaGPS.ViewModels.Base;

namespace AgValoniaGPS.ViewModels.Dialogs.FieldManagement;

/// <summary>
/// ViewModel for field directory browser/selector (FormFieldDir)
/// </summary>
public class FieldDirViewModel : DialogViewModelBase
{
    private readonly IFieldService? _fieldService;
    private string _selectedDirectory = string.Empty;
    private bool _isValidDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldDirViewModel"/> class.
    /// </summary>
    /// <param name="fieldService">Optional field service for directory operations.</param>
    public FieldDirViewModel(IFieldService? fieldService = null)
    {
        _fieldService = fieldService;

        Directories = new ObservableCollection<DirectoryInfo>();
        FieldsInDirectory = new ObservableCollection<FieldInfo>();

        BrowseCommand = new RelayCommand(OnBrowse);
        CreateDirectoryCommand = new RelayCommand(OnCreateDirectory);
        RefreshCommand = new RelayCommand(OnRefresh);
    }

    /// <summary>
    /// Gets or sets the currently selected directory path.
    /// </summary>
    public string SelectedDirectory
    {
        get => _selectedDirectory;
        set
        {
            SetProperty(ref _selectedDirectory, value);
            OnRefresh();
        }
    }

    /// <summary>
    /// Gets the subdirectories in the current location.
    /// </summary>
    public ObservableCollection<DirectoryInfo> Directories { get; }

    /// <summary>
    /// Gets the fields found in the selected directory.
    /// </summary>
    public ObservableCollection<FieldInfo> FieldsInDirectory { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the directory contains valid fields.
    /// </summary>
    public bool IsValidDirectory
    {
        get => _isValidDirectory;
        set => SetProperty(ref _isValidDirectory, value);
    }

    /// <summary>
    /// Gets the number of fields in the directory.
    /// </summary>
    public int FieldCount => FieldsInDirectory.Count;

    /// <summary>
    /// Gets the command to open folder picker.
    /// </summary>
    public ICommand BrowseCommand { get; }

    /// <summary>
    /// Gets the command to create a new directory.
    /// </summary>
    public ICommand CreateDirectoryCommand { get; }

    /// <summary>
    /// Gets the command to refresh the directory listing.
    /// </summary>
    public ICommand RefreshCommand { get; }

    /// <summary>
    /// Called when OK is clicked. Validates that a directory is selected.
    /// </summary>
    protected override void OnOK()
    {
        if (string.IsNullOrWhiteSpace(SelectedDirectory))
        {
            SetError("Please select a directory.");
            return;
        }

        if (!Directory.Exists(SelectedDirectory))
        {
            SetError("Selected directory does not exist.");
            return;
        }

        base.OnOK();
    }

    /// <summary>
    /// Opens a folder picker dialog.
    /// </summary>
    private void OnBrowse()
    {
        // In a real implementation, this would open a platform-specific folder picker
        // For now, placeholder - would use IDialogService or similar
    }

    /// <summary>
    /// Creates a new directory in the current location.
    /// </summary>
    private void OnCreateDirectory()
    {
        if (string.IsNullOrWhiteSpace(SelectedDirectory))
        {
            SetError("Please select a parent directory first.");
            return;
        }

        // In a real implementation, would show a dialog to enter new directory name
        // For now, placeholder
    }

    /// <summary>
    /// Refreshes the directory and field listings.
    /// </summary>
    private void OnRefresh()
    {
        ClearError();
        Directories.Clear();
        FieldsInDirectory.Clear();

        if (string.IsNullOrWhiteSpace(SelectedDirectory) || !Directory.Exists(SelectedDirectory))
        {
            IsValidDirectory = false;
            OnPropertyChanged(nameof(FieldCount));
            return;
        }

        try
        {
            // Load subdirectories
            var dirInfo = new DirectoryInfo(SelectedDirectory);
            var subDirs = dirInfo.GetDirectories();

            foreach (var dir in subDirs.OrderBy(d => d.Name))
            {
                Directories.Add(dir);
            }

            // Load fields using service if available
            if (_fieldService != null)
            {
                var fieldNames = _fieldService.GetAvailableFields(SelectedDirectory);

                foreach (var fieldName in fieldNames)
                {
                    var fieldPath = Path.Combine(SelectedDirectory, fieldName);

                    FieldsInDirectory.Add(new FieldInfo
                    {
                        Name = fieldName,
                        Path = fieldPath,
                        DateModified = Directory.GetLastWriteTime(fieldPath)
                    });
                }
            }
            else
            {
                // Fallback: scan for directories that look like field directories
                foreach (var dir in subDirs)
                {
                    FieldsInDirectory.Add(new FieldInfo
                    {
                        Name = dir.Name,
                        Path = dir.FullName,
                        DateModified = dir.LastWriteTime
                    });
                }
            }

            IsValidDirectory = FieldsInDirectory.Count > 0;
            OnPropertyChanged(nameof(FieldCount));
        }
        catch (Exception ex)
        {
            SetError($"Error reading directory: {ex.Message}");
            IsValidDirectory = false;
        }
    }
}
