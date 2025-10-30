using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using AgValoniaGPS.Services.GPS;
using AgValoniaGPS.Services.FieldOperations;
using AgValoniaGPS.Services.Guidance;
using AgValoniaGPS.Services.Section;
using AgValoniaGPS.Services.Rendering;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Desktop.Views.Controls;

/// <summary>
/// 2D Field Map renderer using Avalonia's DrawingContext
/// Displays vehicle, boundaries, guidance lines, and coverage
/// </summary>
public partial class FieldMapControl : Control
{
    // Services
    private readonly IPositionUpdateService? _positionService;
    private readonly IBoundaryManagementService? _boundaryService;
    private readonly IABLineService? _abLineService;
    private readonly ICoverageMapService? _coverageService;
    private readonly IVehicleGeometryService? _vehicleGeometryService;
    private readonly IBoundaryGeometryService? _boundaryGeometryService;
    private readonly IGuidanceGeometryService? _guidanceGeometryService;
    private readonly ICoverageGeometryService? _coverageGeometryService;

    // Camera/viewport state
    private Point _cameraPosition = new Point(0, 0); // World position in meters
    private double _zoomLevel = 1.0; // Meters per pixel
    private double _minZoom = 0.1; // 10cm per pixel (very close)
    private double _maxZoom = 50.0; // 50m per pixel (very far)

    // Pan state
    private bool _isPanning = false;
    private Point _lastPanPoint;

    // Rendering state
    private GeoCoord? _vehiclePosition;
    private double _vehicleHeading = 0.0;
    private Position[]? _boundaryPoints;
    private ABLine? _activeABLine;
    private List<Models.Section.CoverageTriangle>? _coverageTriangles;

    // Rendering timer
    private DispatcherTimer? _renderTimer;

    // Brushes and pens (cached for performance)
    private readonly IBrush _backgroundBrush = new SolidColorBrush(Color.Parse("#0a0a0a"));
    private readonly IBrush _vehicleBrush = new SolidColorBrush(Color.Parse("#4CAF50"));
    private readonly IBrush _boundaryBrush = new SolidColorBrush(Color.Parse("#2196F3"));
    private readonly IPen _boundaryPen;
    private readonly IBrush _guidanceBrush = new SolidColorBrush(Color.Parse("#FF9800"));
    private readonly IPen _guidancePen;
    private readonly IBrush _coverageBrush = new SolidColorBrush(Color.FromArgb(128, 76, 175, 80));
    private readonly IBrush _overlapBrush = new SolidColorBrush(Color.FromArgb(128, 255, 152, 0));

    // Grid rendering (made more visible)
    private readonly IBrush _gridBrush = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255));
    private readonly IPen _gridPen;

    public FieldMapControl()
    {
        InitializeComponent();

        // Initialize pens
        _boundaryPen = new Pen(_boundaryBrush, 2.0);
        _guidancePen = new Pen(_guidanceBrush, 3.0);
        _gridPen = new Pen(_gridBrush, 1.0);

        // Try to get services from DI (they may be null in designer)
        if (Design.IsDesignMode)
            return;

        try
        {
            var services = App.Services;
            if (services != null)
            {
                _positionService = services.GetService(typeof(IPositionUpdateService)) as IPositionUpdateService;
                _boundaryService = services.GetService(typeof(IBoundaryManagementService)) as IBoundaryManagementService;
                _abLineService = services.GetService(typeof(IABLineService)) as IABLineService;
                _coverageService = services.GetService(typeof(ICoverageMapService)) as ICoverageMapService;
                _vehicleGeometryService = services.GetService(typeof(IVehicleGeometryService)) as IVehicleGeometryService;
                _boundaryGeometryService = services.GetService(typeof(IBoundaryGeometryService)) as IBoundaryGeometryService;
                _guidanceGeometryService = services.GetService(typeof(IGuidanceGeometryService)) as IGuidanceGeometryService;
                _coverageGeometryService = services.GetService(typeof(ICoverageGeometryService)) as ICoverageGeometryService;

                // Subscribe to service events
                if (_positionService != null)
                {
                    _positionService.PositionUpdated += OnPositionUpdated;
                }

                if (_boundaryService != null)
                {
                    // Note: IBoundaryManagementService doesn't expose a BoundaryChanged event
                    // We'll just check for boundaries on each render
                }

                if (_abLineService != null)
                {
                    _abLineService.ABLineChanged += OnABLineChanged;
                }

                if (_coverageService != null)
                {
                    _coverageService.CoverageUpdated += OnCoverageUpdated;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FieldMapControl: Error initializing services: {ex.Message}");
        }

        // Set up rendering timer (30 FPS)
        _renderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33)
        };
        _renderTimer.Tick += (s, e) => InvalidateVisual();
        _renderTimer.Start();

        // Initialize with some default zoom
        _zoomLevel = 2.0; // 2 meters per pixel
    }

    private void OnPositionUpdated(object? sender, PositionUpdateEventArgs e)
    {
        _vehiclePosition = e.Position;
        _vehicleHeading = e.Heading;

        // Follow vehicle with camera
        _cameraPosition = new Point(e.Position.Easting, e.Position.Northing);
    }

    private void OnABLineChanged(object? sender, ABLineChangedEventArgs e)
    {
        _activeABLine = e.Line;
    }

    private void OnCoverageUpdated(object? sender, Models.Events.CoverageMapUpdatedEventArgs e)
    {
        // Reload all coverage triangles
        if (_coverageService != null)
        {
            _coverageTriangles = _coverageService.GetAllTriangles().ToList();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        // Clear background
        context.FillRectangle(_backgroundBrush, new Rect(0, 0, Bounds.Width, Bounds.Height));

        // Draw grid (with more visible lines)
        DrawGrid(context);

        // Update boundary if available
        if (_boundaryService?.HasBoundary() == true)
        {
            _boundaryPoints = _boundaryService.GetCurrentBoundary();
        }

        // Draw coverage map (lowest layer)
        DrawCoverageMap(context);

        // Draw boundaries
        DrawBoundaries(context);

        // Draw guidance line
        DrawGuidanceLine(context);

        // Draw vehicle (top layer)
        DrawVehicle(context);

        // Draw info overlay
        DrawInfoOverlay(context);
    }

    private void DrawGrid(DrawingContext context)
    {
        // Draw a simple grid at 10 meter intervals
        double gridSpacing = 10.0; // meters

        // Calculate visible world bounds
        var topLeft = ScreenToWorld(new Point(0, 0));
        var bottomRight = ScreenToWorld(new Point(Bounds.Width, Bounds.Height));

        // Draw vertical lines
        double startX = Math.Floor(topLeft.X / gridSpacing) * gridSpacing;
        for (double x = startX; x <= bottomRight.X; x += gridSpacing)
        {
            var p1 = WorldToScreen(new Point(x, topLeft.Y));
            var p2 = WorldToScreen(new Point(x, bottomRight.Y));
            context.DrawLine(_gridPen, p1, p2);
        }

        // Draw horizontal lines
        double startY = Math.Floor(bottomRight.Y / gridSpacing) * gridSpacing;
        for (double y = startY; y <= topLeft.Y; y += gridSpacing)
        {
            var p1 = WorldToScreen(new Point(topLeft.X, y));
            var p2 = WorldToScreen(new Point(bottomRight.X, y));
            context.DrawLine(_gridPen, p1, p2);
        }
    }

    private void DrawVehicle(DrawingContext context)
    {
        if (_vehiclePosition == null)
        {
            // Draw a placeholder dot in the center
            var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
            context.FillRectangle(_vehicleBrush, new Rect(center.X - 10, center.Y - 10, 20, 20));
            return;
        }

        var screenPos = WorldToScreen(new Point(_vehiclePosition.Easting, _vehiclePosition.Northing));

        // Draw simple vehicle representation (rectangle with heading arrow)
        double vehicleLength = 5.0 / _zoomLevel; // 5 pixels at any zoom
        double vehicleWidth = 3.0 / _zoomLevel;

        // Save context state
        using (context.PushTransform(Matrix.CreateTranslation(screenPos.X, screenPos.Y)))
        using (context.PushTransform(Matrix.CreateRotation(_vehicleHeading)))
        {
            // Draw vehicle body
            var rect = new Rect(-vehicleWidth / 2, -vehicleLength / 2, vehicleWidth, vehicleLength);
            context.FillRectangle(_vehicleBrush, rect);

            // Draw heading arrow
            var arrowPoints = new[]
            {
                new Point(0, -vehicleLength / 2 - 5),
                new Point(-3, -vehicleLength / 2),
                new Point(3, -vehicleLength / 2)
            };

            var geometry = new PolylineGeometry(arrowPoints, true);
            context.DrawGeometry(_vehicleBrush, null, geometry);
        }
    }

    private void DrawBoundaries(DrawingContext context)
    {
        if (_boundaryPoints == null || _boundaryPoints.Length < 3)
            return;

        // Convert boundary points to screen coordinates
        var screenPoints = _boundaryPoints
            .Select(p => WorldToScreen(new Point(p.Easting, p.Northing)))
            .ToList();

        if (screenPoints.Count < 2)
            return;

        // Draw boundary lines
        for (int i = 0; i < screenPoints.Count; i++)
        {
            var p1 = screenPoints[i];
            var p2 = screenPoints[(i + 1) % screenPoints.Count];
            context.DrawLine(_boundaryPen, p1, p2);
        }
    }

    private void DrawGuidanceLine(DrawingContext context)
    {
        if (_activeABLine == null)
            return;

        // Use PointA as the origin
        var origin = new Point(_activeABLine.PointA.Easting, _activeABLine.PointA.Northing);
        double heading = _activeABLine.Heading; // Already in radians

        // Calculate line endpoints extending far beyond visible area
        double extension = 10000.0; // 10km in each direction
        var p1World = new Point(
            origin.X - extension * Math.Sin(heading),
            origin.Y + extension * Math.Cos(heading)
        );
        var p2World = new Point(
            origin.X + extension * Math.Sin(heading),
            origin.Y - extension * Math.Cos(heading)
        );

        var p1Screen = WorldToScreen(p1World);
        var p2Screen = WorldToScreen(p2World);

        context.DrawLine(_guidancePen, p1Screen, p2Screen);

        // Draw origin point
        var originScreen = WorldToScreen(origin);
        context.FillRectangle(_guidanceBrush, new Rect(originScreen.X - 4, originScreen.Y - 4, 8, 8));
    }

    private void DrawCoverageMap(DrawingContext context)
    {
        if (_coverageTriangles == null || _coverageTriangles.Count == 0)
            return;

        foreach (var triangle in _coverageTriangles)
        {
            if (triangle.Vertices == null || triangle.Vertices.Length < 3)
                continue;

            var p1 = WorldToScreen(new Point(triangle.Vertices[0].Easting, triangle.Vertices[0].Northing));
            var p2 = WorldToScreen(new Point(triangle.Vertices[1].Easting, triangle.Vertices[1].Northing));
            var p3 = WorldToScreen(new Point(triangle.Vertices[2].Easting, triangle.Vertices[2].Northing));

            var points = new[] { p1, p2, p3 };
            var geometry = new PolylineGeometry(points, true);

            // Color based on overlap count
            var brush = triangle.OverlapCount > 1 ? _overlapBrush : _coverageBrush;
            context.DrawGeometry(brush, null, geometry);
        }
    }

    private void DrawInfoOverlay(DrawingContext context)
    {
        // Draw zoom level and position info
        var text = $"Zoom: {_zoomLevel:F2} m/px";
        if (_vehiclePosition != null)
        {
            text += $"\nPos: {_vehiclePosition.Easting:F1}, {_vehiclePosition.Northing:F1}";
            text += $"\nHeading: {(_vehicleHeading * 180 / Math.PI):F1}°";
        }

        var formattedText = new FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            14,
            Brushes.White);

        context.DrawText(formattedText, new Point(10, 10));
    }

    // Coordinate conversion
    private Point WorldToScreen(Point worldPos)
    {
        // Convert world coordinates (meters) to screen pixels
        double offsetX = worldPos.X - _cameraPosition.X;
        double offsetY = worldPos.Y - _cameraPosition.Y;

        return new Point(
            Bounds.Width / 2 + offsetX / _zoomLevel,
            Bounds.Height / 2 - offsetY / _zoomLevel  // Flip Y axis
        );
    }

    private Point ScreenToWorld(Point screenPos)
    {
        // Convert screen pixels to world coordinates (meters)
        double offsetX = (screenPos.X - Bounds.Width / 2) * _zoomLevel;
        double offsetY = -(screenPos.Y - Bounds.Height / 2) * _zoomLevel; // Flip Y axis

        return new Point(
            _cameraPosition.X + offsetX,
            _cameraPosition.Y + offsetY
        );
    }

    // Input handling
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isPanning = true;
            _lastPanPoint = e.GetPosition(this);
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_isPanning)
        {
            var currentPoint = e.GetPosition(this);
            var delta = currentPoint - _lastPanPoint;

            // Move camera in world space
            _cameraPosition = new Point(
                _cameraPosition.X - delta.X * _zoomLevel,
                _cameraPosition.Y + delta.Y * _zoomLevel // Flip Y
            );

            _lastPanPoint = currentPoint;
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_isPanning)
        {
            _isPanning = false;
            e.Handled = true;
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        // Zoom in/out
        double zoomFactor = e.Delta.Y > 0 ? 0.9 : 1.1;
        _zoomLevel *= zoomFactor;

        // Clamp zoom level
        _zoomLevel = Math.Max(_minZoom, Math.Min(_maxZoom, _zoomLevel));

        e.Handled = true;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        // Clean up
        _renderTimer?.Stop();

        if (_positionService != null)
        {
            _positionService.PositionUpdated -= OnPositionUpdated;
        }

        if (_abLineService != null)
        {
            _abLineService.ABLineChanged -= OnABLineChanged;
        }

        if (_coverageService != null)
        {
            _coverageService.CoverageUpdated -= OnCoverageUpdated;
        }
    }
}
