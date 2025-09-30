using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using AgValoniaGPS.ViewModels;

namespace AgValoniaGPS.Desktop.Views;

public partial class MainWindow : Window
{
    private MainViewModel? ViewModel => DataContext as MainViewModel;
    private bool _isDraggingSection = false;
    private Avalonia.Point _dragStartPoint;

    public MainWindow()
    {
        InitializeComponent();

        // Set DataContext from DI
        if (App.Services != null)
        {
            DataContext = App.Services.GetRequiredService<MainViewModel>();
        }
    }

    private async void BtnNtripConnect_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            await ViewModel.ConnectToNtripAsync();
        }
    }

    private async void BtnNtripDisconnect_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            await ViewModel.DisconnectFromNtripAsync();
        }
    }

    private void BtnDataIO_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new DataIODialog
        {
            DataContext = ViewModel
        };
        dialog.ShowDialog(this);
    }

    // Drag functionality for Section Control
    private void SectionControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border)
        {
            _isDraggingSection = true;
            _dragStartPoint = e.GetPosition(this);
            e.Pointer.Capture(border);
        }
    }

    private void SectionControl_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDraggingSection && sender is Border border)
        {
            var currentPoint = e.GetPosition(this);
            var delta = currentPoint - _dragStartPoint;

            // Update position
            Canvas.SetLeft(border, Canvas.GetLeft(border) + delta.X);
            Canvas.SetTop(border, Canvas.GetTop(border) + delta.Y);

            _dragStartPoint = currentPoint;
        }
    }

    private void SectionControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDraggingSection)
        {
            _isDraggingSection = false;
            if (sender is Border border)
            {
                e.Pointer.Capture(null);
            }
        }
    }
}