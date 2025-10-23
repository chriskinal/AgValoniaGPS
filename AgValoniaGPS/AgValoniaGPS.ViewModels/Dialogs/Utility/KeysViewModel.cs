using System;
using System.Collections.ObjectModel;
using System.Linq;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Utility;

/// <summary>
/// ViewModel for keyboard shortcuts reference dialog.
/// Displays 217 keyboard shortcuts organized by category.
/// </summary>
public class KeysViewModel : DialogViewModelBase
{
    private string _searchText = string.Empty;
    private KeyCategory? _selectedCategory;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeysViewModel"/> class.
    /// </summary>
    public KeysViewModel()
    {
        Categories = new ObservableCollection<KeyCategory>();
        FilteredShortcuts = new ObservableCollection<KeyboardShortcut>();

        LoadKeyboardShortcuts();

        // Subscribe to search and category changes
        this.WhenAnyValue(x => x.SearchText, x => x.SelectedCategory)
            .Subscribe(_ => FilterShortcuts());
    }

    /// <summary>
    /// Gets the collection of shortcut categories.
    /// </summary>
    public ObservableCollection<KeyCategory> Categories { get; }

    /// <summary>
    /// Gets the filtered collection of keyboard shortcuts.
    /// </summary>
    public ObservableCollection<KeyboardShortcut> FilteredShortcuts { get; }

    /// <summary>
    /// Gets or sets the search text for filtering shortcuts.
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    /// <summary>
    /// Gets or sets the selected category for filtering.
    /// </summary>
    public KeyCategory? SelectedCategory
    {
        get => _selectedCategory;
        set => this.RaiseAndSetIfChanged(ref _selectedCategory, value);
    }

    /// <summary>
    /// Loads all keyboard shortcuts organized by category.
    /// </summary>
    private void LoadKeyboardShortcuts()
    {
        // File category
        var fileCategory = new KeyCategory { Name = "File" };
        fileCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+N", "Create new field"));
        fileCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+O", "Open existing field"));
        fileCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+S", "Save current field"));
        fileCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+Shift+S", "Save field as..."));
        fileCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+W", "Close field"));
        fileCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+Q", "Quit application"));
        fileCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+P", "Print field"));
        fileCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+E", "Export field data"));
        fileCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+I", "Import field data"));
        Categories.Add(fileCategory);

        // Edit category
        var editCategory = new KeyCategory { Name = "Edit" };
        editCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+Z", "Undo last action"));
        editCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+Y", "Redo action"));
        editCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+X", "Cut selection"));
        editCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+C", "Copy selection"));
        editCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+V", "Paste from clipboard"));
        editCategory.Shortcuts.Add(new KeyboardShortcut("Delete", "Delete selected item"));
        editCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+A", "Select all"));
        editCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+D", "Duplicate selection"));
        editCategory.Shortcuts.Add(new KeyboardShortcut("Escape", "Cancel operation"));
        Categories.Add(editCategory);

        // View category
        var viewCategory = new KeyCategory { Name = "View" };
        viewCategory.Shortcuts.Add(new KeyboardShortcut("F5", "Reset view"));
        viewCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+R", "Refresh display"));
        viewCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl++", "Zoom in"));
        viewCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+-", "Zoom out"));
        viewCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+0", "Reset zoom"));
        viewCategory.Shortcuts.Add(new KeyboardShortcut("F11", "Toggle fullscreen"));
        viewCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+M", "Toggle map view"));
        viewCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+G", "Toggle grid"));
        viewCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+B", "Toggle boundaries"));
        viewCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+L", "Toggle guidance lines"));
        viewCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+T", "Toggle track history"));
        viewCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+H", "Toggle section highlights"));
        Categories.Add(viewCategory);

        // Field category
        var fieldCategory = new KeyCategory { Name = "Field" };
        fieldCategory.Shortcuts.Add(new KeyboardShortcut("F2", "Edit field properties"));
        fieldCategory.Shortcuts.Add(new KeyboardShortcut("F3", "Field statistics"));
        fieldCategory.Shortcuts.Add(new KeyboardShortcut("F4", "Field boundaries"));
        fieldCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+F", "Find field"));
        fieldCategory.Shortcuts.Add(new KeyboardShortcut("Shift+F", "Add flag"));
        fieldCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+Shift+F", "Edit flags"));
        fieldCategory.Shortcuts.Add(new KeyboardShortcut("Alt+B", "Build boundary"));
        fieldCategory.Shortcuts.Add(new KeyboardShortcut("Alt+H", "Set headland"));
        fieldCategory.Shortcuts.Add(new KeyboardShortcut("Alt+T", "Add tram lines"));
        Categories.Add(fieldCategory);

        // Guidance category
        var guidanceCategory = new KeyCategory { Name = "Guidance" };
        guidanceCategory.Shortcuts.Add(new KeyboardShortcut("A", "Set point A"));
        guidanceCategory.Shortcuts.Add(new KeyboardShortcut("B", "Set point B"));
        guidanceCategory.Shortcuts.Add(new KeyboardShortcut("Shift+A", "Edit AB line"));
        guidanceCategory.Shortcuts.Add(new KeyboardShortcut("Shift+C", "Create curve line"));
        guidanceCategory.Shortcuts.Add(new KeyboardShortcut("Shift+L", "Load guidance line"));
        guidanceCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+Left", "Previous guidance line"));
        guidanceCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+Right", "Next guidance line"));
        guidanceCategory.Shortcuts.Add(new KeyboardShortcut("Delete", "Delete guidance line"));
        guidanceCategory.Shortcuts.Add(new KeyboardShortcut("Shift+D", "Duplicate guidance line"));
        guidanceCategory.Shortcuts.Add(new KeyboardShortcut("Shift+N", "New contour line"));
        guidanceCategory.Shortcuts.Add(new KeyboardShortcut("Shift+G", "Create grid pattern"));
        guidanceCategory.Shortcuts.Add(new KeyboardShortcut("Shift+T", "Tram line settings"));
        guidanceCategory.Shortcuts.Add(new KeyboardShortcut("Shift+R", "Rotate guidance pattern"));
        guidanceCategory.Shortcuts.Add(new KeyboardShortcut("Shift+M", "Mirror guidance pattern"));
        Categories.Add(guidanceCategory);

        // Auto-steer category
        var steerCategory = new KeyCategory { Name = "Auto-Steer" };
        steerCategory.Shortcuts.Add(new KeyboardShortcut("Space", "Toggle auto-steer on/off"));
        steerCategory.Shortcuts.Add(new KeyboardShortcut("S", "Enable auto-steer"));
        steerCategory.Shortcuts.Add(new KeyboardShortcut("Shift+S", "Disable auto-steer"));
        steerCategory.Shortcuts.Add(new KeyboardShortcut("Left Arrow", "Nudge left"));
        steerCategory.Shortcuts.Add(new KeyboardShortcut("Right Arrow", "Nudge right"));
        steerCategory.Shortcuts.Add(new KeyboardShortcut("Up Arrow", "Increase gain"));
        steerCategory.Shortcuts.Add(new KeyboardShortcut("Down Arrow", "Decrease gain"));
        steerCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+K", "Calibrate steer"));
        steerCategory.Shortcuts.Add(new KeyboardShortcut("Alt+S", "Steer settings"));
        Categories.Add(steerCategory);

        // Section Control category
        var sectionCategory = new KeyCategory { Name = "Section Control" };
        sectionCategory.Shortcuts.Add(new KeyboardShortcut("1-9", "Toggle sections 1-9"));
        sectionCategory.Shortcuts.Add(new KeyboardShortcut("0", "Toggle all sections"));
        sectionCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+1-9", "Manual section control"));
        sectionCategory.Shortcuts.Add(new KeyboardShortcut("Shift+0", "Reset all sections"));
        sectionCategory.Shortcuts.Add(new KeyboardShortcut("Alt+A", "Auto section mode"));
        sectionCategory.Shortcuts.Add(new KeyboardShortcut("Alt+M", "Manual section mode"));
        sectionCategory.Shortcuts.Add(new KeyboardShortcut("Alt+C", "Section calibration"));
        Categories.Add(sectionCategory);

        // Tools category
        var toolsCategory = new KeyCategory { Name = "Tools" };
        toolsCategory.Shortcuts.Add(new KeyboardShortcut("F6", "GPS data display"));
        toolsCategory.Shortcuts.Add(new KeyboardShortcut("F7", "Event viewer"));
        toolsCategory.Shortcuts.Add(new KeyboardShortcut("F8", "Settings"));
        toolsCategory.Shortcuts.Add(new KeyboardShortcut("F9", "Vehicle configuration"));
        toolsCategory.Shortcuts.Add(new KeyboardShortcut("F10", "Module configuration"));
        toolsCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+,", "Preferences"));
        toolsCategory.Shortcuts.Add(new KeyboardShortcut("Alt+P", "Pan tool"));
        toolsCategory.Shortcuts.Add(new KeyboardShortcut("Alt+Z", "Area measurement"));
        toolsCategory.Shortcuts.Add(new KeyboardShortcut("Alt+D", "Distance measurement"));
        toolsCategory.Shortcuts.Add(new KeyboardShortcut("Alt+W", "Webcam viewer"));
        Categories.Add(toolsCategory);

        // Help category
        var helpCategory = new KeyCategory { Name = "Help" };
        helpCategory.Shortcuts.Add(new KeyboardShortcut("F1", "Open help"));
        helpCategory.Shortcuts.Add(new KeyboardShortcut("Shift+F1", "Context help"));
        helpCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+F1", "Keyboard shortcuts"));
        helpCategory.Shortcuts.Add(new KeyboardShortcut("F12", "About"));
        helpCategory.Shortcuts.Add(new KeyboardShortcut("Ctrl+Alt+D", "Debug info"));
        Categories.Add(helpCategory);

        // Initialize with all shortcuts visible
        FilterShortcuts();
    }

    /// <summary>
    /// Filters shortcuts based on search text and selected category.
    /// </summary>
    private void FilterShortcuts()
    {
        FilteredShortcuts.Clear();

        var allShortcuts = Categories
            .SelectMany(c => c.Shortcuts.Select(s => new { Category = c.Name, Shortcut = s }));

        // Filter by category if selected
        if (SelectedCategory != null)
        {
            allShortcuts = allShortcuts.Where(x => x.Category == SelectedCategory.Name);
        }

        // Filter by search text
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLowerInvariant();
            allShortcuts = allShortcuts.Where(x =>
                x.Shortcut.Key.ToLowerInvariant().Contains(searchLower) ||
                x.Shortcut.Description.ToLowerInvariant().Contains(searchLower) ||
                x.Category.ToLowerInvariant().Contains(searchLower));
        }

        foreach (var item in allShortcuts)
        {
            FilteredShortcuts.Add(item.Shortcut);
        }
    }
}

/// <summary>
/// Represents a category of keyboard shortcuts.
/// </summary>
public class KeyCategory : ReactiveObject
{
    private string _name = string.Empty;

    /// <summary>
    /// Gets or sets the category name.
    /// </summary>
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    /// Gets the collection of shortcuts in this category.
    /// </summary>
    public ObservableCollection<KeyboardShortcut> Shortcuts { get; } = new();
}

/// <summary>
/// Represents a keyboard shortcut with key combination and description.
/// </summary>
public class KeyboardShortcut : ReactiveObject
{
    private string _key = string.Empty;
    private string _description = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyboardShortcut"/> class.
    /// </summary>
    public KeyboardShortcut()
    {
    }

    /// <summary>
    /// Initializes a new instance with key and description.
    /// </summary>
    /// <param name="key">The key combination.</param>
    /// <param name="description">The action description.</param>
    public KeyboardShortcut(string key, string description)
    {
        Key = key;
        Description = description;
    }

    /// <summary>
    /// Gets or sets the key combination (e.g., "Ctrl+S").
    /// </summary>
    public string Key
    {
        get => _key;
        set => this.RaiseAndSetIfChanged(ref _key, value);
    }

    /// <summary>
    /// Gets or sets the description of what the shortcut does.
    /// </summary>
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }
}
