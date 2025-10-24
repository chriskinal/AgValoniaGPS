using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AgValoniaGPS.Models;
using AgValoniaGPS.Services;
using AgValoniaGPS.ViewModels.Base;

namespace AgValoniaGPS.ViewModels.Dialogs.FieldManagement;

/// <summary>
/// ViewModel for existing field loader with preview (FormFieldExisting)
/// </summary>
public class FieldExistingViewModel : DialogViewModelBase
{
    private readonly IFieldService? _fieldService;
    private FieldInfo? _selectedField;
    private string _fieldPreview = string.Empty;
    private string _fieldsDirectory = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldExistingViewModel"/> class.
    /// </summary>
    /// <param name="fieldService">Optional field service.</param>
    /// <param name="fieldsDirectory">Directory containing fields.</param>
    public FieldExistingViewModel(IFieldService? fieldService = null, string fieldsDirectory = "")
    {
        _fieldService = fieldService;
        _fieldsDirectory = fieldsDirectory;

        Fields = new ObservableCollection<FieldInfo>();

        LoadCommand = new RelayCommand(OnLoad);
        DeleteCommand = new RelayCommand(OnDelete);
        RefreshCommand = new RelayCommand(OnRefresh);

        // Load fields on initialization if directory is provided
        if (!string.IsNullOrEmpty(fieldsDirectory))
        {
            OnRefresh();
        }
    }

    /// <summary>
    /// Gets the collection of available fields.
    /// </summary>
    public ObservableCollection<FieldInfo> Fields { get; }

    /// <summary>
    /// Gets or sets the currently selected field.
    /// </summary>
    public FieldInfo? SelectedField
    {
        get => _selectedField;
        set
        {
            SetProperty(ref _selectedField, value);
            UpdateFieldPreview();
        }
    }

    /// <summary>
    /// Gets or sets the field preview text (summary of field details).
    /// </summary>
    public string FieldPreview
    {
        get => _fieldPreview;
        set => SetProperty(ref _fieldPreview, value);
    }

    /// <summary>
    /// Gets or sets the fields directory.
    /// </summary>
    public string FieldsDirectory
    {
        get => _fieldsDirectory;
        set
        {
            SetProperty(ref _fieldsDirectory, value);
            OnRefresh();
        }
    }

    /// <summary>
    /// Gets the command to load the selected field.
    /// </summary>
    public ICommand LoadCommand { get; }

    /// <summary>
    /// Gets the command to delete the selected field.
    /// </summary>
    public ICommand DeleteCommand { get; }

    /// <summary>
    /// Gets the command to refresh the field list.
    /// </summary>
    public ICommand RefreshCommand { get; }

    /// <summary>
    /// Loads the selected field.
    /// </summary>
    private void OnLoad()
    {
        if (SelectedField == null)
        {
            SetError("Please select a field to load.");
            return;
        }

        // Set DialogResult to true and close
        // The actual field loading would be done by the caller
        DialogResult = true;
        RequestClose(true);
    }

    /// <summary>
    /// Deletes the selected field after confirmation.
    /// </summary>
    private void OnDelete()
    {
        if (SelectedField == null) return;

        try
        {
            // In a real implementation, would show confirmation dialog first
            if (_fieldService != null)
            {
                _fieldService.DeleteField(SelectedField.Path);
            }

            // Remove from list
            Fields.Remove(SelectedField);
            SelectedField = null;
        }
        catch (Exception ex)
        {
            SetError($"Error deleting field: {ex.Message}");
        }
    }

    /// <summary>
    /// Refreshes the field list from the directory.
    /// </summary>
    private void OnRefresh()
    {
        Fields.Clear();
        FieldPreview = string.Empty;
        ClearError();

        if (string.IsNullOrWhiteSpace(FieldsDirectory))
        {
            return;
        }

        try
        {
            if (_fieldService != null)
            {
                var fieldNames = _fieldService.GetAvailableFields(FieldsDirectory);

                foreach (var fieldName in fieldNames)
                {
                    var fieldPath = System.IO.Path.Combine(FieldsDirectory, fieldName);

                    // Try to load field metadata
                    try
                    {
                        var field = _fieldService.LoadField(fieldPath);

                        Fields.Add(new FieldInfo
                        {
                            Name = field.Name,
                            Path = fieldPath,
                            AreaHectares = field.TotalArea,
                            DateModified = field.LastModifiedDate,
                            DateCreated = field.CreatedDate,
                            BoundaryPointCount = field.Boundary?.OuterBoundary?.Points.Count ?? 0,
                            HasBoundary = field.Boundary != null
                        });
                    }
                    catch
                    {
                        // If loading fails, add minimal info
                        Fields.Add(new FieldInfo
                        {
                            Name = fieldName,
                            Path = fieldPath
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            SetError($"Error loading fields: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the field preview text based on the selected field.
    /// </summary>
    private void UpdateFieldPreview()
    {
        if (SelectedField == null)
        {
            FieldPreview = string.Empty;
            return;
        }

        var preview = $"Field: {SelectedField.Name}\n";
        preview += $"Area: {SelectedField.AreaHectares:F2} hectares\n";
        preview += $"Boundary Points: {SelectedField.BoundaryPointCount}\n";
        preview += $"Created: {SelectedField.DateCreated:yyyy-MM-dd}\n";
        preview += $"Last Modified: {SelectedField.DateModified:yyyy-MM-dd}";

        FieldPreview = preview;
    }
}
