using CommunityToolkit.Mvvm.Input;
using AgValoniaGPS.Models;
using AgValoniaGPS.Services;
using AgValoniaGPS.Services.FieldOperations;
using AgValoniaGPS.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace AgValoniaGPS.ViewModels.Panels.FieldManagement;

/// <summary>
/// ViewModel for the Field File Manager panel providing field file operations.
/// Manages field creation, loading, saving, and import/export of boundary files.
/// </summary>
public partial class FormFieldFileManagerViewModel : PanelViewModelBase
{
    private readonly IFieldService _fieldService;
    private readonly IBoundaryFileService _boundaryFileService;

    private string _currentFieldName = "No Field";
    private FieldInfo? _selectedField;
    private DateTime? _lastSaveTime;

    public FormFieldFileManagerViewModel(
        IFieldService fieldService,
        IBoundaryFileService boundaryFileService)
    {
        _fieldService = fieldService ?? throw new ArgumentNullException(nameof(fieldService));
        _boundaryFileService = boundaryFileService ?? throw new ArgumentNullException(nameof(boundaryFileService));

        Title = "Field Files";

        AvailableFields = new ObservableCollection<FieldInfo>();

        // Commands
        NewFieldCommand = new RelayCommand(OnNewField);
        OpenFieldCommand = new RelayCommand(OnOpenField);
        SaveFieldCommand = new RelayCommand(OnSaveField);
        SaveAsFieldCommand = new RelayCommand(OnSaveAsField);
        DeleteFieldCommand = new RelayCommand(OnDeleteField);
        ImportBoundaryCommand = new RelayCommand(OnImportBoundary);
        ExportBoundaryCommand = new RelayCommand(OnExportBoundary);

        // Subscribe to field service events
        _fieldService.ActiveFieldChanged += OnActiveFieldChanged;

        // Load available fields
        LoadAvailableFields();

        // Update current field name
        UpdateCurrentField();
    }

    public string Title { get; } = "Field Files";

    /// <summary>
    /// Current field name
    /// </summary>
    public string CurrentFieldName
    {
        get => _currentFieldName;
        set => SetProperty(ref _currentFieldName, value);
    }

    /// <summary>
    /// Collection of available fields
    /// </summary>
    public ObservableCollection<FieldInfo> AvailableFields { get; }

    /// <summary>
    /// Currently selected field in the list
    /// </summary>
    public FieldInfo? SelectedField
    {
        get => _selectedField;
        set => SetProperty(ref _selectedField, value);
    }

    /// <summary>
    /// Last save time for current field
    /// </summary>
    public DateTime? LastSaveTime
    {
        get => _lastSaveTime;
        set => SetProperty(ref _lastSaveTime, value);
    }

    public ICommand NewFieldCommand { get; }
    public ICommand OpenFieldCommand { get; }
    public ICommand SaveFieldCommand { get; }
    public ICommand SaveAsFieldCommand { get; }
    public ICommand DeleteFieldCommand { get; }
    public ICommand ImportBoundaryCommand { get; }
    public ICommand ExportBoundaryCommand { get; }

    private void OnNewField()
    {
        try
        {
            // Dialog to get field name will be implemented
            SetError("New field dialog not yet implemented");
        }
        catch (Exception ex)
        {
            SetError($"Failed to create new field: {ex.Message}");
        }
    }

    private void OnOpenField()
    {
        try
        {
            if (SelectedField == null)
            {
                SetError("No field selected");
                return;
            }

            var field = _fieldService.LoadField(SelectedField.Path);
            _fieldService.SetActiveField(field);

            CurrentFieldName = field.Name;
            LastSaveTime = DateTime.Now;
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to open field: {ex.Message}");
        }
    }

    private void OnSaveField()
    {
        try
        {
            var activeField = _fieldService.ActiveField;
            if (activeField == null)
            {
                SetError("No active field to save");
                return;
            }

            _fieldService.SaveField(activeField);
            LastSaveTime = DateTime.Now;
            LoadAvailableFields();
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to save field: {ex.Message}");
        }
    }

    private void OnSaveAsField()
    {
        try
        {
            // Dialog to get new field name will be implemented
            SetError("Save as dialog not yet implemented");
        }
        catch (Exception ex)
        {
            SetError($"Failed to save field as: {ex.Message}");
        }
    }

    private void OnDeleteField()
    {
        try
        {
            if (SelectedField == null)
            {
                SetError("No field selected");
                return;
            }

            // Confirmation dialog will be implemented
            _fieldService.DeleteField(SelectedField.Path);
            LoadAvailableFields();
            SelectedField = null;
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to delete field: {ex.Message}");
        }
    }

    private void OnImportBoundary()
    {
        try
        {
            // File picker dialog will be implemented
            // Support for GeoJSON and KML formats
            SetError("Import boundary dialog not yet implemented");
        }
        catch (Exception ex)
        {
            SetError($"Failed to import boundary: {ex.Message}");
        }
    }

    private void OnExportBoundary()
    {
        try
        {
            // File save dialog will be implemented
            // Support for GeoJSON and KML formats
            SetError("Export boundary dialog not yet implemented");
        }
        catch (Exception ex)
        {
            SetError($"Failed to export boundary: {ex.Message}");
        }
    }

    private void OnActiveFieldChanged(object? sender, Field? field)
    {
        UpdateCurrentField();
        LoadAvailableFields();
    }

    private void UpdateCurrentField()
    {
        var activeField = _fieldService.ActiveField;
        CurrentFieldName = activeField?.Name ?? "No Field";
    }

    private void LoadAvailableFields()
    {
        try
        {
            // Get fields directory from configuration (placeholder path)
            var fieldsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/AgValoniaGPS/Fields";

            var fieldNames = _fieldService.GetAvailableFields(fieldsDirectory);

            AvailableFields.Clear();
            foreach (var fieldName in fieldNames)
            {
                var fieldPath = System.IO.Path.Combine(fieldsDirectory, fieldName);

                // Create FieldInfo with placeholder values
                // Real implementation would read actual field metadata
                var fieldInfo = new FieldInfo
                {
                    Name = fieldName,
                    Path = fieldPath,
                    AreaHectares = 0,
                    DateModified = DateTime.Now,
                    DateCreated = DateTime.Now,
                    BoundaryPointCount = 0,
                    HasBoundary = false
                };

                AvailableFields.Add(fieldInfo);
            }

            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to load available fields: {ex.Message}");
        }
    }
}
