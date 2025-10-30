# Wave 11: OpenGL Rendering Engine - Task Breakdown

**Date**: 2025-10-30
**Total Estimated Effort**: 14-18 days
**Parallelizable**: Yes (7 task groups can run in parallel)

---

## Task Group 1: Foundation (OpenGL Setup)

**Estimated Effort**: 2-3 days
**Dependencies**: None
**Can Start**: Immediately

### Tasks:

#### 1.1: OpenGLMapControl Base Implementation
**Effort**: 4-6 hours
**Files**:
- `AgValoniaGPS.Desktop/Views/Controls/OpenGLMapControl.cs`
- `AgValoniaGPS.Desktop/Views/Controls/OpenGLMapControl.axaml`

**Requirements**:
- Inherit from `Avalonia.OpenGL.Controls.OpenGlControlBase`
- Implement `OnOpenGlInit`, `OnOpenGlRender`, `OnOpenGlDeinit`
- Set up OpenGL context with proper settings (depth buffer, MSAA)
- Create basic render loop (clear color, swap buffers)
- Add bindable properties for camera settings

**Tests**:
- OpenGL context created successfully
- Render loop executes without errors
- Control renders blank canvas

**Acceptance**:
- Control displays in Avalonia window
- OpenGL version logged (3.3+)
- No OpenGL errors on startup

---

#### 1.2: ShaderManager Implementation
**Effort**: 4-6 hours
**Files**:
- `AgValoniaGPS.Services/Rendering/ShaderManager.cs`
- `AgValoniaGPS.Services/Rendering/ShaderProgram.cs`
- `AgValoniaGPS.Services.Tests/Rendering/ShaderManagerTests.cs`

**Requirements**:
- Load shaders from embedded resources or strings
- Compile vertex and fragment shaders
- Link shader programs
- Cache compiled programs (dictionary by name)
- Validate compilation and linking (throw exceptions on errors)
- Provide methods to set uniforms (Matrix4x4, Vector4, float, int)
- Implement `IDisposable` for cleanup

**Tests**:
- Load and compile valid shader
- Detect and throw on shader compilation error
- Set uniform values correctly
- Program caching works
- Disposal cleans up GPU resources

**Acceptance**:
- Can compile simple test shader
- Uniform setting verified
- 100% test coverage

---

#### 1.3: BufferManager Implementation
**Effort**: 4-6 hours
**Files**:
- `AgValoniaGPS.Services/Rendering/BufferManager.cs`
- `AgValoniaGPS.Services.Tests/Rendering/BufferManagerTests.cs`

**Requirements**:
- Create VBOs (Vertex Buffer Objects) with usage hints
- Create VAOs (Vertex Array Objects)
- Create EBOs (Element Buffer Objects) for indexed rendering
- Update buffer data (full and partial updates)
- Configure vertex attributes (position, color, tex coords)
- Implement `IDisposable` for cleanup

**Tests**:
- Create VBO with test data
- Update VBO with new data
- Create VAO and configure attributes
- Create EBO for indexed rendering
- Disposal cleans up GPU resources

**Acceptance**:
- Buffers created successfully
- Data uploaded to GPU
- Vertex attributes configured correctly
- 100% test coverage

---

#### 1.4: Basic Shader Creation
**Effort**: 2-3 hours
**Files**:
- `AgValoniaGPS.Desktop/Shaders/SolidColor.vert`
- `AgValoniaGPS.Desktop/Shaders/SolidColor.frag`
- `AgValoniaGPS.Desktop/Shaders/Coverage.vert`
- `AgValoniaGPS.Desktop/Shaders/Coverage.frag`

**Requirements**:
- Create GLSL 330 vertex and fragment shaders
- SolidColorShader: Flat-colored geometry (boundaries, guidance)
- CoverageShader: Per-vertex colored geometry (coverage triangles)
- Both shaders use ModelViewProjection matrix uniform

**Tests**:
- Manual testing: Shaders compile without errors
- Visual test: Render colored triangle and quad

**Acceptance**:
- Shaders compile in ShaderManager
- Test geometry renders with correct colors

---

#### 1.5: Test Geometry Rendering
**Effort**: 2-3 hours
**Files**:
- `AgValoniaGPS.Desktop/Views/Controls/OpenGLMapControl.cs` (test code)

**Requirements**:
- Create test triangle vertices
- Upload to VBO
- Render using SolidColorShader
- Verify triangle displays on screen

**Tests**:
- Manual visual test: Triangle visible
- Triangle color matches expected

**Acceptance**:
- Colored triangle renders on screen
- No OpenGL errors
- Frame rate logged (should be ≥60 FPS)

---

**Task Group 1 Deliverables**:
- ✅ OpenGLMapControl with working OpenGL context
- ✅ ShaderManager with 100% test coverage
- ✅ BufferManager with 100% test coverage
- ✅ Basic shaders (SolidColor, Coverage)
- ✅ Test triangle rendering successfully

---

## Task Group 2: Camera System

**Estimated Effort**: 2 days
**Dependencies**: Task Group 1 (ShaderManager, BufferManager)
**Can Start**: After Task Group 1

### Tasks:

#### 2.1: CameraService Implementation
**Effort**: 4-6 hours
**Files**:
- `AgValoniaGPS.Services/Rendering/CameraService.cs`
- `AgValoniaGPS.Services/Rendering/ICameraService.cs`
- `AgValoniaGPS.Services.Tests/Rendering/CameraServiceTests.cs`

**Requirements**:
- Track camera position (world coordinates, Vector2)
- Track zoom level (meters per pixel, float)
- Track rotation (radians, float)
- Track viewport dimensions (width, height in pixels)
- Implement "follow vehicle" mode
- Raise `CameraChanged` event on property changes

**Tests**:
- Set position, verify property
- Set zoom, verify property
- Set rotation, verify property
- CameraChanged event fires on changes
- Follow vehicle mode updates position automatically

**Acceptance**:
- Camera properties tracked correctly
- Events fire on changes
- 100% test coverage

---

#### 2.2: Matrix Calculation
**Effort**: 4-6 hours
**Files**:
- `AgValoniaGPS.Services/Rendering/CameraService.cs` (methods)
- `AgValoniaGPS.Services.Tests/Rendering/CameraServiceTests.cs` (matrix tests)

**Requirements**:
- Calculate view matrix (camera position, rotation)
- Calculate projection matrix (orthographic, based on zoom and viewport)
- Calculate combined view-projection matrix
- Use `System.Numerics.Matrix4x4`

**Tests**:
- View matrix identity at origin with 0 rotation
- View matrix translates correctly
- Projection matrix scales correctly based on zoom
- View-projection matrix combines correctly

**Acceptance**:
- Matrices calculated correctly
- Test vectors transform as expected
- 100% test coverage

---

#### 2.3: Coordinate Transformations
**Effort**: 3-4 hours
**Files**:
- `AgValoniaGPS.Services/Rendering/CameraService.cs` (methods)
- `AgValoniaGPS.Services.Tests/Rendering/CameraServiceTests.cs` (transform tests)

**Requirements**:
- `ScreenToWorld(Vector2 screenPoint)`: Convert screen pixel to world coordinates
- `WorldToScreen(Vector2 worldPoint)`: Convert world coordinates to screen pixel
- Handle rotation correctly
- Handle zoom correctly

**Tests**:
- Screen center maps to camera position
- Round-trip conversion (world → screen → world) is accurate
- Zoom affects conversion scale
- Rotation affects conversion

**Acceptance**:
- Conversions mathematically correct
- Round-trip error <0.01 pixels
- 100% test coverage

---

#### 2.4: Camera Control Methods
**Effort**: 3-4 hours
**Files**:
- `AgValoniaGPS.Services/Rendering/CameraService.cs` (methods)
- `AgValoniaGPS.Services.Tests/Rendering/CameraServiceTests.cs` (control tests)

**Requirements**:
- `Pan(Vector2 delta)`: Move camera by delta in world coordinates
- `Zoom(float factor, Vector2? focusPoint)`: Zoom toward focus point
- `Rotate(float angleDelta)`: Rotate camera by angle
- `FitBounds(BoundingBox bounds)`: Fit bounds to viewport
- `CenterOn(Vector2 worldPoint)`: Center camera on point

**Tests**:
- Pan moves camera position
- Zoom changes zoom level and adjusts position for focus point
- Rotate changes rotation angle
- FitBounds calculates correct zoom and position
- CenterOn sets position correctly

**Acceptance**:
- All camera controls work correctly
- Zoom-to-focus-point maintains focus point on screen
- 100% test coverage

---

**Task Group 2 Deliverables**:
- ✅ CameraService with full implementation
- ✅ Matrix calculation (view, projection, view-projection)
- ✅ Coordinate transformations (screen ↔ world)
- ✅ Camera control methods (pan, zoom, rotate, fit, center)
- ✅ 100% test coverage

---

## Task Group 3: Geometry Services

**Estimated Effort**: 3-4 days
**Dependencies**: None (independent of rendering)
**Can Start**: Immediately (parallel with Task Group 1)

### Tasks:

#### 3.1: VehicleGeometryService Implementation
**Effort**: 4-6 hours
**Files**:
- `AgValoniaGPS.Services/Rendering/VehicleGeometryService.cs`
- `AgValoniaGPS.Services/Rendering/IVehicleGeometryService.cs`
- `AgValoniaGPS.Services.Tests/Rendering/VehicleGeometryServiceTests.cs`

**Requirements**:
- Generate vehicle body mesh (rectangle or trapezoid)
- Generate implement mesh (rectangle with section divisions)
- Generate heading arrow mesh (triangle)
- Output format: Interleaved vertex array (X, Y, R, G, B)
- Scale based on `VehicleConfiguration` dimensions

**Tests**:
- Vehicle mesh has correct number of vertices
- Implement mesh matches tool width
- Heading arrow points forward
- Colors assigned correctly

**Acceptance**:
- Vehicle mesh generated correctly
- Implement mesh matches configuration
- 100% test coverage

---

#### 3.2: BoundaryGeometryService Implementation
**Effort**: 4-6 hours
**Files**:
- `AgValoniaGPS.Services/Rendering/BoundaryGeometryService.cs`
- `AgValoniaGPS.Services/Rendering/IBoundaryGeometryService.cs`
- `AgValoniaGPS.Services.Tests/Rendering/BoundaryGeometryServiceTests.cs`

**Requirements**:
- Generate boundary line strip from `List<Position>`
- Generate triangulated boundary fill (optional, for solid boundaries)
- Generate headland lines (inner offset boundaries)
- Output format: Vertex array (X, Y)

**Tests**:
- Boundary line follows input points
- Closed boundary has last point = first point
- Headland lines are inset correctly

**Acceptance**:
- Boundary geometry generated correctly
- Line strip format correct
- 100% test coverage

---

#### 3.3: GuidanceGeometryService Implementation
**Effort**: 4-6 hours
**Files**:
- `AgValoniaGPS.Services/Rendering/GuidanceGeometryService.cs`
- `AgValoniaGPS.Services/Rendering/IGuidanceGeometryService.cs`
- `AgValoniaGPS.Services.Tests/Rendering/GuidanceGeometryServiceTests.cs`

**Requirements**:
- Generate AB line geometry (infinite line clipped to visible area)
- Generate curve line geometry (point list)
- Generate contour line geometry (point list)
- Generate cross-track error indicator (perpendicular line from vehicle to guidance)
- Output format: Vertex array (X, Y)

**Tests**:
- AB line extends to visible limits
- Curve line follows input points
- Contour line follows input points
- Cross-track indicator perpendicular to guidance line

**Acceptance**:
- Guidance geometry generated correctly
- Lines render as expected
- 100% test coverage

---

#### 3.4: CoverageGeometryService Implementation
**Effort**: 4-6 hours
**Files**:
- `AgValoniaGPS.Services/Rendering/CoverageGeometryService.cs`
- `AgValoniaGPS.Services/Rendering/ICoverageGeometryService.cs`
- `AgValoniaGPS.Services.Tests/Rendering/CoverageGeometryServiceTests.cs`

**Requirements**:
- Convert `IEnumerable<CoverageTriangle>` to GPU mesh
- Output format: Interleaved vertex array (X, Y, R, G, B) + index array
- Support batching (combine multiple triangles into single mesh)
- Color triangles by pass number or timestamp

**Tests**:
- Triangle mesh has 3 vertices per triangle
- Vertex positions match input triangles
- Colors assigned correctly
- Index array correct

**Acceptance**:
- Coverage mesh generated correctly
- Triangles render with correct colors
- 100% test coverage

---

#### 3.5: SectionGeometryService Extension
**Effort**: 3-4 hours
**Files**:
- `AgValoniaGPS.Services/Section/SectionGeometryService.cs` (extend existing)
- `AgValoniaGPS.Services.Tests/Section/SectionGeometryServiceTests.cs` (add tests)

**Requirements**:
- Add `GenerateSectionOverlayMesh()` method
- Generate colored rectangles for each section
- Rectangle positioned at section lateral offset
- Rectangle width = section width, length = vehicle length
- Color based on section ON/OFF state
- Output format: Interleaved vertex array (X, Y, R, G, B, A)

**Tests**:
- Overlay mesh has correct number of rectangles
- Rectangle positions match section offsets
- Colors match section states
- Alpha set for transparency

**Acceptance**:
- Section overlays generated correctly
- Overlays render with transparency
- Tests pass

---

**Task Group 3 Deliverables**:
- ✅ VehicleGeometryService with 100% test coverage
- ✅ BoundaryGeometryService with 100% test coverage
- ✅ GuidanceGeometryService with 100% test coverage
- ✅ CoverageGeometryService with 100% test coverage
- ✅ SectionGeometryService extension with tests

---

## Task Group 4: Rendering Coordinator & Pipeline

**Estimated Effort**: 3-4 days
**Dependencies**: Task Group 3 (Geometry Services)
**Can Start**: After Task Group 3

### Tasks:

#### 4.1: RenderingCoordinatorService Implementation
**Effort**: 6-8 hours
**Files**:
- `AgValoniaGPS.Services/Rendering/RenderingCoordinatorService.cs`
- `AgValoniaGPS.Services/Rendering/IRenderingCoordinatorService.cs`
- `AgValoniaGPS.Services/Rendering/RenderData.cs` (data structures)
- `AgValoniaGPS.Services.Tests/Rendering/RenderingCoordinatorServiceTests.cs`

**Requirements**:
- Subscribe to service events (position, guidance, section, coverage, boundary)
- Aggregate current render state from all services
- Generate GPU-ready geometry using geometry services
- Track dirty flags per geometry type
- Raise `GeometryChanged` event when geometry updates
- Provide accessors for each render data type

**Tests**:
- Service event subscription works
- Position update triggers vehicle geometry update
- Guidance change triggers guidance geometry update
- Dirty flags tracked correctly
- GeometryChanged event fires

**Acceptance**:
- Coordinator aggregates all geometry correctly
- Service integration verified
- 100% test coverage

---

#### 4.2: RenderPassManager Implementation
**Effort**: 6-8 hours
**Files**:
- `AgValoniaGPS.Services/Rendering/RenderPassManager.cs`
- `AgValoniaGPS.Services.Tests/Rendering/RenderPassManagerTests.cs`

**Requirements**:
- Execute render passes in correct order:
  1. Background (clear)
  2. Coverage map (opaque triangles)
  3. Boundaries (lines)
  4. Guidance lines (lines)
  5. Tram lines (lines)
  6. Section overlays (transparent quads)
  7. Vehicle (opaque mesh)
  8. UI overlay (optional)
- Set OpenGL state for each pass (depth test, blending, line width)
- Use correct shader for each pass
- Accept render settings (show/hide each layer)

**Tests**:
- Render passes execute in correct order
- OpenGL state set correctly per pass
- Render settings respected (hidden layers not rendered)

**Acceptance**:
- Full frame renders without errors
- Geometry appears in correct order (no Z-fighting)
- Transparency works correctly

---

#### 4.3: Service Integration
**Effort**: 4-6 hours
**Files**:
- `AgValoniaGPS.Desktop/DependencyInjection/ServiceCollectionExtensions.cs` (register services)
- `AgValoniaGPS.Desktop/Views/Controls/OpenGLMapControl.cs` (inject services)

**Requirements**:
- Register rendering services in DI container:
  - `ICameraService` → `CameraService` (singleton)
  - `IRenderingCoordinatorService` → `RenderingCoordinatorService` (singleton)
  - `IVehicleGeometryService` → `VehicleGeometryService` (singleton)
  - `IBoundaryGeometryService` → `BoundaryGeometryService` (singleton)
  - `IGuidanceGeometryService` → `GuidanceGeometryService` (singleton)
  - `ICoverageGeometryService` → `CoverageGeometryService` (singleton)
- Inject services into `OpenGLMapControl`
- Wire up service events to trigger re-render

**Tests**:
- Services resolve from DI container
- Service events trigger `OpenGLMapControl.RequestNextFramePresentation()`

**Acceptance**:
- All services registered correctly
- Service events wired up
- Frame invalidation works

---

#### 4.4: Full Rendering Pipeline
**Effort**: 6-8 hours
**Files**:
- `AgValoniaGPS.Desktop/Views/Controls/OpenGLMapControl.cs` (OnOpenGlRender implementation)

**Requirements**:
- Implement full `OnOpenGlRender` method:
  1. Get render data from coordinator
  2. Update camera matrices
  3. Execute render passes via RenderPassManager
  4. Handle frame timing (measure FPS)
- Upload geometry to GPU (VBOs)
- Bind correct shader per pass
- Set uniforms (MVP matrix, colors, etc.)
- Draw geometry (glDrawArrays or glDrawElements)

**Tests**:
- Manual visual test: Full scene renders
- FPS logged (should be ≥60)
- No OpenGL errors

**Acceptance**:
- Complete scene renders correctly
- All geometry visible
- Frame rate ≥60 FPS

---

**Task Group 4 Deliverables**:
- ✅ RenderingCoordinatorService with service integration
- ✅ RenderPassManager with multi-pass rendering
- ✅ Services registered in DI
- ✅ Full rendering pipeline in OpenGLMapControl
- ✅ Frame rate ≥60 FPS verified

---

## Task Group 5: Input Handling

**Estimated Effort**: 2 days
**Dependencies**: Task Group 2 (CameraService)
**Can Start**: After Task Group 2

### Tasks:

#### 5.1: Mouse Input Implementation
**Effort**: 4-6 hours
**Files**:
- `AgValoniaGPS.Desktop/Views/Controls/OpenGLMapControl.cs` (input handlers)

**Requirements**:
- Override `OnPointerPressed`, `OnPointerMoved`, `OnPointerReleased`
- Implement pan: Left button drag moves camera
- Implement zoom: Mouse wheel zooms toward cursor
- Implement select: Single click converts screen → world, emits event
- Track pointer state (pressed position, dragging, etc.)

**Tests**:
- Manual test: Mouse pan works
- Manual test: Mouse wheel zoom works
- Manual test: Click emits event with correct world coordinates

**Acceptance**:
- Mouse input works smoothly
- Pan and zoom feel natural
- No input lag

---

#### 5.2: Touch Input Implementation
**Effort**: 4-6 hours
**Files**:
- `AgValoniaGPS.Desktop/Views/Controls/OpenGLMapControl.cs` (touch handlers)

**Requirements**:
- Override touch event handlers
- Implement single-finger pan
- Implement pinch-to-zoom (two fingers)
- Implement two-finger rotate (optional)
- Track touch points and gestures

**Tests**:
- Manual test on touch screen: Single finger pan works
- Manual test on touch screen: Pinch zoom works
- Manual test on touch screen: Two-finger rotate works (if implemented)

**Acceptance**:
- Touch gestures work correctly
- Pinch zoom feels natural
- No touch input lag

---

#### 5.3: Keyboard Shortcuts Implementation
**Effort**: 2-3 hours
**Files**:
- `AgValoniaGPS.Desktop/Views/Controls/OpenGLMapControl.cs` (keyboard handlers)

**Requirements**:
- Override `OnKeyDown`, `OnKeyUp`
- Implement arrow keys for pan
- Implement +/- for zoom
- Implement Home for center on vehicle
- Implement F for fit field to view
- Implement R for reset rotation

**Tests**:
- Manual test: Arrow keys pan camera
- Manual test: +/- zoom in/out
- Manual test: Home centers on vehicle
- Manual test: F fits field
- Manual test: R resets rotation

**Acceptance**:
- All keyboard shortcuts work
- Shortcuts documented in UI

---

**Task Group 5 Deliverables**:
- ✅ Mouse input (pan, zoom, select)
- ✅ Touch input (pan, pinch-zoom, rotate)
- ✅ Keyboard shortcuts (pan, zoom, center, fit, reset)
- ✅ All input methods tested and working

---

## Task Group 6: Performance Optimization

**Estimated Effort**: 2-3 days
**Dependencies**: Task Group 4 (Rendering Pipeline)
**Can Start**: After Task Group 4

### Tasks:

#### 6.1: Geometry Batching
**Effort**: 4-6 hours
**Files**:
- `AgValoniaGPS.Services/Rendering/RenderingCoordinatorService.cs` (batching logic)

**Requirements**:
- Combine all field boundaries into single VBO
- Combine all guidance lines into single VBO
- Combine all coverage triangles into single VBO
- Reduce draw calls per frame from N to 1 per geometry type
- Support partial updates (e.g., add new coverage triangles without rebuilding entire mesh)

**Tests**:
- Draw call count measured (should be ≤10 per frame)
- Batching doesn't break rendering
- Frame rate improves

**Acceptance**:
- Draw call count reduced
- Frame rate ≥60 FPS with 10,000+ triangles

---

#### 6.2: Frustum Culling
**Effort**: 4-6 hours
**Files**:
- `AgValoniaGPS.Services/Rendering/CameraService.cs` (frustum calculation)
- `AgValoniaGPS.Services/Rendering/RenderPassManager.cs` (culling logic)

**Requirements**:
- Calculate view frustum (visible rectangle in world coordinates)
- Skip rendering objects outside frustum
- Apply to boundaries, guidance lines, coverage triangles
- Use bounding boxes for quick rejection

**Tests**:
- Objects outside frustum not rendered
- Frame rate improves for large fields

**Acceptance**:
- Frustum culling works correctly
- No visible popping (objects appear/disappear smoothly)
- Frame rate improves when zoomed in

---

#### 6.3: Coverage Map LOD
**Effort**: 3-4 hours
**Files**:
- `AgValoniaGPS.Services/Rendering/CoverageGeometryService.cs` (LOD logic)

**Requirements**:
- Implement level-of-detail for coverage map
- Far zoom: Skip triangles (render every Nth triangle)
- Close zoom: Render all triangles
- LOD based on zoom level thresholds

**Tests**:
- Triangle count reduces at far zoom
- No visible gaps in coverage
- Frame rate improves at far zoom

**Acceptance**:
- LOD works smoothly
- Coverage map still looks complete at all zoom levels
- Frame rate ≥60 FPS at far zoom

---

#### 6.4: Dirty Flag Optimization
**Effort**: 3-4 hours
**Files**:
- `AgValoniaGPS.Services/Rendering/RenderingCoordinatorService.cs` (dirty tracking)

**Requirements**:
- Track which geometry types have changed
- Only regenerate changed geometry
- Cache unchanged VBOs
- Skip rendering unchanged layers

**Tests**:
- Geometry regeneration only happens when dirty
- Frame rate stable when nothing changes

**Acceptance**:
- Dirty flags tracked correctly
- Unnecessary regeneration eliminated
- Frame rate stable at ≥60 FPS

---

#### 6.5: Performance Profiling
**Effort**: 2-3 hours
**Files**:
- `AgValoniaGPS.Services.Tests/Rendering/PerformanceTests.cs`

**Requirements**:
- Measure frame rate with various triangle counts (1K, 10K, 100K)
- Measure position update latency
- Measure camera update latency
- Measure geometry generation time

**Tests**:
- Frame rate ≥60 FPS with 10,000 triangles
- Position update latency <16ms
- Camera update latency <16ms
- Geometry generation <5ms per type

**Acceptance**:
- All performance targets met
- Bottlenecks identified and optimized

---

**Task Group 6 Deliverables**:
- ✅ Geometry batching (reduced draw calls)
- ✅ Frustum culling (skip off-screen objects)
- ✅ Coverage map LOD (scale triangle count with zoom)
- ✅ Dirty flag optimization (avoid unnecessary work)
- ✅ Performance profiling (verify targets met)

---

## Task Group 7: Polish & Documentation

**Estimated Effort**: 1-2 days
**Dependencies**: All task groups
**Can Start**: After all other task groups

### Tasks:

#### 7.1: Anti-Aliasing
**Effort**: 2-3 hours
**Files**:
- `AgValoniaGPS.Desktop/Views/Controls/OpenGLMapControl.cs` (MSAA setup)

**Requirements**:
- Enable MSAA (Multisample Anti-Aliasing) in OpenGL context
- Test 2x, 4x, 8x MSAA
- Choose optimal setting (4x recommended)
- Fallback to no MSAA if unsupported

**Tests**:
- Manual visual test: Lines appear smooth
- No jagged edges on guidance lines or boundaries

**Acceptance**:
- Anti-aliasing enabled
- Lines look smooth

---

#### 7.2: Visual Polish
**Effort**: 3-4 hours
**Files**:
- `AgValoniaGPS.Desktop/Views/Controls/OpenGLMapControl.cs` (animations)
- `AgValoniaGPS.Services/Rendering/CameraService.cs` (smooth transitions)

**Requirements**:
- Add smooth camera transitions (pan, zoom)
- Add fade-in animation for newly loaded geometry
- Add scale bar overlay (shows map scale)
- Add compass overlay (shows north direction)

**Tests**:
- Manual test: Camera transitions smooth
- Manual test: Scale bar accurate
- Manual test: Compass points north

**Acceptance**:
- UI feels polished and professional
- Animations smooth

---

#### 7.3: Developer Documentation
**Effort**: 3-4 hours
**Files**:
- `AgValoniaGPS/agent-os/specs/2025-10-30-wave-11-opengl-rendering/IMPLEMENTATION_GUIDE.md`

**Requirements**:
- Document architecture (rendering pipeline, services)
- Document shader system
- Document coordinate systems
- Document how to add new geometry types
- Document performance optimization techniques

**Acceptance**:
- Documentation clear and complete
- New developers can understand rendering system

---

#### 7.4: User Guide
**Effort**: 2-3 hours
**Files**:
- `AgValoniaGPS/agent-os/specs/2025-10-30-wave-11-opengl-rendering/USER_GUIDE.md`

**Requirements**:
- Document camera controls (mouse, touch, keyboard)
- Document visualization settings (show/hide layers)
- Document troubleshooting (OpenGL version issues, performance)
- Add screenshots

**Acceptance**:
- User guide clear and complete
- Users can understand how to navigate the map

---

#### 7.5: Demo Video
**Effort**: 1-2 hours
**Files**:
- `AgValoniaGPS/agent-os/specs/2025-10-30-wave-11-opengl-rendering/demo.mp4`

**Requirements**:
- Record video showing:
  - Field loading
  - Vehicle movement
  - Guidance line display
  - Coverage map accumulation
  - Camera controls (pan, zoom, rotate)
  - Touch gestures (if available)
- Duration: 2-3 minutes
- Narration or text overlays

**Acceptance**:
- Video demonstrates all key features
- Video is smooth (60 FPS)

---

**Task Group 7 Deliverables**:
- ✅ Anti-aliasing (MSAA)
- ✅ Visual polish (animations, overlays)
- ✅ Developer documentation
- ✅ User guide
- ✅ Demo video

---

## Success Criteria

### Functional Requirements
- [ ] OpenGL context created successfully on Windows, Linux, macOS
- [ ] Vehicle renders at correct position with correct heading
- [ ] Field boundaries render correctly
- [ ] Guidance lines (AB, Curve, Contour) render correctly
- [ ] Coverage map triangles render with correct colors
- [ ] Section overlays show current section states
- [ ] Camera can pan, zoom, rotate via mouse/keyboard/touch
- [ ] Follow vehicle mode works (camera tracks vehicle)
- [ ] Service updates trigger immediate re-render

### Performance Requirements
- [ ] Frame rate ≥60 FPS under normal load (1,000-10,000 triangles)
- [ ] Position update latency <16ms
- [ ] Camera update latency <16ms
- [ ] Field load time <500ms for 100 hectare field
- [ ] No frame drops during vehicle movement
- [ ] Smooth pan/zoom/rotate with no stuttering

### Quality Requirements
- [ ] No OpenGL errors during rendering
- [ ] No memory leaks (proper disposal of GPU resources)
- [ ] Clean shader compilation (no warnings)
- [ ] Accurate coordinate transformations (screen ↔ world)
- [ ] Correct depth ordering (no Z-fighting)
- [ ] Proper transparency rendering (alpha blending)
- [ ] 100% unit test coverage for services

---

## Estimated Timeline

**Sequential (Single Developer)**:
- Task Group 1: 2-3 days
- Task Group 2: 2 days
- Task Group 3: 3-4 days (parallel possible)
- Task Group 4: 3-4 days
- Task Group 5: 2 days
- Task Group 6: 2-3 days
- Task Group 7: 1-2 days
- **Total: 15-20 days**

**Parallel (Multiple Agents)**:
- Week 1:
  - Task Group 1 (Foundation) - Agent A
  - Task Group 3 (Geometry Services) - Agents B, C, D (parallel)
- Week 2:
  - Task Group 2 (Camera) - Agent A
  - Task Group 4 (Rendering Pipeline) - Agent B
  - Task Group 5 (Input) - Agent C (can start after camera)
- Week 3:
  - Task Group 6 (Performance) - Agent A
  - Task Group 7 (Polish) - Agent B
- **Total: 3 weeks with 4 agents**

---

## Dependencies Graph

```
Task Group 1 (Foundation)
     ├─→ Task Group 2 (Camera)
     │        ├─→ Task Group 5 (Input)
     │        └─→ Task Group 4 (Rendering Pipeline)
     │                 └─→ Task Group 6 (Performance)
     │                          └─→ Task Group 7 (Polish)
     └─→ Task Group 3 (Geometry Services)
              └─→ Task Group 4 (Rendering Pipeline)
```

**Critical Path**: TG1 → TG2 → TG4 → TG6 → TG7 (15-18 days)

**Parallelizable**:
- TG3 can run parallel with TG1 (saves 3-4 days)
- TG5 can run parallel with TG4 (saves 2 days)
- With full parallelization: 10-12 days minimum

---

## Risk Mitigation

### Risk 1: OpenGL Compatibility Issues
**Mitigation**: Test on multiple platforms early
**Timeline Impact**: +1-2 days if compatibility issues found

### Risk 2: Performance Below Target
**Mitigation**: Profile early (Task Group 6)
**Timeline Impact**: +2-3 days if major optimization needed

### Risk 3: Avalonia OpenGL Integration Problems
**Mitigation**: Test Avalonia OpenGL samples first (Day 1)
**Timeline Impact**: +3-5 days if custom hosting needed

---

**End of Task Breakdown**
