using CommunityToolkit.Mvvm.Input;
using AgValoniaGPS.Models;
using AgValoniaGPS.Services;
using AgValoniaGPS.Services.GPS;
using AgValoniaGPS.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace AgValoniaGPS.ViewModels.Panels.FieldManagement;

/// <summary>
/// ViewModel for the Field Flags panel allowing users to mark points of interest in the field.
/// Provides flag creation, deletion, and management capabilities.
/// </summary>
public partial class FormFlagsViewModel : PanelViewModelBase
{
    private readonly IFieldService _fieldService;
    private readonly IPositionUpdateService _positionService;

    private FieldFlag? _selectedFlag;
    private bool _canAddFlag = true;
    private bool _canDeleteFlag;

    public FormFlagsViewModel(
        IFieldService fieldService,
        IPositionUpdateService positionService)
    {
        _fieldService = fieldService ?? throw new ArgumentNullException(nameof(fieldService));
        _positionService = positionService ?? throw new ArgumentNullException(nameof(positionService));

        Title = "Field Flags";

        Flags = new ObservableCollection<FieldFlag>();

        // Commands
        AddFlagCommand = new RelayCommand(OnAddFlag);
        DeleteFlagCommand = new RelayCommand(OnDeleteFlag);
        EditFlagCommand = new RelayCommand(OnEditFlag);
        ClearAllFlagsCommand = new RelayCommand(OnClearAllFlags);

        // Load existing flags
        LoadFlags();
    }

    public string Title { get; } = "Field Flags";

    /// <summary>
    /// Collection of all field flags
    /// </summary>
    public ObservableCollection<FieldFlag> Flags { get; }

    /// <summary>
    /// Currently selected flag
    /// </summary>
    public FieldFlag? SelectedFlag
    {
        get => _selectedFlag;
        set
        {
            SetProperty(ref _selectedFlag, value);
            CanDeleteFlag = value != null;
        }
    }

    /// <summary>
    /// Whether a flag can be added (GPS position available)
    /// </summary>
    public bool CanAddFlag
    {
        get => _canAddFlag;
        set => SetProperty(ref _canAddFlag, value);
    }

    /// <summary>
    /// Whether the selected flag can be deleted
    /// </summary>
    public bool CanDeleteFlag
    {
        get => _canDeleteFlag;
        set => SetProperty(ref _canDeleteFlag, value);
    }

    public ICommand AddFlagCommand { get; }
    public ICommand DeleteFlagCommand { get; }
    public ICommand EditFlagCommand { get; }
    public ICommand ClearAllFlagsCommand { get; }

    private void OnAddFlag()
    {
        try
        {
            var geoCoord = _positionService.GetCurrentPosition();
            if (geoCoord == null)
            {
                SetError("No GPS position available");
                return;
            }

            var heading = _positionService.GetCurrentHeading();
            var speed = _positionService.GetCurrentSpeed();

            var position = new Position
            {
                Easting = geoCoord.Easting,
                Northing = geoCoord.Northing,
                Altitude = geoCoord.Altitude,
                Heading = heading * 180.0 / Math.PI, // Radians to degrees
                Speed = speed
            };

            var flag = new FieldFlag
            {
                Id = Guid.NewGuid(),
                Name = $"Flag {Flags.Count + 1}",
                Position = position,
                ColorHex = GetNextFlagColor(),
                Notes = string.Empty,
                DateCreated = DateTime.Now
            };

            Flags.Add(flag);
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to add flag: {ex.Message}");
        }
    }

    private void OnDeleteFlag()
    {
        if (SelectedFlag != null)
        {
            Flags.Remove(SelectedFlag);
            SelectedFlag = null;
            ClearError();
        }
    }

    private void OnEditFlag()
    {
        // Dialog for editing flag name/color will be implemented in future task
        if (SelectedFlag != null)
        {
            SetError("Edit flag dialog not yet implemented");
        }
    }

    private void OnClearAllFlags()
    {
        Flags.Clear();
        SelectedFlag = null;
        ClearError();
    }

    private void LoadFlags()
    {
        // Load flags from field service when field is active
        if (_fieldService.ActiveField != null)
        {
            // Flags will be loaded from field data in future implementation
            ClearError();
        }
    }

    private string GetNextFlagColor()
    {
        // Cycle through a set of colors
        var colors = new[] { "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#FF00FF", "#00FFFF", "#FFA500" };
        return colors[Flags.Count % colors.Length];
    }
}
