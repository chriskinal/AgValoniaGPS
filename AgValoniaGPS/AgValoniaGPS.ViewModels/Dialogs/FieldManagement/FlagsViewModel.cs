using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AgValoniaGPS.Models;
using AgValoniaGPS.ViewModels.Base;
using CommunityToolkit.Mvvm.Input;

namespace AgValoniaGPS.ViewModels.Dialogs.FieldManagement;

/// <summary>
/// ViewModel for flag management list dialog (FormFlags)
/// </summary>
public class FlagsViewModel : DialogViewModelBase
{
    private FieldFlag? _selectedFlag;
    private string _searchText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlagsViewModel"/> class.
    /// </summary>
    public FlagsViewModel()
    {
        Flags = new ObservableCollection<FieldFlag>();
        FilteredFlags = new ObservableCollection<FieldFlag>();

        AddFlagCommand = new RelayCommand(OnAddFlag);
        EditFlagCommand = new RelayCommand(OnEditFlag);
        DeleteFlagCommand = new RelayCommand(OnDeleteFlag);
        ClearAllCommand = new RelayCommand(OnClearAll);
    }

    /// <summary>
    /// Gets all flags collection.
    /// </summary>
    public ObservableCollection<FieldFlag> Flags { get; }

    /// <summary>
    /// Gets the filtered flags based on search text.
    /// </summary>
    public ObservableCollection<FieldFlag> FilteredFlags { get; }

    /// <summary>
    /// Gets or sets the currently selected flag.
    /// </summary>
    public FieldFlag? SelectedFlag
    {
        get => _selectedFlag;
        set => SetProperty(ref _selectedFlag, value);
    }

    /// <summary>
    /// Gets or sets the search text for filtering flags.
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                FilterFlags();
        }
    }

    /// <summary>
    /// Gets the total number of flags.
    /// </summary>
    public int FlagCount => Flags.Count;

    /// <summary>
    /// Gets the command to add a new flag.
    /// </summary>
    public ICommand AddFlagCommand { get; }

    /// <summary>
    /// Gets the command to edit the selected flag.
    /// </summary>
    public ICommand EditFlagCommand { get; }

    /// <summary>
    /// Gets the command to delete the selected flag.
    /// </summary>
    public ICommand DeleteFlagCommand { get; }

    /// <summary>
    /// Gets the command to clear all flags.
    /// </summary>
    public ICommand ClearAllCommand { get; }

    /// <summary>
    /// Event raised when add flag is requested (to open FormEnterFlag).
    /// </summary>
    public event EventHandler? AddFlagRequested;

    /// <summary>
    /// Event raised when edit flag is requested (to open FormEnterFlag).
    /// </summary>
    public event EventHandler<FieldFlag>? EditFlagRequested;

    /// <summary>
    /// Adds a flag to the collection.
    /// </summary>
    /// <param name="flag">The flag to add.</param>
    public void AddFlag(FieldFlag flag)
    {
        Flags.Add(flag);
        FilterFlags();
        OnPropertyChanged(nameof(FlagCount));
    }

    /// <summary>
    /// Updates an existing flag.
    /// </summary>
    /// <param name="oldFlag">The flag to replace.</param>
    /// <param name="newFlag">The updated flag.</param>
    public void UpdateFlag(FieldFlag oldFlag, FieldFlag newFlag)
    {
        var index = Flags.IndexOf(oldFlag);
        if (index >= 0)
        {
            Flags[index] = newFlag;
            FilterFlags();
        }
    }

    /// <summary>
    /// Called when Add Flag button is clicked.
    /// </summary>
    private void OnAddFlag()
    {
        AddFlagRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when Edit Flag button is clicked.
    /// </summary>
    private void OnEditFlag()
    {
        if (SelectedFlag != null)
        {
            EditFlagRequested?.Invoke(this, SelectedFlag);
        }
    }

    /// <summary>
    /// Called when Delete Flag button is clicked.
    /// </summary>
    private void OnDeleteFlag()
    {
        if (SelectedFlag != null)
        {
            // In a real implementation, would show confirmation dialog
            Flags.Remove(SelectedFlag);
            FilterFlags();
            OnPropertyChanged(nameof(FlagCount));
            SelectedFlag = null;
        }
    }

    /// <summary>
    /// Called when Clear All button is clicked.
    /// </summary>
    private void OnClearAll()
    {
        // In a real implementation, would show confirmation dialog
        Flags.Clear();
        FilterFlags();
        OnPropertyChanged(nameof(FlagCount));
    }

    /// <summary>
    /// Filters flags based on search text.
    /// </summary>
    private void FilterFlags()
    {
        FilteredFlags.Clear();

        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? Flags
            : Flags.Where(f => f.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        foreach (var flag in filtered)
        {
            FilteredFlags.Add(flag);
        }
    }
}
