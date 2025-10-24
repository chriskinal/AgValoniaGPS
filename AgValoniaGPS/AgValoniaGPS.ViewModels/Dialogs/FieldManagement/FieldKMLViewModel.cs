using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AgValoniaGPS.Services.FieldOperations;
using AgValoniaGPS.ViewModels.Base;

namespace AgValoniaGPS.ViewModels.Dialogs.FieldManagement;

/// <summary>
/// Simple class to represent a KML feature for display
/// </summary>
public class KMLFeature
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Polygon, LineString, Point
    public int PointCount { get; set; }
}

/// <summary>
/// ViewModel for KML file import dialog (FormFieldKML)
/// </summary>
public class FieldKMLViewModel : DialogViewModelBase
{
    private readonly IBoundaryFileService? _boundaryFileService;
    private string _kmlFilePath = string.Empty;
    private KMLFeature? _selectedFeature;
    private int _pointCount;
    private string _previewText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldKMLViewModel"/> class.
    /// </summary>
    /// <param name="boundaryFileService">Optional boundary file service.</param>
    public FieldKMLViewModel(IBoundaryFileService? boundaryFileService = null)
    {
        _boundaryFileService = boundaryFileService;

        Features = new ObservableCollection<KMLFeature>();

        BrowseKMLCommand = new RelayCommand(OnBrowseKML);
        ImportCommand = new RelayCommand(OnImport);
    }

    /// <summary>
    /// Gets the features found in the KML file.
    /// </summary>
    public ObservableCollection<KMLFeature> Features { get; }

    /// <summary>
    /// Gets or sets the selected KML file path.
    /// </summary>
    public string KMLFilePath
    {
        get => _kmlFilePath;
        set
        {
            SetProperty(ref _kmlFilePath, value);
            ParseKMLFile();
        }
    }

    /// <summary>
    /// Gets or sets the selected feature to import.
    /// </summary>
    public KMLFeature? SelectedFeature
    {
        get => _selectedFeature;
        set
        {
            SetProperty(ref _selectedFeature, value);
            if (value != null)
            {
                PointCount = value.PointCount;
            }
        }
    }

    /// <summary>
    /// Gets or sets the number of boundary points in the selected feature.
    /// </summary>
    public int PointCount
    {
        get => _pointCount;
        set => SetProperty(ref _pointCount, value);
    }

    /// <summary>
    /// Gets or sets the KML structure preview text.
    /// </summary>
    public string PreviewText
    {
        get => _previewText;
        set => SetProperty(ref _previewText, value);
    }

    /// <summary>
    /// Gets the command to open file picker for KML.
    /// </summary>
    public ICommand BrowseKMLCommand { get; }

    /// <summary>
    /// Gets the command to import the selected feature.
    /// </summary>
    public ICommand ImportCommand { get; }

    /// <summary>
    /// Opens file picker to select KML file.
    /// </summary>
    private void OnBrowseKML()
    {
        // In a real implementation, would open file picker dialog
        // For now, placeholder
    }

    /// <summary>
    /// Imports the selected KML feature as a boundary.
    /// </summary>
    private void OnImport()
    {
        if (SelectedFeature == null)
        {
            SetError("Please select a feature to import.");
            return;
        }

        if (string.IsNullOrWhiteSpace(KMLFilePath))
        {
            SetError("No KML file selected.");
            return;
        }

        try
        {
            // In a real implementation, would use _boundaryFileService to import
            // For now, just close with success
            DialogResult = true;
            RequestClose(true);
        }
        catch (Exception ex)
        {
            SetError($"Error importing KML: {ex.Message}");
        }
    }

    /// <summary>
    /// Parses the KML file and populates the features list.
    /// </summary>
    private void ParseKMLFile()
    {
        Features.Clear();
        PreviewText = string.Empty;
        ClearError();

        if (string.IsNullOrWhiteSpace(KMLFilePath) || !System.IO.File.Exists(KMLFilePath))
        {
            return;
        }

        try
        {
            // In a real implementation, would parse KML file
            // For now, create sample features for testing
            Features.Add(new KMLFeature { Name = "Field Boundary", Type = "Polygon", PointCount = 45 });
            Features.Add(new KMLFeature { Name = "Inner Boundary", Type = "Polygon", PointCount = 12 });

            PreviewText = $"KML File: {System.IO.Path.GetFileName(KMLFilePath)}\nFeatures: {Features.Count}";
        }
        catch (Exception ex)
        {
            SetError($"Error parsing KML file: {ex.Message}");
        }
    }
}
