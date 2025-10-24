using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace AgValoniaGPS.ViewModels.Base;

/// <summary>
/// Base class for picker dialogs that allow selecting an item from a list with search functionality.
/// </summary>
/// <typeparam name="T">The type of items to pick from.</typeparam>
public abstract class PickerViewModelBase<T> : DialogViewModelBase
{
    private ObservableCollection<T> _items = new();
    private ObservableCollection<T> _filteredItems = new();
    private T? _selectedItem;
    private string _searchText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="PickerViewModelBase{T}"/> class.
    /// </summary>
    protected PickerViewModelBase()
    {
        // FilteredItems will be updated automatically when SearchText changes via property setter
    }

    /// <summary>
    /// Gets or sets the full collection of items available for selection.
    /// </summary>
    public ObservableCollection<T> Items
    {
        get => _items;
        set
        {
            SetProperty(ref _items, value);
            UpdateFilteredItems();
        }
    }

    /// <summary>
    /// Gets the filtered collection of items based on the current search text.
    /// This is a computed property that updates automatically when Items or SearchText changes.
    /// </summary>
    public ObservableCollection<T> FilteredItems
    {
        get => _filteredItems;
        private set => SetProperty(ref _filteredItems, value);
    }

    /// <summary>
    /// Gets or sets the currently selected item.
    /// </summary>
    public T? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    /// <summary>
    /// Gets or sets the search text used to filter items.
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                UpdateFilteredItems();
        }
    }

    /// <summary>
    /// Gets a value indicating whether any items are available.
    /// </summary>
    public bool HasItems => Items.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the filtered list is empty.
    /// </summary>
    public bool HasNoFilteredItems => FilteredItems.Count == 0 && !string.IsNullOrWhiteSpace(SearchText);

    /// <summary>
    /// Abstract method that derived classes must implement to define how items are filtered
    /// based on the search text.
    /// </summary>
    /// <param name="item">The item to test against the filter.</param>
    /// <param name="searchText">The current search text.</param>
    /// <returns>True if the item matches the filter and should be shown, false otherwise.</returns>
    protected abstract bool FilterPredicate(T item, string searchText);

    /// <summary>
    /// Updates the filtered items collection based on the current search text.
    /// </summary>
    private void UpdateFilteredItems()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            // No filter - show all items
            FilteredItems = new ObservableCollection<T>(Items);
        }
        else
        {
            // Apply filter using the derived class's predicate
            var filtered = Items.Where(item => FilterPredicate(item, SearchText)).ToList();
            FilteredItems = new ObservableCollection<T>(filtered);
        }

        // Raise property changed for computed properties
        OnPropertyChanged(nameof(HasItems));
        OnPropertyChanged(nameof(HasNoFilteredItems));
    }

    /// <summary>
    /// Called when OK command is executed. Validates that an item is selected.
    /// </summary>
    protected override void OnOK()
    {
        if (SelectedItem == null)
        {
            SetError("Please select an item.");
            return;
        }

        ClearError();
        base.OnOK();
    }
}
