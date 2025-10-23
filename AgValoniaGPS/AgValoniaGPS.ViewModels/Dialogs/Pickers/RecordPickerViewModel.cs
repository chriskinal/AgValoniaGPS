using System;
using AgValoniaGPS.ViewModels.Base;

namespace AgValoniaGPS.ViewModels.Dialogs.Pickers;

/// <summary>
/// Generic ViewModel for record picker dialog that allows selecting any type of record from a list with search functionality.
/// </summary>
/// <typeparam name="T">The type of record to pick from.</typeparam>
public class RecordPickerViewModel<T> : PickerViewModelBase<T>
{
    private string _title = "Select Record";
    private Func<T, string, bool> _customFilterPredicate;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordPickerViewModel{T}"/> class.
    /// </summary>
    public RecordPickerViewModel()
    {
        _customFilterPredicate = DefaultFilterPredicate;
    }

    /// <summary>
    /// Initializes a new instance with a title and custom filter.
    /// </summary>
    /// <param name="title">The title for the picker dialog.</param>
    /// <param name="filterPredicate">Optional custom filter predicate. If null, uses ToString() matching.</param>
    public RecordPickerViewModel(string title, Func<T, string, bool>? filterPredicate = null)
    {
        _title = title;
        _customFilterPredicate = filterPredicate ?? DefaultFilterPredicate;
    }

    /// <summary>
    /// Gets or sets the title of the picker dialog.
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value ?? "Select Record");
    }

    /// <summary>
    /// Sets a custom filter predicate for filtering records.
    /// </summary>
    /// <param name="predicate">The filter predicate function.</param>
    public void SetFilterPredicate(Func<T, string, bool> predicate)
    {
        _customFilterPredicate = predicate ?? DefaultFilterPredicate;
    }

    /// <summary>
    /// Filters records based on the custom or default predicate.
    /// </summary>
    protected override bool FilterPredicate(T item, string searchText)
    {
        if (item == null) return false;
        return _customFilterPredicate(item, searchText);
    }

    /// <summary>
    /// Default filter predicate that uses ToString() matching.
    /// </summary>
    private bool DefaultFilterPredicate(T item, string searchText)
    {
        if (item == null) return false;

        var itemString = item.ToString() ?? string.Empty;
        var search = searchText.ToLowerInvariant();

        return itemString.ToLowerInvariant().Contains(search);
    }
}
