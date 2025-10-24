using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;
using AgValoniaGPS.Models;
using AgValoniaGPS.ViewModels.Base;

namespace AgValoniaGPS.ViewModels.Dialogs.FieldManagement;

/// <summary>
/// ViewModel for creating or editing a field flag (FormEnterFlag)
/// </summary>
public class EnterFlagViewModel : DialogViewModelBase
{
    private string _flagName = string.Empty;
    private double _latitude;
    private double _longitude;
    private string _flagColorHex = "#FF0000"; // Red by default
    private string _notes = string.Empty;
    private Position? _currentPosition;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnterFlagViewModel"/> class.
    /// </summary>
    public EnterFlagViewModel()
    {
        PickColorCommand = new RelayCommand(OnPickColor);
        UseCurrentPositionCommand = new RelayCommand(OnUseCurrentPosition);
    }

    /// <summary>
    /// Initializes a new instance with an existing flag for editing.
    /// </summary>
    /// <param name="flag">The flag to edit.</param>
    public EnterFlagViewModel(FieldFlag flag) : this()
    {
        _flagName = flag.Name;
        _latitude = flag.Position.Latitude;
        _longitude = flag.Position.Longitude;
        _flagColorHex = flag.ColorHex;
        _notes = flag.Notes;
    }

    /// <summary>
    /// Gets or sets the flag name (required, 1-30 chars).
    /// </summary>
    public string FlagName
    {
        get => _flagName;
        set
        {
            SetProperty(ref _flagName, value);
            ClearError();
        }
    }

    /// <summary>
    /// Gets or sets the flag latitude (-90 to 90).
    /// </summary>
    public double Latitude
    {
        get => _latitude;
        set
        {
            SetProperty(ref _latitude, value);
            ClearError();
        }
    }

    /// <summary>
    /// Gets or sets the flag longitude (-180 to 180).
    /// </summary>
    public double Longitude
    {
        get => _longitude;
        set
        {
            SetProperty(ref _longitude, value);
            ClearError();
        }
    }

    /// <summary>
    /// Gets or sets the flag color as hex string (e.g., "#FF0000").
    /// </summary>
    public string FlagColorHex
    {
        get => _flagColorHex;
        set => SetProperty(ref _flagColorHex, value);
    }

    /// <summary>
    /// Gets or sets optional notes for the flag.
    /// </summary>
    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    /// <summary>
    /// Gets or sets the current GPS position (for "Use Current Position" feature).
    /// </summary>
    public Position? CurrentPosition
    {
        get => _currentPosition;
        set => SetProperty(ref _currentPosition, value);
    }

    /// <summary>
    /// Gets the command to open color picker.
    /// </summary>
    public ICommand PickColorCommand { get; }

    /// <summary>
    /// Gets the command to use current GPS position.
    /// </summary>
    public ICommand UseCurrentPositionCommand { get; }

    /// <summary>
    /// Creates a FieldFlag object from the current values.
    /// </summary>
    /// <returns>A new FieldFlag instance.</returns>
    public FieldFlag ToFieldFlag()
    {
        return new FieldFlag
        {
            Name = FlagName,
            Position = new Position
            {
                Latitude = Latitude,
                Longitude = Longitude
            },
            ColorHex = FlagColorHex,
            Notes = Notes,
            DateCreated = DateTime.Now
        };
    }

    /// <summary>
    /// Validates flag data before accepting.
    /// </summary>
    protected override void OnOK()
    {
        // Validate flag name
        if (string.IsNullOrWhiteSpace(FlagName))
        {
            SetError("Flag name is required.");
            return;
        }

        if (FlagName.Length > 30)
        {
            SetError("Flag name must be 30 characters or less.");
            return;
        }

        // Validate latitude
        if (Latitude < -90 || Latitude > 90)
        {
            SetError("Latitude must be between -90 and 90 degrees.");
            return;
        }

        // Validate longitude
        if (Longitude < -180 || Longitude > 180)
        {
            SetError("Longitude must be between -180 and 180 degrees.");
            return;
        }

        base.OnOK();
    }

    /// <summary>
    /// Called when Pick Color button is clicked.
    /// This would open a color picker dialog (FormColorPicker).
    /// </summary>
    private void OnPickColor()
    {
        // In a real implementation, this would open FormColorPicker
        // and set FlagColorHex to the selected color
        // For now, this is a placeholder
    }

    /// <summary>
    /// Called when Use Current Position button is clicked.
    /// Sets Latitude and Longitude from CurrentPosition.
    /// </summary>
    private void OnUseCurrentPosition()
    {
        if (CurrentPosition != null)
        {
            Latitude = CurrentPosition.Latitude;
            Longitude = CurrentPosition.Longitude;
        }
        else
        {
            SetError("No GPS position available.");
        }
    }
}
