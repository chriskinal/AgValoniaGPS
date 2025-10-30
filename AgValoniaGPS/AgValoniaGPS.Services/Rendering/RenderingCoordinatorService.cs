using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Services.GPS;
using AgValoniaGPS.Services.Guidance;
using AgValoniaGPS.Services.Section;
using AgValoniaGPS.Services.FieldOperations;

namespace AgValoniaGPS.Services.Rendering;

/// <summary>
/// Coordinates rendering by aggregating geometry from all backend services.
/// Subscribes to service events and regenerates GPU-ready geometry on demand.
/// </summary>
/// <remarks>
/// Thread Safety: Must be accessed from UI thread only
/// Performance Target: Geometry regeneration <5ms per update
/// </remarks>
public class RenderingCoordinatorService : IRenderingCoordinatorService
{
    // Backend services (data sources)
    private readonly IPositionUpdateService _positionService;
    private readonly IABLineService _abLineService;
    private readonly ICurveLineService _curveLineService;
    private readonly IContourService _contourService;
    private readonly ISectionControlService _sectionControlService;
    private readonly IBoundaryManagementService _boundaryService;
    private readonly ITramLineService _tramLineService;
    private readonly ICoverageMapService _coverageMapService;

    // Geometry generation services
    private readonly IVehicleGeometryService _vehicleGeometryService;
    private readonly IBoundaryGeometryService _boundaryGeometryService;
    private readonly IGuidanceGeometryService _guidanceGeometryService;
    private readonly ICoverageGeometryService _coverageGeometryService;
    private readonly ISectionGeometryService _sectionGeometryService;

    // Configuration
    private readonly VehicleConfiguration _vehicleConfig;

    // Cached render data
    private VehicleRenderData? _vehicleData;
    private BoundaryRenderData? _boundaryData;
    private GuidanceRenderData? _guidanceData;
    private CoverageRenderData? _coverageData;
    private SectionRenderData? _sectionData;
    private TramLineRenderData? _tramLineData;

    // Dirty flags
    private readonly HashSet<GeometryType> _dirtyGeometry = new();
    private bool _isDirty;

    // Current state
    private Position? _currentPosition;
    private double _currentHeading;

    /// <summary>
    /// Raised when any geometry changes and a re-render is needed.
    /// </summary>
    public event EventHandler? GeometryChanged;

    public RenderingCoordinatorService(
        IPositionUpdateService positionService,
        IABLineService abLineService,
        ICurveLineService curveLineService,
        IContourService contourService,
        ISectionControlService sectionControlService,
        IBoundaryManagementService boundaryService,
        ITramLineService tramLineService,
        ICoverageMapService coverageMapService,
        IVehicleGeometryService vehicleGeometryService,
        IBoundaryGeometryService boundaryGeometryService,
        IGuidanceGeometryService guidanceGeometryService,
        ICoverageGeometryService coverageGeometryService,
        ISectionGeometryService sectionGeometryService,
        VehicleConfiguration vehicleConfig)
    {
        _positionService = positionService ?? throw new ArgumentNullException(nameof(positionService));
        _abLineService = abLineService ?? throw new ArgumentNullException(nameof(abLineService));
        _curveLineService = curveLineService ?? throw new ArgumentNullException(nameof(curveLineService));
        _contourService = contourService ?? throw new ArgumentNullException(nameof(contourService));
        _sectionControlService = sectionControlService ?? throw new ArgumentNullException(nameof(sectionControlService));
        _boundaryService = boundaryService ?? throw new ArgumentNullException(nameof(boundaryService));
        _tramLineService = tramLineService ?? throw new ArgumentNullException(nameof(tramLineService));
        _coverageMapService = coverageMapService ?? throw new ArgumentNullException(nameof(coverageMapService));
        _vehicleGeometryService = vehicleGeometryService ?? throw new ArgumentNullException(nameof(vehicleGeometryService));
        _boundaryGeometryService = boundaryGeometryService ?? throw new ArgumentNullException(nameof(boundaryGeometryService));
        _guidanceGeometryService = guidanceGeometryService ?? throw new ArgumentNullException(nameof(guidanceGeometryService));
        _coverageGeometryService = coverageGeometryService ?? throw new ArgumentNullException(nameof(coverageGeometryService));
        _sectionGeometryService = sectionGeometryService ?? throw new ArgumentNullException(nameof(sectionGeometryService));
        _vehicleConfig = vehicleConfig ?? throw new ArgumentNullException(nameof(vehicleConfig));

        // Subscribe to service events
        SubscribeToServiceEvents();
    }

    private void SubscribeToServiceEvents()
    {
        // Position updates (Wave 1)
        _positionService.PositionUpdated += OnPositionUpdated;

        // Guidance line changes (Wave 2)
        _abLineService.ABLineChanged += OnABLineChanged;
        _curveLineService.CurveChanged += OnCurveLineChanged;
        _contourService.StateChanged += OnContourStateChanged;

        // Section state changes (Wave 4)
        _sectionControlService.SectionStateChanged += OnSectionStateChanged;

        // Boundary changes (Wave 5)
        // Note: IBoundaryManagementService doesn't have a Changed event, so we'll need to
        // manually call InvalidateGeometry(GeometryType.Boundaries) when boundaries are loaded

        // Tram line changes (Wave 5)
        // Note: ITramLineService doesn't have a Changed event, so we'll need to
        // manually call InvalidateGeometry(GeometryType.TramLines) when tram lines change

        // Coverage map updates (Wave 6B)
        _coverageMapService.CoverageUpdated += OnCoverageMapUpdated;
    }

    private void OnPositionUpdated(object? sender, PositionUpdateEventArgs e)
    {
        // Update current position
        _currentPosition = new Position
        {
            Easting = e.Position.Easting,
            Northing = e.Position.Northing,
            Altitude = e.Position.Altitude
        };
        _currentHeading = e.Heading;

        // Regenerate vehicle geometry
        RegenerateVehicleGeometry();

        // Mark vehicle geometry as dirty
        MarkGeometryDirty(GeometryType.Vehicle);
    }

    private void OnABLineChanged(object? sender, EventArgs e)
    {
        // Regenerate guidance geometry
        RegenerateGuidanceGeometry();
        MarkGeometryDirty(GeometryType.Guidance);
    }

    private void OnCurveLineChanged(object? sender, EventArgs e)
    {
        RegenerateGuidanceGeometry();
        MarkGeometryDirty(GeometryType.Guidance);
    }

    private void OnContourStateChanged(object? sender, EventArgs e)
    {
        RegenerateGuidanceGeometry();
        MarkGeometryDirty(GeometryType.Guidance);
    }

    private void OnSectionStateChanged(object? sender, EventArgs e)
    {
        // Regenerate section overlay geometry
        RegenerateSectionGeometry();
        MarkGeometryDirty(GeometryType.Sections);
    }

    private void OnCoverageMapUpdated(object? sender, EventArgs e)
    {
        // Regenerate coverage mesh
        RegenerateCoverageGeometry();
        MarkGeometryDirty(GeometryType.Coverage);
    }

    private void MarkGeometryDirty(GeometryType type)
    {
        _dirtyGeometry.Add(type);
        _isDirty = true;
        GeometryChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RegenerateVehicleGeometry()
    {
        if (_currentPosition == null)
        {
            _vehicleData = null;
            _dirtyGeometry.Remove(GeometryType.Vehicle);
            return;
        }

        // Generate vehicle mesh using geometry service
        var vehicleVertices = _vehicleGeometryService.GenerateVehicleMesh(
            _vehicleConfig,
            r: 0.3f, g: 0.6f, b: 1.0f); // Blue color

        _vehicleData = new VehicleRenderData
        {
            Position = new Vector2((float)_currentPosition.Easting, (float)_currentPosition.Northing),
            Heading = (float)_currentHeading,
            Vertices = vehicleVertices,
            Color = new Vector3(0.3f, 0.6f, 1.0f)
        };

        // Clear dirty flag after regeneration
        _dirtyGeometry.Remove(GeometryType.Vehicle);
    }

    private void RegenerateBoundaryGeometry()
    {
        try
        {
            var boundary = _boundaryService.GetCurrentBoundary();
            if (boundary == null || boundary.Length == 0)
            {
                _boundaryData = null;
                _dirtyGeometry.Remove(GeometryType.Boundaries);
                return;
            }

            // Convert Position array to List
            var boundaryList = new List<Position>(boundary);

            // Generate boundary line vertices
            var vertices = _boundaryGeometryService.GenerateBoundaryLines(boundaryList);

            _boundaryData = new BoundaryRenderData
            {
                Vertices = vertices,
                Color = new Vector3(1.0f, 1.0f, 0.0f), // Yellow
                LineWidth = 3.0f
            };

            // Clear dirty flag after regeneration
            _dirtyGeometry.Remove(GeometryType.Boundaries);
        }
        catch (Exception ex)
        {
            // Log error but don't crash
            Console.WriteLine($"Error regenerating boundary geometry: {ex.Message}");
            _boundaryData = null;
        }
    }

    private void RegenerateGuidanceGeometry()
    {
        try
        {
            // For now, just implement AB line rendering
            // TODO: Add curve and contour support
            // Note: We need to track active line through a service that manages it
            // For now, just set to null until we have a guidance coordinator service
            _guidanceData = null;

            // Clear dirty flag after regeneration
            _dirtyGeometry.Remove(GeometryType.Guidance);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error regenerating guidance geometry: {ex.Message}");
            _guidanceData = null;
        }
    }

    private void RegenerateCoverageGeometry()
    {
        try
        {
            var triangles = _coverageMapService.GetAllTriangles();
            if (triangles == null || !triangles.Any())
            {
                _coverageData = null;
                _dirtyGeometry.Remove(GeometryType.Coverage);
                return;
            }

            var mesh = _coverageGeometryService.GenerateCoverageMesh(triangles);

            _coverageData = new CoverageRenderData
            {
                Vertices = mesh.Vertices,
                Indices = mesh.Indices,
                TriangleCount = mesh.TriangleCount
            };

            // Clear dirty flag after regeneration
            _dirtyGeometry.Remove(GeometryType.Coverage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error regenerating coverage geometry: {ex.Message}");
            _coverageData = null;
        }
    }

    private void RegenerateSectionGeometry()
    {
        try
        {
            if (_currentPosition == null)
            {
                _sectionData = null;
                _dirtyGeometry.Remove(GeometryType.Sections);
                return;
            }

            // Get section states and configuration
            // This is a placeholder - actual implementation depends on section service API
            // TODO: Implement full section overlay generation

            _sectionData = new SectionRenderData
            {
                Vertices = Array.Empty<float>(),
                SectionCount = 0
            };

            // Clear dirty flag after regeneration
            _dirtyGeometry.Remove(GeometryType.Sections);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error regenerating section geometry: {ex.Message}");
            _sectionData = null;
        }
    }

    private void RegenerateTramLineGeometry()
    {
        try
        {
            // TODO: Implement tram line geometry generation
            // This depends on ITramLineService API
            _tramLineData = null;

            // Clear dirty flag after regeneration
            _dirtyGeometry.Remove(GeometryType.TramLines);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error regenerating tram line geometry: {ex.Message}");
            _tramLineData = null;
        }
    }

    // Public API

    public VehicleRenderData? GetVehicleData()
    {
        // Regenerate if dirty
        if (_dirtyGeometry.Contains(GeometryType.Vehicle))
        {
            RegenerateVehicleGeometry();
        }
        return _vehicleData;
    }

    public BoundaryRenderData? GetBoundaryData()
    {
        // Regenerate if dirty or never generated
        if (_dirtyGeometry.Contains(GeometryType.Boundaries) || _boundaryData == null)
        {
            RegenerateBoundaryGeometry();
        }
        return _boundaryData;
    }

    public GuidanceRenderData? GetGuidanceData()
    {
        if (_dirtyGeometry.Contains(GeometryType.Guidance))
        {
            RegenerateGuidanceGeometry();
        }
        return _guidanceData;
    }

    public CoverageRenderData? GetCoverageData()
    {
        if (_dirtyGeometry.Contains(GeometryType.Coverage))
        {
            RegenerateCoverageGeometry();
        }
        return _coverageData;
    }

    public SectionRenderData? GetSectionData()
    {
        if (_dirtyGeometry.Contains(GeometryType.Sections))
        {
            RegenerateSectionGeometry();
        }
        return _sectionData;
    }

    public TramLineRenderData? GetTramLineData()
    {
        if (_dirtyGeometry.Contains(GeometryType.TramLines))
        {
            RegenerateTramLineGeometry();
        }
        return _tramLineData;
    }

    public bool IsDirty => _isDirty;

    public void MarkClean()
    {
        _dirtyGeometry.Clear();
        _isDirty = false;
    }

    public void InvalidateGeometry(GeometryType type)
    {
        MarkGeometryDirty(type);
    }
}
