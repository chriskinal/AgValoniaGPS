# Wave 11: OpenGL Rendering Engine - Specification

**Date**: 2025-10-30
**Wave**: 11
**Category**: Graphics Rendering & Visualization
**Dependencies**: Waves 1-10 (Backend services, UI panels, Service migration complete)
**Priority**: Critical (Core visualization system)

---

## Executive Summary

Wave 11 implements the OpenGL rendering engine that visualizes field operations in real-time. This is the core graphical component that displays the vehicle, field boundaries, guidance lines, coverage maps, section states, and all spatial data on an interactive 2D map. The rendering engine must achieve high performance (≥60 FPS), support touch/mouse interaction, and integrate seamlessly with the service layer from Waves 1-8.

**Key Objectives:**
- Implement cross-platform OpenGL rendering using Avalonia's OpenGL integration
- Render vehicle position, heading, and implement geometry
- Display field boundaries, guidance lines (AB, Curve, Contour), and tram lines
- Visualize coverage maps using triangle meshes (from Wave 6B)
- Show section states with color-coded overlays
- Provide interactive camera controls (pan, zoom, rotate, follow vehicle)
- Achieve ≥60 FPS performance with 10,000+ triangles
- Support both desktop (mouse/keyboard) and touch input

**Performance Targets:**
- Frame rate: ≥60 FPS under normal load
- Triangle throughput: 10,000+ triangles per frame
- Camera update latency: <16ms (one frame)
- Vehicle position update: <16ms from service event to screen
- Field load time: <500ms for typical field (100 hectares)

---

## 1. Architecture Overview

### 1.1 Rendering Pipeline

```
┌──────────────────────────────────────────────────────────┐
│                    Service Layer                          │
│  (Position, Guidance, Boundary, Section, Coverage)       │
└────────────────────┬─────────────────────────────────────┘
                     │ Events & Property Changes
                     ▼
┌──────────────────────────────────────────────────────────┐
│              Rendering Coordinator Service               │
│  - Aggregates data from all services                     │
│  - Manages render state                                  │
│  - Triggers frame updates                                │
└────────────────────┬─────────────────────────────────────┘
                     │
                     ▼
┌──────────────────────────────────────────────────────────┐
│                Geometry Generation Layer                  │
│  - VehicleGeometryService: Vehicle mesh                  │
│  - BoundaryGeometryService: Boundary lines/polygons      │
│  - GuidanceGeometryService: AB/Curve/Contour lines       │
│  - CoverageGeometryService: Triangle mesh from coverage  │
│  - SectionGeometryService: Section overlay rectangles    │
└────────────────────┬─────────────────────────────────────┘
                     │ GPU-ready buffers (VBO format)
                     ▼
┌──────────────────────────────────────────────────────────┐
│              OpenGL Rendering Layer                       │
│  - OpenGLMapControl (Avalonia UserControl)               │
│  - ShaderManager: GLSL shader compilation/management     │
│  - BufferManager: VBO/VAO management                     │
│  - TextureManager: Texture loading/caching               │
│  - RenderPassManager: Multi-pass rendering               │
└────────────────────┬─────────────────────────────────────┘
                     │ OpenGL Commands
                     ▼
┌──────────────────────────────────────────────────────────┐
│                  GPU (Graphics Card)                      │
│  - Vertex processing (shaders)                           │
│  - Rasterization                                         │
│  - Fragment shading                                      │
│  - Frame buffer output                                   │
└──────────────────────────────────────────────────────────┘
```

### 1.2 Coordinate System

**World Coordinates**: UTM or local field coordinates (meters, Easting/Northing)
- Origin: Field reference point (typically first position recorded)
- Units: Meters
- Axes: +X = East, +Y = North

**View Coordinates**: Camera-transformed world coordinates
- Camera position: (X, Y) in world space
- Camera zoom: Scale factor (1.0 = 1 meter = N pixels)
- Camera rotation: Angle in radians (0 = North up)

**Screen Coordinates**: Pixel coordinates on display
- Origin: Top-left corner
- Units: Pixels
- Axes: +X = Right, +Y = Down

**Transformation Pipeline**:
```
World → View → Projection → Screen
  ↓       ↓         ↓          ↓
(m,m) → Camera → NDC → (px,px)
```

### 1.3 Technology Stack

**Graphics API**: OpenGL 3.3+ (Core Profile)
**Avalonia Integration**: `Avalonia.OpenGL` + custom `OpenGLMapControl`
**Shader Language**: GLSL 330 (OpenGL Shading Language)
**Math Library**: `System.Numerics` for Matrix4x4 operations
**Performance**: GPU-accelerated rendering, VBO batching

---

## 2. Core Components

### 2.1 OpenGLMapControl (Avalonia UserControl)

**Responsibility**: Host OpenGL context within Avalonia UI

**Key Features**:
- Inherits from `Avalonia.OpenGL.Controls.OpenGlControlBase`
- Manages OpenGL context lifecycle (create, render, dispose)
- Handles input events (pointer, touch, mouse wheel)
- Exposes properties for camera control
- Raises events for user interactions (click, drag, zoom)

**API Design**:
```csharp
public class OpenGLMapControl : OpenGlControlBase
{
    // Services (injected)
    private readonly IRenderingCoordinatorService _renderCoordinator;
    private readonly ICameraService _cameraService;

    // Rendering managers
    private ShaderManager _shaderManager;
    private BufferManager _bufferManager;
    private RenderPassManager _renderPassManager;

    // Camera properties (bindable)
    public Vector2 CameraPosition { get; set; }
    public float ZoomLevel { get; set; }
    public float CameraRotation { get; set; }
    public bool FollowVehicle { get; set; }

    // Rendering settings
    public bool ShowBoundaries { get; set; } = true;
    public bool ShowGuidanceLines { get; set; } = true;
    public bool ShowCoverageMap { get; set; } = true;
    public bool ShowSections { get; set; } = true;
    public bool ShowVehicle { get; set; } = true;

    // OpenGL lifecycle
    protected override void OnOpenGlInit(GlInterface gl);
    protected override void OnOpenGlRender(GlInterface gl, int width, int height);
    protected override void OnOpenGlDeinit(GlInterface gl);

    // Input handling
    protected override void OnPointerPressed(PointerPressedEventArgs e);
    protected override void OnPointerMoved(PointerEventArgs e);
    protected override void OnPointerReleased(PointerReleasedEventArgs e);
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e);

    // Camera control methods
    public void PanCamera(Vector2 delta);
    public void ZoomCamera(float delta, Vector2? focusPoint = null);
    public void RotateCamera(float angleDelta);
    public void FitToField();
    public void CenterOnVehicle();
}
```

### 2.2 ShaderManager

**Responsibility**: Load, compile, and manage GLSL shaders

**Key Features**:
- Load shaders from embedded resources or files
- Compile vertex and fragment shaders
- Link shader programs
- Cache compiled programs
- Validate shader compilation and linking
- Set uniform variables (matrices, colors, etc.)

**Shader Programs Needed**:

1. **SolidColorShader**: Flat-colored geometry (boundaries, guidance lines)
   - Uniforms: ModelViewProjection matrix, Color
   - Attributes: Position

2. **TexturedShader**: Textured geometry (vehicle icon, background imagery)
   - Uniforms: ModelViewProjection matrix, Texture sampler
   - Attributes: Position, TexCoords

3. **CoverageShader**: Coverage map triangles with per-vertex colors
   - Uniforms: ModelViewProjection matrix
   - Attributes: Position, Color

4. **SectionShader**: Semi-transparent section overlays
   - Uniforms: ModelViewProjection matrix, SectionColor, Alpha
   - Attributes: Position

**API Design**:
```csharp
public class ShaderManager : IDisposable
{
    private readonly Dictionary<string, ShaderProgram> _programs;

    public ShaderProgram LoadProgram(string name, string vertexShader, string fragmentShader);
    public ShaderProgram GetProgram(string name);
    public void UseProgram(string name);
    public void SetUniform(string name, Matrix4x4 value);
    public void SetUniform(string name, Vector4 value);
    public void SetUniform(string name, float value);
    public void Dispose();
}

public class ShaderProgram : IDisposable
{
    public int ProgramId { get; }
    public int VertexShaderId { get; }
    public int FragmentShaderId { get; }

    public void Use();
    public int GetUniformLocation(string name);
    public int GetAttributeLocation(string name);
    public void Dispose();
}
```

### 2.3 BufferManager

**Responsibility**: Manage GPU buffers (VBOs, VAOs, EBOs)

**Key Features**:
- Create and manage Vertex Buffer Objects (VBOs)
- Create and manage Vertex Array Objects (VAOs)
- Create and manage Element Buffer Objects (EBOs) for indexed rendering
- Support dynamic updates (coverage map, vehicle position)
- Support static buffers (boundaries, guidance lines)
- Efficient batching and instancing

**Buffer Types**:

1. **Static Buffers**: Rarely change (field boundaries, guidance lines)
   - Usage: `GL_STATIC_DRAW`
   - Update frequency: On field load or guidance change

2. **Dynamic Buffers**: Frequently change (vehicle position, section states)
   - Usage: `GL_DYNAMIC_DRAW`
   - Update frequency: Every frame or every position update

3. **Stream Buffers**: Change every frame (coverage map triangles)
   - Usage: `GL_STREAM_DRAW`
   - Update frequency: Every frame

**API Design**:
```csharp
public class BufferManager : IDisposable
{
    public int CreateVertexBuffer(float[] vertices, BufferUsageHint usage);
    public int CreateIndexBuffer(uint[] indices, BufferUsageHint usage);
    public int CreateVertexArray();
    public void BindVertexArray(int vaoId);
    public void UpdateVertexBuffer(int vboId, float[] vertices, int offset = 0);
    public void ConfigureVertexAttribute(int location, int size, VertexAttribPointerType type,
                                          int stride, int offset);
    public void DeleteBuffer(int bufferId);
    public void DeleteVertexArray(int vaoId);
    public void Dispose();
}
```

### 2.4 RenderPassManager

**Responsibility**: Execute multi-pass rendering in correct order

**Key Features**:
- Manage render pass order (back-to-front for transparency)
- Handle depth testing and blending
- Provide clear separation of concerns per pass

**Render Pass Order**:

1. **Background Pass**: Clear color buffer, render background imagery (optional)
2. **Coverage Map Pass**: Render coverage triangles (opaque)
3. **Boundary Pass**: Render field boundaries (lines)
4. **Guidance Pass**: Render AB lines, curves, contours (lines)
5. **Tram Line Pass**: Render tram lines (lines)
6. **Section Overlay Pass**: Render section states (semi-transparent quads)
7. **Vehicle Pass**: Render vehicle icon and implement (textured quad)
8. **UI Overlay Pass**: Render scale bar, compass, etc. (optional)

**API Design**:
```csharp
public class RenderPassManager
{
    private readonly IRenderingCoordinatorService _renderCoordinator;
    private readonly ShaderManager _shaderManager;
    private readonly BufferManager _bufferManager;

    public void RenderFrame(Matrix4x4 viewProjectionMatrix, RenderSettings settings);

    private void RenderBackground();
    private void RenderCoverageMap(Matrix4x4 mvp);
    private void RenderBoundaries(Matrix4x4 mvp);
    private void RenderGuidanceLines(Matrix4x4 mvp);
    private void RenderTramLines(Matrix4x4 mvp);
    private void RenderSectionOverlays(Matrix4x4 mvp);
    private void RenderVehicle(Matrix4x4 mvp);
    private void RenderUIOverlay(Matrix4x4 mvp);
}

public class RenderSettings
{
    public bool ShowBoundaries { get; set; } = true;
    public bool ShowGuidanceLines { get; set; } = true;
    public bool ShowCoverageMap { get; set; } = true;
    public bool ShowSections { get; set; } = true;
    public bool ShowVehicle { get; set; } = true;
    public bool ShowTramLines { get; set; } = true;
}
```

### 2.5 CameraService

**Responsibility**: Manage camera state and transformations

**Key Features**:
- Track camera position (world coordinates)
- Track zoom level (meters per pixel)
- Track rotation (radians)
- Calculate view-projection matrix
- Handle "follow vehicle" mode
- Provide screen-to-world and world-to-screen conversions

**API Design**:
```csharp
public interface ICameraService
{
    // Camera state
    Vector2 Position { get; set; }
    float ZoomLevel { get; set; }  // Meters per pixel
    float Rotation { get; set; }    // Radians

    // View dimensions
    int ViewportWidth { get; set; }
    int ViewportHeight { get; set; }

    // Follow mode
    bool FollowVehicle { get; set; }
    Vector2? VehiclePosition { get; }

    // Transformations
    Matrix4x4 GetViewMatrix();
    Matrix4x4 GetProjectionMatrix();
    Matrix4x4 GetViewProjectionMatrix();

    // Coordinate conversions
    Vector2 ScreenToWorld(Vector2 screenPoint);
    Vector2 WorldToScreen(Vector2 worldPoint);

    // Camera controls
    void Pan(Vector2 delta);
    void Zoom(float factor, Vector2? focusPoint = null);
    void Rotate(float angleDelta);
    void FitBounds(BoundingBox bounds);
    void CenterOn(Vector2 worldPoint);

    // Events
    event EventHandler<CameraChangedEventArgs> CameraChanged;
}
```

### 2.6 RenderingCoordinatorService

**Responsibility**: Aggregate geometry from all services and coordinate rendering

**Key Features**:
- Subscribe to service events (position updates, guidance changes, etc.)
- Maintain current render state
- Generate GPU-ready geometry buffers
- Trigger frame invalidation when data changes
- Batch geometry updates for efficiency

**API Design**:
```csharp
public interface IRenderingCoordinatorService
{
    // Geometry accessors
    VehicleRenderData GetVehicleData();
    BoundaryRenderData GetBoundaryData();
    GuidanceRenderData GetGuidanceData();
    CoverageRenderData GetCoverageData();
    SectionRenderData GetSectionData();
    TramLineRenderData GetTramLineData();

    // State management
    bool IsDirty { get; }
    void MarkClean();
    void InvalidateGeometry(GeometryType type);

    // Events
    event EventHandler GeometryChanged;
}

public class VehicleRenderData
{
    public Vector2 Position { get; set; }
    public float Heading { get; set; }
    public float[] Vertices { get; set; }  // Vehicle shape vertices
    public uint[] Indices { get; set; }
    public Color Color { get; set; }
}

public class BoundaryRenderData
{
    public float[] Vertices { get; set; }  // Line strip vertices
    public Color Color { get; set; }
    public float LineWidth { get; set; }
}

// Similar structures for GuidanceRenderData, CoverageRenderData, etc.
```

---

## 3. Geometry Generation Services

### 3.1 VehicleGeometryService

**Responsibility**: Generate vehicle and implement mesh

**Features**:
- Vehicle body (tractor shape)
- Implement (plow, planter, etc.) based on `ToolConfig`
- Antenna position markers
- Direction indicator (heading arrow)

**Geometry**:
- Vehicle: Rectangle or trapezoid (simplified tractor)
- Implement: Rectangle with section divisions
- Heading: Triangle or arrow shape

**API**:
```csharp
public interface IVehicleGeometryService
{
    float[] GenerateVehicleMesh(VehicleConfiguration config);
    float[] GenerateImplementMesh(ToolConfig toolConfig);
    float[] GenerateHeadingArrow(float length);
}
```

### 3.2 BoundaryGeometryService

**Responsibility**: Convert boundary polygons to GPU-ready line strips

**Features**:
- Field boundary (outer polygon)
- Headland boundaries (inner offsets)
- Boundary visualization (fill vs. outline)

**API**:
```csharp
public interface IBoundaryGeometryService
{
    float[] GenerateBoundaryLines(List<Position> boundaryPoints);
    float[] GenerateBoundaryFill(List<Position> boundaryPoints);  // Triangulated
    float[] GenerateHeadlandLines(List<Position> headlandPoints);
}
```

### 3.3 GuidanceGeometryService

**Responsibility**: Convert guidance lines to GPU-ready geometry

**Features**:
- AB line (infinite line or finite segment)
- Curve line (point list)
- Contour line (point list)
- Cross-track error visualization (perpendicular line to vehicle)

**API**:
```csharp
public interface IGuidanceGeometryService
{
    float[] GenerateABLine(ABLine line, float visibleLength);
    float[] GenerateCurveLine(CurveLine curve);
    float[] GenerateContourLine(ContourLine contour);
    float[] GenerateCrossTrackIndicator(Position vehiclePos, Position closestPoint);
}
```

### 3.4 CoverageGeometryService

**Responsibility**: Convert coverage triangles to GPU mesh

**Features**:
- Batch triangles from `CoverageMapService`
- Color-coded by pass number or time
- Efficient triangle streaming

**API**:
```csharp
public interface ICoverageGeometryService
{
    CoverageMesh GenerateCoverageMesh(IEnumerable<CoverageTriangle> triangles);
}

public class CoverageMesh
{
    public float[] Vertices { get; set; }  // Interleaved: X, Y, R, G, B
    public uint[] Indices { get; set; }
    public int TriangleCount { get; set; }
}
```

### 3.5 SectionGeometryService (Extension)

**Responsibility**: Generate section overlay rectangles for visualization

**Features** (extends existing service from Wave 6B):
- Generate colored rectangles showing section ON/OFF state
- Semi-transparent overlays
- Real-time updates as sections switch

**API** (extends existing):
```csharp
public interface ISectionGeometryService
{
    // Existing methods from Wave 6B...

    // New for Wave 11:
    float[] GenerateSectionOverlayMesh(
        Position vehiclePosition,
        double vehicleHeading,
        SectionConfig sectionConfig,
        ToolConfig toolConfig,
        bool[] sectionStates);
}
```

---

## 4. Shader Implementation

### 4.1 SolidColorShader (Boundaries, Guidance Lines)

**Vertex Shader**:
```glsl
#version 330 core

layout(location = 0) in vec2 aPosition;

uniform mat4 uModelViewProjection;

void main()
{
    gl_Position = uModelViewProjection * vec4(aPosition, 0.0, 1.0);
}
```

**Fragment Shader**:
```glsl
#version 330 core

out vec4 FragColor;

uniform vec4 uColor;

void main()
{
    FragColor = uColor;
}
```

### 4.2 CoverageShader (Coverage Map Triangles)

**Vertex Shader**:
```glsl
#version 330 core

layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec3 aColor;

out vec3 vColor;

uniform mat4 uModelViewProjection;

void main()
{
    gl_Position = uModelViewProjection * vec4(aPosition, 0.0, 1.0);
    vColor = aColor;
}
```

**Fragment Shader**:
```glsl
#version 330 core

in vec3 vColor;
out vec4 FragColor;

void main()
{
    FragColor = vec4(vColor, 1.0);
}
```

### 4.3 VehicleShader (Textured Vehicle Icon)

**Vertex Shader**:
```glsl
#version 330 core

layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aTexCoord;

out vec2 vTexCoord;

uniform mat4 uModelViewProjection;

void main()
{
    gl_Position = uModelViewProjection * vec4(aPosition, 0.0, 1.0);
    vTexCoord = aTexCoord;
}
```

**Fragment Shader**:
```glsl
#version 330 core

in vec2 vTexCoord;
out vec4 FragColor;

uniform sampler2D uTexture;
uniform vec4 uTintColor;

void main()
{
    vec4 texColor = texture(uTexture, vTexCoord);
    FragColor = texColor * uTintColor;
}
```

---

## 5. Camera Control & Input Handling

### 5.1 Mouse/Pointer Input

**Pan**: Click and drag with left button
- Calculate delta in screen coordinates
- Convert to world coordinates based on zoom
- Update camera position

**Zoom**: Mouse wheel
- Calculate zoom factor (e.g., 1.1 per notch)
- Zoom toward cursor position (not center)
- Clamp zoom level (min: 0.1 m/px, max: 100 m/px)

**Rotate**: Right-click and drag (optional)
- Calculate angle delta from drag vector
- Update camera rotation
- Re-render with rotated view

**Select**: Single click
- Convert screen coordinates to world coordinates
- Raycast to find clicked object (boundary point, vehicle, etc.)
- Emit selection event

### 5.2 Touch Input

**Pan**: Single finger drag
- Same as mouse pan

**Zoom**: Pinch gesture
- Calculate distance between two touch points
- Calculate zoom factor from distance change
- Zoom toward pinch center

**Rotate**: Two-finger rotate gesture
- Calculate angle between touch points
- Update camera rotation based on angle change

### 5.3 Keyboard Shortcuts

**Arrow Keys**: Pan camera
- Up/Down: Move camera north/south
- Left/Right: Move camera east/west

**+ / -**: Zoom in/out

**Home**: Center on vehicle

**F**: Fit field to view

**R**: Reset camera rotation to north-up

---

## 6. Performance Optimization

### 6.1 Batching

**Static Geometry Batching**:
- Combine all field boundaries into single VBO
- Combine all guidance lines into single VBO
- Reduce draw calls from N to 1

**Dynamic Geometry Batching**:
- Combine all section overlays into single VBO
- Update only changed sections (partial buffer update)

### 6.2 Level of Detail (LOD)

**Distance-Based LOD**:
- Far objects: Use simplified geometry (fewer vertices)
- Close objects: Use detailed geometry

**Coverage Map LOD**:
- Far zoom: Skip triangles (render every Nth triangle)
- Close zoom: Render all triangles

### 6.3 Frustum Culling

**Cull Off-Screen Objects**:
- Calculate view frustum (visible area)
- Skip rendering objects outside frustum
- Reduces GPU load for large fields

### 6.4 Dirty Flags

**Only Re-render When Needed**:
- Track which geometry has changed
- Only regenerate changed geometry
- Reuse cached VBOs when possible

### 6.5 Instancing

**Repeated Objects** (section markers, waypoint icons):
- Use instanced rendering
- Single draw call for multiple instances
- Provide per-instance data (position, color)

---

## 7. Integration with Services

### 7.1 Position Updates (Wave 1)

**Service**: `IPositionUpdateService`

**Integration**:
```csharp
_positionService.PositionChanged += (s, e) =>
{
    var vehicleData = _vehicleGeometryService.GenerateVehicleMesh(_vehicleConfig);
    _renderCoordinator.InvalidateGeometry(GeometryType.Vehicle);

    if (_cameraService.FollowVehicle)
    {
        _cameraService.CenterOn(new Vector2((float)e.Position.Easting,
                                             (float)e.Position.Northing));
    }

    _openGLMapControl.RequestNextFramePresentation();
};
```

### 7.2 Guidance Lines (Wave 2)

**Services**: `IABLineService`, `ICurveLineService`, `IContourLineService`

**Integration**:
```csharp
_abLineService.ABLineChanged += (s, e) =>
{
    var guidanceData = _guidanceGeometryService.GenerateABLine(e.ABLine, 1000.0f);
    _renderCoordinator.InvalidateGeometry(GeometryType.Guidance);
    _openGLMapControl.RequestNextFramePresentation();
};
```

### 7.3 Section States (Wave 4)

**Service**: `ISectionControlService`

**Integration**:
```csharp
_sectionControlService.SectionStateChanged += (s, e) =>
{
    var sectionData = _sectionGeometryService.GenerateSectionOverlayMesh(
        _vehiclePosition, _vehicleHeading, _sectionConfig, _toolConfig,
        _sectionControlService.GetAllSectionStates());
    _renderCoordinator.InvalidateGeometry(GeometryType.Sections);
    _openGLMapControl.RequestNextFramePresentation();
};
```

### 7.4 Coverage Map (Wave 6B)

**Service**: `ICoverageMapService`

**Integration**:
```csharp
_coverageMapService.CoverageAdded += (s, e) =>
{
    var coverageMesh = _coverageGeometryService.GenerateCoverageMesh(
        _coverageMapService.GetAllTriangles());
    _renderCoordinator.InvalidateGeometry(GeometryType.Coverage);
    _openGLMapControl.RequestNextFramePresentation();
};
```

---

## 8. Testing Strategy

### 8.1 Unit Tests

**ShaderManager Tests**:
- Shader compilation (valid shaders)
- Shader compilation errors (invalid shaders)
- Program linking
- Uniform setting

**BufferManager Tests**:
- VBO creation and updates
- VAO configuration
- Buffer deletion

**CameraService Tests**:
- View-projection matrix calculation
- Screen-to-world conversion
- World-to-screen conversion
- Pan, zoom, rotate operations

**Geometry Services Tests**:
- Vehicle mesh generation
- Boundary line generation
- Guidance line generation
- Coverage mesh generation

### 8.2 Integration Tests

**Rendering Pipeline Tests**:
- Full frame render without errors
- Geometry updates trigger re-render
- Camera changes trigger re-render

**Service Integration Tests**:
- Position update → vehicle position updated on screen
- Guidance line change → guidance line updated on screen
- Section state change → section overlay updated on screen

### 8.3 Performance Tests

**Frame Rate Tests**:
- Measure FPS with 1,000 triangles
- Measure FPS with 10,000 triangles
- Measure FPS with 100,000 triangles

**Latency Tests**:
- Position update to screen update latency
- Camera control latency
- Zoom/pan responsiveness

### 8.4 Visual Regression Tests

**Screenshot Comparison**:
- Render known scene
- Compare screenshot to golden master
- Detect unintended rendering changes

---

## 9. Implementation Plan

### 9.1 Task Group 1: Foundation (OpenGL Setup)

**Effort**: 2-3 days

**Tasks**:
1. Create `OpenGLMapControl` (Avalonia UserControl with OpenGL context)
2. Implement `ShaderManager` (load, compile, link shaders)
3. Implement `BufferManager` (VBO, VAO, EBO management)
4. Create basic shaders (SolidColorShader, CoverageShader)
5. Render simple test geometry (triangle, quad)

**Deliverables**:
- `OpenGLMapControl.cs` + unit tests
- `ShaderManager.cs` + unit tests
- `BufferManager.cs` + unit tests
- Basic shader GLSL files
- Test application showing colored triangle

**Validation**:
- OpenGL context created successfully
- Shaders compile without errors
- Test geometry renders on screen

---

### 9.2 Task Group 2: Camera System

**Effort**: 2 days

**Tasks**:
1. Implement `CameraService` (position, zoom, rotation)
2. Calculate view-projection matrix
3. Implement coordinate transformations (screen ↔ world)
4. Add camera controls (pan, zoom, rotate)
5. Implement "follow vehicle" mode

**Deliverables**:
- `CameraService.cs` + unit tests
- Matrix calculation tests
- Coordinate conversion tests

**Validation**:
- Camera can pan, zoom, rotate
- Screen-to-world conversion is accurate
- Follow vehicle mode works

---

### 9.3 Task Group 3: Geometry Services

**Effort**: 3-4 days

**Tasks**:
1. Implement `VehicleGeometryService` (vehicle + implement mesh)
2. Implement `BoundaryGeometryService` (boundary lines + fill)
3. Implement `GuidanceGeometryService` (AB, curve, contour lines)
4. Implement `CoverageGeometryService` (triangle mesh from coverage)
5. Extend `SectionGeometryService` (section overlay rectangles)

**Deliverables**:
- 5 geometry service implementations + unit tests
- Geometry generation validated with test data

**Validation**:
- Vehicle mesh has correct shape
- Boundary lines follow boundary points
- Guidance lines render correctly
- Coverage triangles match input data

---

### 9.4 Task Group 4: Rendering Coordinator & Pipeline

**Effort**: 3-4 days

**Tasks**:
1. Implement `RenderingCoordinatorService` (aggregate geometry from services)
2. Implement `RenderPassManager` (multi-pass rendering)
3. Integrate with Wave 1-8 services (position, guidance, section, coverage)
4. Implement render passes (background, coverage, boundaries, guidance, vehicle, etc.)
5. Add dirty flag tracking and frame invalidation

**Deliverables**:
- `RenderingCoordinatorService.cs` + tests
- `RenderPassManager.cs` + tests
- Service event subscriptions
- Full rendering pipeline

**Validation**:
- All geometry renders in correct order
- Service updates trigger re-render
- Frame rate ≥60 FPS with typical field data

---

### 9.5 Task Group 5: Input Handling

**Effort**: 2 days

**Tasks**:
1. Implement mouse input (pan, zoom, select)
2. Implement touch input (pan, pinch-zoom, rotate)
3. Implement keyboard shortcuts
4. Add input event handlers to `OpenGLMapControl`

**Deliverables**:
- Input handling code in `OpenGLMapControl`
- Touch gesture recognition
- Keyboard shortcut handlers

**Validation**:
- Mouse pan/zoom works
- Touch gestures work on touch screen
- Keyboard shortcuts work

---

### 9.6 Task Group 6: Performance Optimization

**Effort**: 2-3 days

**Tasks**:
1. Implement geometry batching (static and dynamic)
2. Add frustum culling
3. Implement LOD for coverage map
4. Add dirty flag optimizations
5. Profile and optimize hot paths

**Deliverables**:
- Batching implementation
- Frustum culling implementation
- LOD system
- Performance benchmarks

**Validation**:
- Frame rate ≥60 FPS with 10,000+ triangles
- No unnecessary re-renders
- Smooth pan/zoom with large fields

---

### 9.7 Task Group 7: Polish & Documentation

**Effort**: 1-2 days

**Tasks**:
1. Add anti-aliasing (MSAA or FXAA)
2. Add visual polish (smooth camera transitions, etc.)
3. Write developer documentation
4. Create user guide (camera controls, etc.)
5. Record demo video

**Deliverables**:
- Anti-aliasing implementation
- Polish features (animations, transitions)
- Documentation
- Demo video

**Validation**:
- Rendering looks smooth and professional
- Documentation is clear and complete

---

## 10. Success Criteria

### 10.1 Functional Requirements

✅ **OpenGL context created successfully on Windows, Linux, macOS**
✅ **Vehicle renders at correct position with correct heading**
✅ **Field boundaries render correctly**
✅ **Guidance lines (AB, Curve, Contour) render correctly**
✅ **Coverage map triangles render with correct colors**
✅ **Section overlays show current section states**
✅ **Camera can pan, zoom, rotate via mouse/keyboard/touch**
✅ **Follow vehicle mode works (camera tracks vehicle)**
✅ **Service updates trigger immediate re-render**

### 10.2 Performance Requirements

✅ **Frame rate ≥60 FPS under normal load (1,000-10,000 triangles)**
✅ **Position update latency <16ms (one frame)**
✅ **Camera update latency <16ms**
✅ **Field load time <500ms for 100 hectare field**
✅ **No frame drops during vehicle movement**
✅ **Smooth pan/zoom/rotate with no stuttering**

### 10.3 Quality Requirements

✅ **No OpenGL errors during rendering**
✅ **No memory leaks (proper disposal of GPU resources)**
✅ **Clean shader compilation (no warnings)**
✅ **Accurate coordinate transformations (screen ↔ world)**
✅ **Correct depth ordering (no Z-fighting)**
✅ **Proper transparency rendering (alpha blending)**

---

## 11. Risk Mitigation

### 11.1 Known Risks

**Risk 1: Cross-Platform OpenGL Compatibility**
- **Mitigation**: Use OpenGL 3.3 Core Profile (widely supported)
- **Verification**: Test on Windows, Linux, macOS
- **Fallback**: Provide OpenGL 2.1 fallback shaders if needed

**Risk 2: Performance on Low-End Hardware**
- **Mitigation**: Implement LOD and frustum culling
- **Verification**: Test on integrated GPU
- **Fallback**: Reduce triangle count, disable anti-aliasing

**Risk 3: Avalonia OpenGL Integration Issues**
- **Mitigation**: Use `Avalonia.OpenGL` official package
- **Verification**: Test Avalonia OpenGL samples first
- **Fallback**: Create custom OpenGL hosting control if needed

**Risk 4: Coordinate Transformation Bugs**
- **Mitigation**: Extensive unit tests for transformations
- **Verification**: Visual inspection with known test data
- **Fallback**: Manual calculation verification

---

## 12. Dependencies

### 12.1 Backend Services (Waves 1-8)

**Wave 1**: `IPositionUpdateService` (vehicle position)
**Wave 2**: `IABLineService`, `ICurveLineService`, `IContourLineService` (guidance)
**Wave 3**: Not directly used (steering is internal)
**Wave 4**: `ISectionControlService` (section states)
**Wave 5**: `IBoundaryManagementService`, `ITramLineService` (boundaries, tram lines)
**Wave 6B**: `ICoverageMapService` (coverage triangles), `ISectionGeometryService` (section geometry)
**Wave 7**: Not directly used (formatting is for UI text)
**Wave 8**: Not directly used (configuration is loaded at startup)

### 12.2 UI Dependencies

**Wave 9-10**: `MainWindow.axaml` (hosts `OpenGLMapControl` in CENTER panel)
**Wave 10**: Camera control UI (buttons for zoom, pan, follow, etc.)

### 12.3 External Libraries

**Avalonia.OpenGL**: OpenGL integration for Avalonia
**System.Numerics**: Matrix and vector math
**OpenTK** (optional): OpenGL bindings if Avalonia.OpenGL is insufficient

---

## 13. Future Enhancements (Post-Wave 11)

### 13.1 3D Terrain Rendering (Wave 12)
- Elevation-based terrain rendering
- 3D vehicle model
- Perspective projection

### 13.2 Advanced Visualizations (Wave 12)
- Heatmaps (speed, coverage overlap, etc.)
- Real-time telemetry charts overlaid on map
- Historical path playback

### 13.3 Background Imagery (Wave 12)
- Satellite imagery tiles (Google Maps, Bing, etc.)
- Custom georeferenced images
- Image caching and pre-loading

### 13.4 Multi-Vehicle Support (Wave 13)
- Render multiple vehicles on same field
- Vehicle-to-vehicle guidance visualization
- Fleet management view

---

## 14. References

**Legacy AgOpenGPS Rendering**:
- `SourceCode/GPS/Forms/FormGPS_Draw.cs` - Main rendering loop
- `SourceCode/GPS/OpenGL/` - OpenGL utilities and shaders
- `SourceCode/GPS/Classes/CTxtureList.cs` - Texture management

**Avalonia OpenGL**:
- https://docs.avaloniaui.net/docs/next/concepts/graphics/opengl
- Avalonia OpenGL samples: https://github.com/AvaloniaUI/Avalonia/tree/master/samples/OpenGLDemo

**OpenGL Resources**:
- LearnOpenGL: https://learnopengl.com/
- OpenGL 3.3 Core Profile Reference: https://www.khronos.org/opengl/wiki/Core_Language_(GLSL)

**Service Migration Plan**:
- `SERVICE_MIGRATION_PLAN.md` (100% complete - all services implemented)

**Wave Progress**:
- Wave 1-8: Backend services (COMPLETE)
- Wave 9: Simple forms UI (COMPLETE)
- Wave 10: Moderate forms UI (COMPLETE)
- Wave 10.5: Panel docking system (COMPLETE)
- Wave 11: OpenGL rendering engine (THIS SPEC)

---

**End of Specification**
