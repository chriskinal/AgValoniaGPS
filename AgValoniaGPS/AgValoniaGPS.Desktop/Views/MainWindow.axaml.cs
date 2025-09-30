using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using AgValoniaGPS.ViewModels;

namespace AgValoniaGPS.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Set DataContext from DI
        if (App.Services != null)
        {
            DataContext = App.Services.GetRequiredService<MainViewModel>();
        }
    }
}