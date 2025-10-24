using CommunityToolkit.Mvvm.Input;
using System;
using System.Text.RegularExpressions;
using AgValoniaGPS.ViewModels.Base;

namespace AgValoniaGPS.ViewModels.Dialogs.FieldManagement;

/// <summary>
/// ViewModel for field metadata editor dialog (FormFieldData)
/// Allows editing field properties including name, farm, client, and notes
/// </summary>
public class FieldDataViewModel : DialogViewModelBase
{
    private string _fieldName = string.Empty;
    private string _farmName = string.Empty;
    private string _clientName = string.Empty;
    private double _fieldArea;
    private DateTime _dateCreated = DateTime.Now;
    private DateTime _dateModified = DateTime.Now;
    private string _notes = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldDataViewModel"/> class.
    /// </summary>
    public FieldDataViewModel()
    {
        // No special initialization needed
    }

    /// <summary>
    /// Initializes a new instance with existing field data.
    /// </summary>
    /// <param name="fieldName">Initial field name.</param>
    /// <param name="fieldArea">Field area in hectares.</param>
    /// <param name="farmName">Optional farm name.</param>
    /// <param name="clientName">Optional client name.</param>
    /// <param name="notes">Optional notes.</param>
    /// <param name="dateCreated">Date field was created.</param>
    /// <param name="dateModified">Date field was last modified.</param>
    public FieldDataViewModel(
        string fieldName,
        double fieldArea,
        string farmName = "",
        string clientName = "",
        string notes = "",
        DateTime? dateCreated = null,
        DateTime? dateModified = null)
    {
        _fieldName = fieldName;
        _fieldArea = fieldArea;
        _farmName = farmName;
        _clientName = clientName;
        _notes = notes;
        _dateCreated = dateCreated ?? DateTime.Now;
        _dateModified = dateModified ?? DateTime.Now;
    }

    /// <summary>
    /// Gets or sets the field name (required, validated).
    /// </summary>
    public string FieldName
    {
        get => _fieldName;
        set
        {
            SetProperty(ref _fieldName, value);
            ClearError();
        }
    }

    /// <summary>
    /// Gets or sets the farm name.
    /// </summary>
    public string FarmName
    {
        get => _farmName;
        set => SetProperty(ref _farmName, value);
    }

    /// <summary>
    /// Gets or sets the client/owner name.
    /// </summary>
    public string ClientName
    {
        get => _clientName;
        set => SetProperty(ref _clientName, value);
    }

    /// <summary>
    /// Gets or sets the calculated field area in hectares (read-only in UI).
    /// </summary>
    public double FieldArea
    {
        get => _fieldArea;
        set => SetProperty(ref _fieldArea, value);
    }

    /// <summary>
    /// Gets or sets the date the field was created.
    /// </summary>
    public DateTime DateCreated
    {
        get => _dateCreated;
        set => SetProperty(ref _dateCreated, value);
    }

    /// <summary>
    /// Gets or sets the date the field was last modified.
    /// </summary>
    public DateTime DateModified
    {
        get => _dateModified;
        set => SetProperty(ref _dateModified, value);
    }

    /// <summary>
    /// Gets or sets user notes/description for the field.
    /// </summary>
    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    /// <summary>
    /// Validates field data before accepting changes.
    /// </summary>
    /// <returns>True if validation passes.</returns>
    protected override void OnOK()
    {
        // Validate field name
        if (string.IsNullOrWhiteSpace(FieldName))
        {
            SetError("Field name is required.");
            return;
        }

        if (FieldName.Length > 50)
        {
            SetError("Field name must be 50 characters or less.");
            return;
        }

        // Check for invalid filename characters
        if (ContainsInvalidFileNameChars(FieldName))
        {
            SetError("Field name contains invalid characters. Avoid: \\ / : * ? \" < > |");
            return;
        }

        // Validate farm name length
        if (FarmName.Length > 50)
        {
            SetError("Farm name must be 50 characters or less.");
            return;
        }

        // Validate client name length
        if (ClientName.Length > 50)
        {
            SetError("Client name must be 50 characters or less.");
            return;
        }

        // Update modified date on save
        DateModified = DateTime.Now;

        base.OnOK();
    }

    /// <summary>
    /// Checks if a string contains invalid filename characters.
    /// </summary>
    private bool ContainsInvalidFileNameChars(string name)
    {
        char[] invalidChars = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
        return name.IndexOfAny(invalidChars) >= 0;
    }
}
