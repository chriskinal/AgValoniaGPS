using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using AgValoniaGPS.Models;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

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

        AddFlagCommand = ReactiveCommand.Create(OnAddFlag);
        EditFlagCommand = ReactiveCommand.Create(OnEditFlag,
            this.WhenAnyValue(x => x.SelectedFlag).Select(flag => flag != null));
        DeleteFlagCommand = ReactiveCommand.Create(OnDeleteFlag,
            this.WhenAnyValue(x => x.SelectedFlag).Select(flag => flag != null));
        ClearAllCommand = ReactiveCommand.Create(OnClearAll,
            this.WhenAnyValue(x => x.FlagCount).Select(count => count > 0));

        // React to search text changes
        this.WhenAnyValue(x => x.SearchText)
            .Subscribe(_ => FilterFlags());
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
        set => this.RaiseAndSetIfChanged(ref _selectedFlag, value);
    }

    /// <summary>
    /// Gets or sets the search text for filtering flags.
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
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
        this.RaisePropertyChanged(nameof(FlagCount));
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
            this.RaisePropertyChanged(nameof(FlagCount));
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
        this.RaisePropertyChanged(nameof(FlagCount));
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
