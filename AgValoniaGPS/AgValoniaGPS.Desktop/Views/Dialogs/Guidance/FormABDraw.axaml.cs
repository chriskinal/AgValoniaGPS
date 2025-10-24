using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AgValoniaGPS.Models;
using AgValoniaGPS.ViewModels.Dialogs.Guidance;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Guidance;

public partial class FormABDraw : Window
{
    private Canvas? _drawCanvas;

    public FormABDraw()
    {
        InitializeComponent();

        DataContextChanged += (_, _) =>
        {
            if (DataContext is ABDrawViewModel vm)
            {
                vm.CloseRequested += (_, result) => Close(result);
            }
        };

        // Find the canvas and attach click handler
        _drawCanvas = this.FindControl<Canvas>("DrawCanvas");
        if (_drawCanvas != null)
        {
            _drawCanvas.PointerPressed += OnCanvasClick;
        }
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void OnCanvasClick(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not ABDrawViewModel viewModel) return;
        if (_drawCanvas == null) return;

        var point = e.GetPosition(_drawCanvas);

        // Convert canvas coordinates to lat/lon (placeholder conversion)
        // In a real implementation, this would use proper coordinate transformation
        // based on the current field view bounds
        double lat = 42.0 + (point.Y / _drawCanvas.Height) * 0.01;
        double lon = -93.6 + (point.X / _drawCanvas.Width) * 0.01;

        var position = new Position { Latitude = lat, Longitude = lon };
        viewModel.OnCanvasClick(position);
    }
}
