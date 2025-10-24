using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Pickers;

/// <summary>
/// ViewModel for file picker dialog that provides file system navigation and file selection.
/// </summary>
public class FilePickerViewModel : DialogViewModelBase
{
    private string _currentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    private ObservableCollection<FileItem> _files = new();
    private FileItem? _selectedFile;
    private string _filterExtension = "*";
    private string _selectedFilePath = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilePickerViewModel"/> class.
    /// </summary>
    public FilePickerViewModel()
    {
        NavigateUpCommand = ReactiveCommand.Create(NavigateUp, this.WhenAnyValue(x => x.CanNavigateUp));
        NavigateToCommand = ReactiveCommand.Create<string>(NavigateTo);
        SelectFileCommand = ReactiveCommand.Create<FileItem>(SelectFile);

        LoadCurrentDirectory();
    }

    /// <summary>
    /// Initializes a new instance with a filter extension.
    /// </summary>
    /// <param name="filterExtension">The file extension to filter by (e.g., ".txt", ".json"). Use "*" for all files.</param>
    public FilePickerViewModel(string filterExtension) : this()
    {
        FilterExtension = filterExtension;
    }

    /// <summary>
    /// Gets or sets the current directory path.
    /// </summary>
    public string CurrentPath
    {
        get => _currentPath;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentPath, value ?? string.Empty);
            LoadCurrentDirectory();
            this.RaisePropertyChanged(nameof(CanNavigateUp));
        }
    }

    /// <summary>
    /// Gets the collection of files and directories in the current path.
    /// </summary>
    public ObservableCollection<FileItem> Files
    {
        get => _files;
        private set => this.RaiseAndSetIfChanged(ref _files, value);
    }

    /// <summary>
    /// Gets or sets the selected file or directory.
    /// </summary>
    public FileItem? SelectedFile
    {
        get => _selectedFile;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedFile, value);
            if (value != null)
            {
                SelectedFilePath = value.FullPath;
            }
        }
    }

    /// <summary>
    /// Gets or sets the filter extension for files (e.g., ".txt", ".json"). Use "*" for all files.
    /// </summary>
    public string FilterExtension
    {
        get => _filterExtension;
        set
        {
            this.RaiseAndSetIfChanged(ref _filterExtension, value ?? "*");
            LoadCurrentDirectory();
        }
    }

    /// <summary>
    /// Gets or sets the selected file path.
    /// </summary>
    public string SelectedFilePath
    {
        get => _selectedFilePath;
        set => this.RaiseAndSetIfChanged(ref _selectedFilePath, value ?? string.Empty);
    }

    /// <summary>
    /// Gets a value indicating whether navigation up is possible.
    /// </summary>
    public bool CanNavigateUp => Directory.GetParent(CurrentPath) != null;

    /// <summary>
    /// Gets the command to navigate up one directory level.
    /// </summary>
    public ICommand NavigateUpCommand { get; }

    /// <summary>
    /// Gets the command to navigate to a specific path.
    /// </summary>
    public ICommand NavigateToCommand { get; }

    /// <summary>
    /// Gets the command to select a file.
    /// </summary>
    public ICommand SelectFileCommand { get; }

    /// <summary>
    /// Navigates up one directory level.
    /// </summary>
    private void NavigateUp()
    {
        var parent = Directory.GetParent(CurrentPath);
        if (parent != null)
        {
            CurrentPath = parent.FullName;
        }
    }

    /// <summary>
    /// Navigates to a specific directory path.
    /// </summary>
    private void NavigateTo(string path)
    {
        if (Directory.Exists(path))
        {
            CurrentPath = path;
        }
    }

    /// <summary>
    /// Selects a file or navigates into a directory.
    /// </summary>
    private void SelectFile(FileItem file)
    {
        if (file.IsDirectory)
        {
            CurrentPath = file.FullPath;
        }
        else
        {
            SelectedFile = file;
        }
    }

    /// <summary>
    /// Loads files and directories from the current directory.
    /// </summary>
    private void LoadCurrentDirectory()
    {
        try
        {
            IsBusy = true;
            ClearError();

            var items = new ObservableCollection<FileItem>();

            // Get directories
            var directories = Directory.GetDirectories(CurrentPath)
                .Select(d => new FileItem(d, true))
                .OrderBy(d => d.Name);

            foreach (var dir in directories)
            {
                items.Add(dir);
            }

            // Get files (with filter)
            var files = Directory.GetFiles(CurrentPath)
                .Where(f => FilterExtension == "*" || Path.GetExtension(f).Equals(FilterExtension, StringComparison.OrdinalIgnoreCase))
                .Select(f => new FileItem(f, false))
                .OrderBy(f => f.Name);

            foreach (var file in files)
            {
                items.Add(file);
            }

            Files = items;
        }
        catch (UnauthorizedAccessException)
        {
            SetError("Access denied to this directory.");
        }
        catch (Exception ex)
        {
            SetError($"Error loading directory: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Validates that a file is selected before closing.
    /// </summary>
    protected override void OnOK()
    {
        if (string.IsNullOrWhiteSpace(SelectedFilePath))
        {
            SetError("Please select a file.");
            return;
        }

        if (!File.Exists(SelectedFilePath))
        {
            SetError("Selected file does not exist.");
            return;
        }

        ClearError();
        base.OnOK();
    }
}

/// <summary>
/// Represents a file or directory item for display in the file picker.
/// </summary>
public class FileItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileItem"/> class.
    /// </summary>
    /// <param name="path">The full path to the file or directory.</param>
    /// <param name="isDirectory">Whether this is a directory.</param>
    public FileItem(string path, bool isDirectory)
    {
        FullPath = path;
        IsDirectory = isDirectory;

        if (isDirectory)
        {
            Name = new DirectoryInfo(path).Name;
            Size = "--";
            Modified = Directory.GetLastWriteTime(path);
        }
        else
        {
            var fileInfo = new FileInfo(path);
            Name = fileInfo.Name;
            Size = FormatBytes(fileInfo.Length);
            Modified = fileInfo.LastWriteTime;
        }
    }

    /// <summary>
    /// Gets the full path to the file or directory.
    /// </summary>
    public string FullPath { get; }

    /// <summary>
    /// Gets the name of the file or directory.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets a value indicating whether this is a directory.
    /// </summary>
    public bool IsDirectory { get; }

    /// <summary>
    /// Gets the formatted size string.
    /// </summary>
    public string Size { get; }

    /// <summary>
    /// Gets the last modified date.
    /// </summary>
    public DateTime Modified { get; }

    /// <summary>
    /// Gets the formatted modified date string.
    /// </summary>
    public string ModifiedString => Modified.ToString("yyyy-MM-dd HH:mm");

    /// <summary>
    /// Gets the icon for the file or directory.
    /// </summary>
    public string Icon => IsDirectory ? "📁" : "📄";

    /// <summary>
    /// Formats bytes into a human-readable string.
    /// </summary>
    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
