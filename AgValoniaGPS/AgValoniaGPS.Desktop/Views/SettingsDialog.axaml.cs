using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AgValoniaGPS.ViewModels;

namespace AgValoniaGPS.Desktop.Views;

public partial class SettingsDialog : Window
{
    private MainViewModel? ViewModel => DataContext as MainViewModel;

    public SettingsDialog()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void BtnLoadSettings_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel == null) return;

        // For now, load the "Default" profile
        // In a full implementation, this would show a file picker dialog
        await ViewModel.LoadVehicleSettingsAsync("Default");
    }

    private async void BtnSaveSettings_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel == null) return;

        await ViewModel.SaveVehicleSettingsAsync();
    }

    private async void BtnRefreshSession_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel == null) return;

        await ViewModel.UpdateSessionStateDisplayAsync();
    }

    private void BtnClose_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
