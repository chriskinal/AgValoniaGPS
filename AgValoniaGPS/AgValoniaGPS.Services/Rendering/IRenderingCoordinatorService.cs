using System;

namespace AgValoniaGPS.Services.Rendering;

/// <summary>
/// Coordinates rendering by aggregating geometry from all backend services
/// and providing GPU-ready render data to the rendering engine.
/// </summary>
/// <remarks>
/// This service acts as a bridge between the business logic services (position,
/// guidance, coverage, etc.) and the OpenGL rendering layer. It subscribes to
/// events from all services, regenerates geometry when data changes, and
/// provides optimized render data to the rendering pipeline.
///
/// Thread Safety: This service must be accessed from the UI thread only
/// Performance: Geometry regeneration should complete in <5ms per update
/// </remarks>
public interface IRenderingCoordinatorService
{
    /// <summary>
    /// Gets current vehicle render data (position, heading, mesh).
    /// </summary>
    /// <returns>Vehicle render data or null if no vehicle data available</returns>
    VehicleRenderData? GetVehicleData();

    /// <summary>
    /// Gets current boundary render data (field boundary lines).
    /// </summary>
    /// <returns>Boundary render data or null if no boundary loaded</returns>
    BoundaryRenderData? GetBoundaryData();

    /// <summary>
    /// Gets current guidance line render data (AB line, curve, or contour).
    /// </summary>
    /// <returns>Guidance render data or null if no guidance active</returns>
    GuidanceRenderData? GetGuidanceData();

    /// <summary>
    /// Gets current coverage map render data (triangle mesh).
    /// </summary>
    /// <returns>Coverage render data or null if no coverage recorded</returns>
    CoverageRenderData? GetCoverageData();

    /// <summary>
    /// Gets current section state render data (overlay rectangles).
    /// </summary>
    /// <returns>Section render data or null if no sections configured</returns>
    SectionRenderData? GetSectionData();

    /// <summary>
    /// Gets current tram line render data.
    /// </summary>
    /// <returns>Tram line render data or null if no tram lines</returns>
    TramLineRenderData? GetTramLineData();

    /// <summary>
    /// Indicates whether any geometry has changed since last MarkClean() call.
    /// </summary>
    bool IsDirty { get; }

    /// <summary>
    /// Marks all geometry as clean (up-to-date).
    /// Should be called after rendering completes.
    /// </summary>
    void MarkClean();

    /// <summary>
    /// Manually invalidates specific geometry type, forcing regeneration.
    /// </summary>
    /// <param name="type">Type of geometry to invalidate</param>
    void InvalidateGeometry(GeometryType type);

    /// <summary>
    /// Raised when any geometry changes and a re-render is needed.
    /// </summary>
    event EventHandler? GeometryChanged;
}
