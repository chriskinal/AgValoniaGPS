using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using AgValoniaGPS.ViewModels.Base;

namespace AgValoniaGPS.ViewModels.Dialogs.Pickers;

/// <summary>
/// ViewModel for drive/volume picker dialog that displays available drives with their information.
/// </summary>
public class DrivePickerViewModel : PickerViewModelBase<DriveInfoItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DrivePickerViewModel"/> class.
    /// </summary>
    public DrivePickerViewModel()
    {
        RefreshDrivesCommand = new RelayCommand(LoadDrives);
        LoadDrives();
    }

    /// <summary>
    /// Gets the command to refresh the drive list.
    /// </summary>
    public ICommand RefreshDrivesCommand { get; }

    /// <summary>
    /// Loads available drives from the system.
    /// </summary>
    private void LoadDrives()
    {
        try
        {
            IsBusy = true;
            ClearError();

            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady)
                .Select(d => new DriveInfoItem(d))
                .ToList();

            Items = new System.Collections.ObjectModel.ObservableCollection<DriveInfoItem>(drives);

            if (Items.Count == 0)
            {
                SetError("No ready drives found.");
            }
        }
        catch (Exception ex)
        {
            SetError($"Error loading drives: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Filters drives based on search text.
    /// </summary>
    protected override bool FilterPredicate(DriveInfoItem item, string searchText)
    {
        if (item == null) return false;

        var search = searchText.ToLowerInvariant();
        return item.Name.ToLowerInvariant().Contains(search) ||
               item.Label.ToLowerInvariant().Contains(search) ||
               item.DriveType.ToString().ToLowerInvariant().Contains(search);
    }
}

/// <summary>
/// Represents information about a drive for display in the picker.
/// </summary>
public class DriveInfoItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DriveInfoItem"/> class.
    /// </summary>
    /// <param name="driveInfo">The DriveInfo to wrap.</param>
    public DriveInfoItem(DriveInfo driveInfo)
    {
        Name = driveInfo.Name;
        Label = string.IsNullOrWhiteSpace(driveInfo.VolumeLabel) ? "Local Disk" : driveInfo.VolumeLabel;
        DriveType = driveInfo.DriveType;
        TotalSizeBytes = driveInfo.TotalSize;
        FreeSizeBytes = driveInfo.AvailableFreeSpace;
        DriveFormat = driveInfo.DriveFormat;
    }

    /// <summary>
    /// Gets the drive name (e.g., "C:\").
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the drive label/volume name.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Gets the drive type (Fixed, Removable, Network, etc.).
    /// </summary>
    public DriveType DriveType { get; }

    /// <summary>
    /// Gets the total size in bytes.
    /// </summary>
    public long TotalSizeBytes { get; }

    /// <summary>
    /// Gets the free space in bytes.
    /// </summary>
    public long FreeSizeBytes { get; }

    /// <summary>
    /// Gets the file system format (NTFS, FAT32, etc.).
    /// </summary>
    public string DriveFormat { get; }

    /// <summary>
    /// Gets the total size formatted as a human-readable string.
    /// </summary>
    public string TotalSize => FormatBytes(TotalSizeBytes);

    /// <summary>
    /// Gets the free space formatted as a human-readable string.
    /// </summary>
    public string FreeSpace => FormatBytes(FreeSizeBytes);

    /// <summary>
    /// Gets the used space percentage (0-100).
    /// </summary>
    public double UsedPercentage => TotalSizeBytes > 0 ? ((TotalSizeBytes - FreeSizeBytes) / (double)TotalSizeBytes) * 100 : 0;

    /// <summary>
    /// Gets the free space percentage (0-100).
    /// </summary>
    public double FreePercentage => 100 - UsedPercentage;

    /// <summary>
    /// Gets a formatted display string for the drive.
    /// </summary>
    public string DisplayText => $"{Name} ({Label}) - {FreeSpace} free of {TotalSize}";

    /// <summary>
    /// Gets the icon name based on drive type.
    /// </summary>
    public string IconName => DriveType switch
    {
        DriveType.Fixed => "HardDrive",
        DriveType.Removable => "UsbDrive",
        DriveType.Network => "NetworkDrive",
        DriveType.CDRom => "CdDrive",
        _ => "Drive"
    };

    /// <summary>
    /// Formats bytes into a human-readable string (B, KB, MB, GB, TB).
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
