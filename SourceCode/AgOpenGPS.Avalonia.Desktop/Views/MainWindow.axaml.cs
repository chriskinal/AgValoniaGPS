using Avalonia.Controls;
using Avalonia.Threading;
using AgOpenGPS.Avalonia.OpenGL;
using AgOpenGPS.Avalonia.ViewModels;
using System;

namespace AgOpenGPS.Avalonia.Desktop.Views;

public partial class MainWindow : Window
{
    private readonly TestRenderer _testRenderer;
    private readonly DispatcherTimer _renderTimer;
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        _testRenderer = new TestRenderer();

        // Wire up OpenGL events
        OpenGLControl.GlInitialized += OnGlInitialized;
        OpenGLControl.GlRender += OnGlRender;

        // Set up render timer for continuous rendering (60 FPS target)
        _renderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _renderTimer.Tick += (s, e) => OpenGLControl.RequestRender();
        _renderTimer.Start();

        _viewModel.UpdateStatus("Initializing OpenGL...");
    }

    private void OnGlInitialized(object? sender, GlInterfaceEventArgs e)
    {
        _testRenderer.Initialize(e.GlInterface);
        _viewModel.UpdateStatus("OpenGL initialized successfully");
    }

    private void OnGlRender(object? sender, GlInterfaceEventArgs e)
    {
        var width = (int)OpenGLControl.Bounds.Width;
        var height = (int)OpenGLControl.Bounds.Height;

        if (width > 0 && height > 0)
        {
            _testRenderer.Render(e.GlInterface, width, height);
        }
    }
}