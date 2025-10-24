using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AgValoniaGPS.ViewModels.Base;

namespace AgValoniaGPS.ViewModels.Dialogs.FieldManagement;

/// <summary>
/// Simple class to represent an ISOXML field for display
/// </summary>
public class ISOField
{
    public string Name { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public double Area { get; set; }
    public string Customer { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for ISOXML file import dialog (FormFieldISOXML)
/// </summary>
public class FieldISOXMLViewModel : DialogViewModelBase
{
    private string _isoxmlFilePath = string.Empty;
    private ISOField? _selectedField;
    private string _previewText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldISOXMLViewModel"/> class.
    /// </summary>
    public FieldISOXMLViewModel()
    {
        ISOFields = new ObservableCollection<ISOField>();

        BrowseCommand = new RelayCommand(OnBrowse);
        ImportCommand = new RelayCommand(OnImport);
    }

    /// <summary>
    /// Gets the fields found in the ISOXML file.
    /// </summary>
    public ObservableCollection<ISOField> ISOFields { get; }

    /// <summary>
    /// Gets or sets the selected ISOXML file path.
    /// </summary>
    public string ISOXMLFilePath
    {
        get => _isoxmlFilePath;
        set
        {
            SetProperty(ref _isoxmlFilePath, value);
            ParseISOXMLFile();
        }
    }

    /// <summary>
    /// Gets or sets the selected field to import.
    /// </summary>
    public ISOField? SelectedField
    {
        get => _selectedField;
        set
        {
            SetProperty(ref _selectedField, value);
            UpdatePreview();
        }
    }

    /// <summary>
    /// Gets or sets the ISOXML structure preview text.
    /// </summary>
    public string PreviewText
    {
        get => _previewText;
        set => SetProperty(ref _previewText, value);
    }

    /// <summary>
    /// Gets the command to open file picker.
    /// </summary>
    public ICommand BrowseCommand { get; }

    /// <summary>
    /// Gets the command to import the selected field.
    /// </summary>
    public ICommand ImportCommand { get; }

    /// <summary>
    /// Opens file picker to select ISOXML file.
    /// </summary>
    private void OnBrowse()
    {
        // In a real implementation, would open file picker
        // For now, placeholder
    }

    /// <summary>
    /// Imports the selected ISOXML field.
    /// </summary>
    private void OnImport()
    {
        if (SelectedField == null)
        {
            SetError("Please select a field to import.");
            return;
        }

        try
        {
            // In a real implementation, would parse and import ISOXML
            DialogResult = true;
            RequestClose(true);
        }
        catch (Exception ex)
        {
            SetError($"Error importing ISOXML: {ex.Message}");
        }
    }

    /// <summary>
    /// Parses the ISOXML file and populates the fields list.
    /// </summary>
    private void ParseISOXMLFile()
    {
        ISOFields.Clear();
        PreviewText = string.Empty;
        ClearError();

        if (string.IsNullOrWhiteSpace(ISOXMLFilePath) || !System.IO.File.Exists(ISOXMLFilePath))
        {
            return;
        }

        try
        {
            // In a real implementation, would parse ISOXML file
            // For now, create sample fields for testing
            ISOFields.Add(new ISOField { Name = "North Field", Id = "PFD1", Area = 45.5, Customer = "Smith Farm" });
            ISOFields.Add(new ISOField { Name = "South Field", Id = "PFD2", Area = 32.1, Customer = "Smith Farm" });

            PreviewText = $"ISOXML File: {System.IO.Path.GetFileName(ISOXMLFilePath)}\nFields: {ISOFields.Count}";
        }
        catch (Exception ex)
        {
            SetError($"Error parsing ISOXML file: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the preview text for the selected field.
    /// </summary>
    private void UpdatePreview()
    {
        if (SelectedField == null)
        {
            return;
        }

        PreviewText = $"Field: {SelectedField.Name}\n";
        PreviewText += $"ID: {SelectedField.Id}\n";
        PreviewText += $"Area: {SelectedField.Area:F2} ha\n";
        PreviewText += $"Customer: {SelectedField.Customer}";
    }
}
