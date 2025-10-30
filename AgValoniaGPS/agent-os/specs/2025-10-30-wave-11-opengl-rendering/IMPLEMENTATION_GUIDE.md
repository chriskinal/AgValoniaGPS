# Wave 11 OpenGL Rendering Engine - Implementation Guide

**Date**: 2025-10-30
**Version**: 1.0
**Target Audience**: Developers working with AgValoniaGPS rendering system

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Service Integration](#service-integration)
3. [Shader System](#shader-system)
4. [Coordinate System](#coordinate-system)
5. [Adding New Geometry Types](#adding-new-geometry-types)
6. [Performance Optimization](#performance-optimization)
7. [Troubleshooting](#troubleshooting)

---

## Architecture Overview

### System Components

The OpenGL rendering engine consists of several key layers:

```
┌─────────────────────────────────────────────────────────┐
│                  OpenGLMapControl                        │
│  (Avalonia UserControl - UI Entry Point)                │
│  - Hosts OpenGL context                                 │
│  - Handles user input (mouse, touch, keyboard)          │
│  - Manages render loop (60 FPS)                         │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────┐
│              RenderingCoordinatorService                 │
│  - Aggregates geometry from backend services            │
│  - Manages dirty flags for efficient updates            │
│  - Provides GPU-ready render data                       │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────┐
│              Geometry Generation Services                │
│  - VehicleGeometryService                               │
│  - BoundaryGeometryService                              │
│  - GuidanceGeometryService                              │
│  - CoverageGeometryService (with LOD support)           │
│  - SectionGeometryService                               │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────┐
│                   CameraService                          │
│  - Manages camera position, zoom, rotation              │
│  - Calculates view-projection matrices                  │
│  - Provides coordinate transformations                  │
│  - Supports frustum culling                             │
└─────────────────────────────────────────────────────────┘
```

### Key Design Patterns

1. **Service Layer Pattern**: Business logic separated from rendering
2. **Dirty Flag Optimization**: Only regenerate changed geometry
3. **Level of Detail (LOD)**: Reduce triangle count at far zoom levels
4. **Frustum Culling**: Skip rendering off-screen objects

---

## Service Integration

### Subscribing to Backend Services

The `RenderingCoordinatorService` subscribes to events from backend services:

```csharp
// Position updates (Wave 1)
_positionService.PositionUpdated += OnPositionUpdated;

// Guidance line changes (Wave 2)
_abLineService.ABLineChanged += OnABLineChanged;
_curveLineService.CurveChanged += OnCurveLineChanged;
_contourService.StateChanged += OnContourStateChanged;

// Section state changes (Wave 4)
_sectionControlService.SectionStateChanged += OnSectionStateChanged;

// Coverage map updates (Wave 6B)
_coverageMapService.CoverageUpdated += OnCoverageMapUpdated;
```

### Event Flow

1. **Backend service** updates its state
2. **Service event** is raised
3. **RenderingCoordinatorService** receives event
4. **Geometry is marked dirty** via `MarkGeometryDirty(GeometryType type)`
5. **GeometryChanged event** is raised
6. **OpenGLMapControl** requests next frame
7. **Render method** calls `GetXxxData()` which regenerates if dirty

### Adding a New Service Integration

To integrate a new backend service:

1. **Add service field** to RenderingCoordinatorService
2. **Subscribe to events** in `SubscribeToServiceEvents()`
3. **Create event handler** that marks geometry dirty
4. **Implement regeneration method** (e.g., `RegenerateMyGeometry()`)
5. **Add public accessor** (e.g., `GetMyRenderData()`)

Example:

```csharp
// 1. Add field
private readonly IMyNewService _myNewService;
private MyRenderData? _myData;

// 2. Subscribe in constructor
_myNewService.DataChanged += OnMyDataChanged;

// 3. Event handler
private void OnMyDataChanged(object? sender, EventArgs e)
{
    RegenerateMyGeometry();
    MarkGeometryDirty(GeometryType.MyGeometry);
}

// 4. Regeneration method
private void RegenerateMyGeometry()
{
    try
    {
        var data = _myNewService.GetData();
        if (data == null)
        {
            _myData = null;
            _dirtyGeometry.Remove(GeometryType.MyGeometry);
            return;
        }

        // Generate geometry...
        _myData = new MyRenderData { /* ... */ };

        // Clear dirty flag
        _dirtyGeometry.Remove(GeometryType.MyGeometry);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error regenerating my geometry: {ex.Message}");
        _myData = null;
    }
}

// 5. Public accessor
public MyRenderData? GetMyRenderData()
{
    if (_dirtyGeometry.Contains(GeometryType.MyGeometry))
    {
        RegenerateMyGeometry();
    }
    return _myData;
}
```

---

## Shader System

### Available Shaders

The rendering engine uses GLSL 300 ES shaders for cross-platform compatibility:

#### 1. SolidColorShader (Basic geometry)

**Use Case**: Field boundaries, guidance lines, grid

**Vertex Shader Attributes**:
- `layout(location = 0) in vec2 aPosition` - 2D position (X, Y)
- `layout(location = 1) in vec4 aColor` - RGBA color

**Uniforms**:
- `uniform mat4 uMVP` - Model-View-Projection matrix

**Fragment Shader**:
- Outputs per-vertex color

#### 2. TextureShader (Textured objects)

**Use Case**: Vehicle icon, background imagery

**Vertex Shader Attributes**:
- `layout(location = 0) in vec2 aPosition` - 2D position
- `layout(location = 1) in vec2 aTexCoord` - Texture coordinates

**Uniforms**:
- `uniform mat4 uTransform` - Transformation matrix
- `uniform sampler2D uTexture` - Texture sampler

### Creating Custom Shaders

To add a new shader:

1. **Write GLSL shader code** (vertex + fragment)
2. **Compile and link** in `InitializeShaders()` or similar method
3. **Store program ID** in field (e.g., `_myShaderProgram`)
4. **Use in render method**:

```csharp
_gl.UseProgram(_myShaderProgram);

// Set uniforms
int mvpLocation = _gl.GetUniformLocation(_myShaderProgram, "uMVP");
unsafe
{
    fixed (float* m = mvpMatrix)
    {
        _gl.UniformMatrix4(mvpLocation, 1, false, m);
    }
}

// Bind VAO and draw
_gl.BindVertexArray(_myVao);
_gl.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);
```

### Shader Best Practices

- **Always check compilation status**: Use `GetShader()` with `CompileStatus`
- **Log errors**: Call `GetShaderInfoLog()` on failure
- **Clean up**: Delete shader objects after linking
- **Use uniforms efficiently**: Set uniforms before draw calls, not per-vertex
- **Match attribute locations**: Ensure vertex data layout matches shader expectations

---

## Coordinate System

### Coordinate Spaces

The rendering pipeline transforms coordinates through multiple spaces:

```
World Space (meters)
    ↓ View Matrix
Camera Space (relative to camera)
    ↓ Projection Matrix
Clip Space (homogeneous coordinates)
    ↓ Perspective Divide
NDC (Normalized Device Coordinates: -1 to +1)
    ↓ Viewport Transform
Screen Space (pixels)
```

### World Coordinates

- **Origin**: Field reference point (typically first GPS position)
- **Units**: Meters
- **Axes**: +X = East, +Y = North
- **Range**: Unbounded (depends on field size)

### Screen Coordinates

- **Origin**: Top-left corner (0, 0)
- **Units**: Pixels
- **Axes**: +X = Right, +Y = Down
- **Range**: (0, 0) to (ViewportWidth, ViewportHeight)

### Coordinate Conversions

#### Screen to World

```csharp
var worldPos = _cameraService.ScreenToWorld(new Vector2(screenX, screenY));
```

**Use Cases**:
- Converting mouse click position to world position
- Raycasting for object selection
- Touch gesture handling

#### World to Screen

```csharp
var screenPos = _cameraService.WorldToScreen(new Vector2(worldX, worldY));
```

**Use Cases**:
- Positioning UI overlays at world points
- Checking if world point is visible on screen
- Calculating screen distances

### Camera Transformations

The camera uses two matrices:

1. **View Matrix**: Transforms world → camera space
2. **Projection Matrix**: Transforms camera space → clip space

Combined as **View-Projection Matrix** (MVP):

```csharp
Matrix4x4 mvp = _cameraService.GetViewProjectionMatrix();
```

---

## Adding New Geometry Types

### Step-by-Step Guide

#### 1. Create Render Data Structure

```csharp
public class MyRenderData
{
    public float[] Vertices { get; set; }
    public uint[]? Indices { get; set; }
    public Vector3 Color { get; set; }
    public float LineWidth { get; set; }
}
```

#### 2. Create Geometry Service

```csharp
public interface IMyGeometryService
{
    float[] GenerateMyGeometry(MyInputData input);
}

public class MyGeometryService : IMyGeometryService
{
    public float[] GenerateMyGeometry(MyInputData input)
    {
        var vertices = new List<float>();

        // Generate vertices (interleaved: X, Y, R, G, B, A)
        foreach (var point in input.Points)
        {
            vertices.Add((float)point.X);    // Position X
            vertices.Add((float)point.Y);    // Position Y
            vertices.Add(1.0f);               // Color R
            vertices.Add(0.0f);               // Color G
            vertices.Add(0.0f);               // Color B
            vertices.Add(1.0f);               // Color A
        }

        return vertices.ToArray();
    }
}
```

#### 3. Register Service

In `ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<IMyGeometryService, MyGeometryService>();
```

#### 4. Integrate with RenderingCoordinatorService

Add to `RenderingCoordinatorService`:

```csharp
private readonly IMyGeometryService _myGeometryService;
private MyRenderData? _myData;

// Constructor injection
public RenderingCoordinatorService(
    // ... existing services ...
    IMyGeometryService myGeometryService)
{
    _myGeometryService = myGeometryService;
    // ...
}

// Public accessor
public MyRenderData? GetMyRenderData()
{
    if (_dirtyGeometry.Contains(GeometryType.MyGeometry))
    {
        RegenerateMyGeometry();
    }
    return _myData;
}
```

#### 5. Render in OpenGLMapControl

Add rendering logic in `OnOpenGlRender()`:

```csharp
// Get render data
var myData = _renderingCoordinator.GetMyRenderData();
if (myData != null && myData.Vertices.Length > 0)
{
    // Create/update VBO if needed
    if (_myVbo == 0)
    {
        _myVao = _gl.GenVertexArray();
        _gl.BindVertexArray(_myVao);

        _myVbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _myVbo);

        // ... configure vertex attributes ...
    }

    // Update VBO data
    _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _myVbo);
    unsafe
    {
        fixed (float* v = myData.Vertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer,
                (nuint)(myData.Vertices.Length * sizeof(float)),
                v, BufferUsageARB.DynamicDraw);
        }
    }

    // Draw
    _gl.UseProgram(_shaderProgram);
    // Set MVP uniform...
    _gl.BindVertexArray(_myVao);
    _gl.DrawArrays(PrimitiveType.LineStrip, 0, vertexCount);
}
```

---

## Performance Optimization

### 1. Dirty Flag System

**How It Works**:
- Each geometry type has a dirty flag
- Regeneration only happens when dirty
- Flags cleared after regeneration

**Benefits**:
- Avoids unnecessary CPU work
- Reduces memory allocations
- Maintains 60 FPS with frequent updates

**Usage**:

```csharp
// Mark geometry dirty when data changes
MarkGeometryDirty(GeometryType.MyGeometry);

// Check if dirty before regenerating
if (_dirtyGeometry.Contains(GeometryType.MyGeometry))
{
    RegenerateMyGeometry();
}
```

### 2. Frustum Culling

**How It Works**:
- Calculate visible bounds from camera
- Skip rendering objects outside bounds

**Implementation**:

```csharp
var visibleBounds = _cameraService.GetVisibleBounds();

foreach (var obj in objects)
{
    if (IsInView(obj.Position, visibleBounds))
    {
        RenderObject(obj);
    }
}

private bool IsInView(Position point, BoundingBox visibleBounds)
{
    return point.Easting >= visibleBounds.MinX &&
           point.Easting <= visibleBounds.MaxX &&
           point.Northing >= visibleBounds.MinY &&
           point.Northing <= visibleBounds.MaxY;
}
```

### 3. Level of Detail (LOD)

**How It Works**:
- Reduce triangle count based on zoom level
- Far objects use simplified geometry
- Close objects use full detail

**Example: Coverage Map LOD**

```csharp
// In CoverageGeometryService
private int CalculateLODSkipFactor(float zoomLevel)
{
    if (zoomLevel < 0.5f)
        return 1;   // Render all triangles
    else if (zoomLevel < 2.0f)
        return 2;   // Render every 2nd triangle
    else if (zoomLevel < 10.0f)
        return 4;   // Render every 4th triangle
    else
        return 8;   // Render every 8th triangle
}
```

### 4. Geometry Batching

**Concept**: Combine multiple objects into single VBO to reduce draw calls

**Implementation**:

```csharp
// Instead of:
foreach (var boundary in boundaries)
{
    DrawBoundary(boundary);  // Multiple draw calls
}

// Do this:
var allBoundaries = new List<float>();
foreach (var boundary in boundaries)
{
    allBoundaries.AddRange(GenerateBoundaryVertices(boundary));
}
DrawBatch(allBoundaries);  // Single draw call
```

### 5. Buffer Usage Hints

Choose appropriate buffer usage:

- **Static** (`GL_STATIC_DRAW`): Field boundaries, guidance lines (rarely change)
- **Dynamic** (`GL_DYNAMIC_DRAW`): Vehicle position, section states (change frequently)
- **Stream** (`GL_STREAM_DRAW`): Coverage map (changes every frame)

---

## Troubleshooting

### Common Issues

#### 1. Nothing Renders / Black Screen

**Possible Causes**:
- OpenGL context not initialized
- Shader compilation failed
- Camera position far from geometry
- View-projection matrix incorrect

**Solutions**:
- Check console for OpenGL initialization errors
- Verify shader compilation logs
- Reset camera to origin: `_cameraService.CenterOn(Vector2.Zero)`
- Inspect MVP matrix values (should not be all zeros)

#### 2. Geometry Renders Incorrectly

**Possible Causes**:
- Vertex attribute layout mismatch
- Incorrect stride/offset in `VertexAttribPointer`
- Wrong primitive type (e.g., `Triangles` vs `TriangleFan`)

**Solutions**:
- Verify vertex format matches shader expectations
- Check stride calculation: `stride = floats_per_vertex * sizeof(float)`
- Use `GL_DEBUG_OUTPUT` to catch errors

#### 3. Poor Performance / Low FPS

**Possible Causes**:
- Too many draw calls
- No LOD on coverage map
- Dirty flags not working
- Unnecessary geometry regeneration

**Solutions**:
- Batch geometry to reduce draw calls
- Enable LOD for coverage: pass zoom level to `GenerateCoverageMesh()`
- Verify dirty flags are cleared after regeneration
- Profile with `System.Diagnostics.Stopwatch`

#### 4. Input Not Working

**Possible Causes**:
- Control not focused
- Event handlers not attached
- `e.Handled` not set

**Solutions**:
- Set `Focusable = true` in control constructor
- Verify event subscriptions in constructor
- Ensure `e.Handled = true` in event handlers

#### 5. Shader Compilation Errors

**Common Errors**:
- `#version` directive mismatch
- Undefined uniform/attribute
- Type mismatch (e.g., `vec2` vs `vec3`)

**Solutions**:
- Use `#version 300 es` for compatibility
- Check uniform names match between C# and GLSL
- Verify attribute locations: `layout(location = X)`

---

## Debugging Tips

### 1. Enable OpenGL Debug Output

```csharp
_gl.Enable(EnableCap.DebugOutput);
_gl.DebugMessageCallback((source, type, id, severity, length, message, userParam) =>
{
    Console.WriteLine($"GL CALLBACK: {message}");
}, IntPtr.Zero);
```

### 2. Log Rendering State

```csharp
Console.WriteLine($"Camera: Pos={_cameraService.Position}, Zoom={_cameraService.ZoomLevel}");
Console.WriteLine($"Visible Bounds: {visibleBounds.MinX}-{visibleBounds.MaxX}, {visibleBounds.MinY}-{visibleBounds.MaxY}");
Console.WriteLine($"Triangles Rendered: {triangleCount}");
```

### 3. Visualize Bounding Boxes

```csharp
// Draw visible bounds as debug overlay
DrawDebugRect(visibleBounds.MinX, visibleBounds.MinY,
              visibleBounds.Width, visibleBounds.Height,
              Color.Red);
```

### 4. Profile Performance

```csharp
var sw = Stopwatch.StartNew();
RegenerateGeometry();
sw.Stop();
Console.WriteLine($"Geometry regeneration: {sw.ElapsedMilliseconds}ms");
```

---

## Best Practices

### 1. Memory Management

- **Dispose GPU resources**: Delete VBOs, VAOs, textures in `OnOpenGlDeinit()`
- **Avoid allocations in render loop**: Pre-allocate buffers
- **Use object pooling**: For frequently created/destroyed objects

### 2. Thread Safety

- **UI thread only**: All OpenGL calls must be on UI thread
- **Service events**: May fire on background threads, marshal to UI thread if needed

### 3. Error Handling

- **Never crash rendering**: Catch exceptions in render methods
- **Log errors**: Use `Console.WriteLine()` or logger
- **Fail gracefully**: Render placeholder geometry on error

### 4. Testing

- **Unit test geometry services**: Verify vertex generation
- **Integration test services**: Verify event flow
- **Visual regression test**: Compare screenshots to golden masters

---

## Further Reading

- [Avalonia OpenGL Documentation](https://docs.avaloniaui.net/docs/next/concepts/graphics/opengl)
- [LearnOpenGL Tutorial](https://learnopengl.com/)
- [OpenGL ES 3.0 Reference](https://www.khronos.org/opengles/sdk/docs/man3/)
- [AgValoniaGPS Wave 11 Specification](./spec.md)

---

**End of Implementation Guide**
