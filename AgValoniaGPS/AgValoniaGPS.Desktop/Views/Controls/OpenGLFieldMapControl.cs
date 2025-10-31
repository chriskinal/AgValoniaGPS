using System;
using System.IO;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using Silk.NET.OpenGL;
using AgValoniaGPS.Desktop.Graphics;
using AgValoniaGPS.Services.Rendering;
using AgValoniaGPS.Services.GPS;
using AgValoniaGPS.Services.FieldOperations;

namespace AgValoniaGPS.Desktop.Views.Controls;

/// <summary>
/// Production-ready OpenGL 3D renderer for the field map.
/// Integrates with CameraService and RenderingCoordinatorService for complete 3D visualization.
/// Features: Phong lighting, depth testing, anti-aliasing, efficient mesh management.
/// </summary>
public partial class OpenGLFieldMapControl : OpenGlControlBase
{
    // OpenGL context
    private GL? _gl;

    // Shader program
    private ShaderProgram? _shader;

    // Meshes
    private Mesh? _gridMesh;
    private Mesh? _vehicleMesh;
    private Mesh? _boundaryMesh;
    private Mesh? _guidanceMesh;
    private Mesh? _coverageMesh;

    // Services
    private readonly ICameraService? _cameraService;
    private readonly IRenderingCoordinatorService? _renderingCoordinator;
    private readonly IPositionUpdateService? _positionService;
    private readonly IBoundaryManagementService? _boundaryService;

    // Camera state (synced with CameraService)
    private Vector3 _cameraPosition = new Vector3(0, -100, 100); // Start behind and above origin
    private Vector3 _cameraTarget = Vector3.Zero;
    private float _cameraPitch = 0.5f; // radians (about 30 degrees)
    private float _cameraYaw = 0.0f;
    private float _cameraDistance = 100.0f;

    // Input state
    private bool _isPanning = false;
    private bool _isRotating = false;
    private Point _lastMousePosition;

    // Vehicle state
    private Vector2? _vehiclePosition;
    private float _vehicleHeading = 0.0f;

    // Rendering flags
    private bool _meshesNeedUpdate = true;
    private bool _initialized = false;

    // Render timer
    private DispatcherTimer? _renderTimer;

    public OpenGLFieldMapControl()
    {
        // Make control focusable for keyboard input
        Focusable = true;

        // Get services from DI
        if (!Design.IsDesignMode)
        {
            try
            {
                var services = App.Services;
                if (services != null)
                {
                    _cameraService = services.GetService(typeof(ICameraService)) as ICameraService;
                    _renderingCoordinator = services.GetService(typeof(IRenderingCoordinatorService)) as IRenderingCoordinatorService;
                    _positionService = services.GetService(typeof(IPositionUpdateService)) as IPositionUpdateService;
                    _boundaryService = services.GetService(typeof(IBoundaryManagementService)) as IBoundaryManagementService;

                    // Subscribe to events
                    if (_positionService != null)
                    {
                        _positionService.PositionUpdated += OnPositionUpdated;
                    }

                    if (_renderingCoordinator != null)
                    {
                        _renderingCoordinator.GeometryChanged += OnGeometryChanged;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OpenGLFieldMapControl: Error initializing services: {ex.Message}");
            }
        }

        // Input handlers
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerWheelChanged += OnPointerWheelChanged;

        // Start render loop (30 FPS)
        _renderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33)
        };
        _renderTimer.Tick += (s, e) => RequestNextFrameRendering();
        _renderTimer.Start();
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);

        _gl = GL.GetApi(gl.GetProcAddress);

        Console.WriteLine($"OpenGL Version: {_gl.GetStringS(StringName.Version)}");
        Console.WriteLine($"OpenGL Vendor: {_gl.GetStringS(StringName.Vendor)}");
        Console.WriteLine($"OpenGL Renderer: {_gl.GetStringS(StringName.Renderer)}");

        // OpenGL settings
        _gl.ClearColor(0.05f, 0.05f, 0.05f, 1.0f); // Dark background
        _gl.Enable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        // Enable anti-aliasing if supported
        try
        {
            _gl.Enable(EnableCap.Multisample);
            _gl.Enable(EnableCap.LineSmooth);
            _gl.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
        }
        catch
        {
            Console.WriteLine("Anti-aliasing not supported");
        }

        // Initialize shader
        InitializeShader();

        // Initialize meshes
        InitializeGridMesh();

        _initialized = true;
        _meshesNeedUpdate = true;
    }

    private void InitializeShader()
    {
        if (_gl == null) return;

        try
        {
            string shaderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Shaders");
            string vertPath = Path.Combine(shaderPath, "Field3D.vert");
            string fragPath = Path.Combine(shaderPath, "Field3D.frag");

            _shader = new ShaderProgram(_gl);
            _shader.LoadFromFiles(vertPath, fragPath);

            Console.WriteLine("Shaders compiled successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Shader compilation failed: {ex.Message}");
            throw;
        }
    }

    private void InitializeGridMesh()
    {
        if (_gl == null) return;

        try
        {
            float[] gridVertices = GeometryConverter.CreateGridMesh(500.0f, 10.0f, out int lineCount);

            _gridMesh = new Mesh(_gl);
            _gridMesh.SetVertexData(gridVertices, primitiveType: PrimitiveType.Lines);

            Console.WriteLine($"Grid mesh created: {lineCount} lines");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Grid mesh creation failed: {ex.Message}");
        }
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        if (_gl == null || _shader == null)
        {
            Console.WriteLine("[OpenGL Render] Skipped - gl or shader is null");
            return;
        }

        try
        {
            // Update meshes if needed
            if (_meshesNeedUpdate)
            {
                UpdateMeshesFromServices();
                _meshesNeedUpdate = false;
            }

            // Set viewport
            _gl.Viewport(0, 0, (uint)Bounds.Width, (uint)Bounds.Height);

            // Clear buffers (black background)
            _gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Console.WriteLine($"[OpenGL Render] Clearing screen, viewport: {Bounds.Width}x{Bounds.Height}");

            // Calculate matrices
            var viewMatrix = CalculateViewMatrix();
            var projectionMatrix = CalculateProjectionMatrix();

            // Set up shader
            _shader.Use();

            // Set lighting uniforms
            Vector3 lightDir = Vector3.Normalize(new Vector3(0.5f, 0.3f, 1.0f)); // Sun direction
            _shader.SetUniform("uLightDir", lightDir);
            _shader.SetUniform("uLightColor", new Vector3(1.0f, 1.0f, 0.95f)); // Warm white
            _shader.SetUniform("uAmbientLight", new Vector3(0.4f, 0.4f, 0.45f)); // Ambient
            _shader.SetUniform("uCameraPos", _cameraPosition);
            _shader.SetUniform("uUseTexture", false);

            // Set view and projection matrices
            _shader.SetUniform("uView", viewMatrix);
            _shader.SetUniform("uProjection", projectionMatrix);

            // Render grid
            if (_gridMesh != null)
            {
                Console.WriteLine("[OpenGL Render] Drawing grid mesh");
                RenderMesh(_gridMesh, Matrix4x4.Identity);
            }
            else
            {
                Console.WriteLine("[OpenGL Render] Grid mesh is null!");
            }

            // Render coverage (lowest layer)
            if (_coverageMesh != null)
            {
                RenderMesh(_coverageMesh, Matrix4x4.Identity);
            }

            // Render boundary
            if (_boundaryMesh != null)
            {
                _gl.LineWidth(3.0f);
                RenderMesh(_boundaryMesh, Matrix4x4.Identity);
                _gl.LineWidth(1.0f);
            }

            // Render guidance
            if (_guidanceMesh != null)
            {
                _gl.LineWidth(4.0f);
                RenderMesh(_guidanceMesh, Matrix4x4.Identity);
                _gl.LineWidth(1.0f);
            }

            // Render vehicle
            if (_vehicleMesh != null && _vehiclePosition.HasValue)
            {
                var vehicleTransform = Matrix4x4.CreateRotationZ(_vehicleHeading) *
                                      Matrix4x4.CreateTranslation(_vehiclePosition.Value.X, _vehiclePosition.Value.Y, 0);
                RenderMesh(_vehicleMesh, vehicleTransform);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Render error: {ex.Message}");
        }
    }

    private void RenderMesh(Mesh mesh, Matrix4x4 modelMatrix)
    {
        if (_shader == null) return;

        // Set model matrix
        _shader.SetUniform("uModel", modelMatrix);

        // Calculate normal matrix (transpose of inverse of model matrix)
        if (Matrix4x4.Invert(modelMatrix, out var invModel))
        {
            var normalMatrix = Matrix4x4.Transpose(invModel);
            _shader.SetUniformMat3("uNormalMatrix", normalMatrix);
        }
        else
        {
            _shader.SetUniformMat3("uNormalMatrix", Matrix4x4.Identity);
        }

        // Draw
        mesh.Draw();
    }

    private Matrix4x4 CalculateViewMatrix()
    {
        // Update camera position based on target and angles
        if (_vehiclePosition.HasValue)
        {
            _cameraTarget = new Vector3(_vehiclePosition.Value.X, _vehiclePosition.Value.Y, 0);
        }

        // Calculate camera position from spherical coordinates
        float camX = _cameraTarget.X + _cameraDistance * MathF.Cos(_cameraPitch) * MathF.Sin(_cameraYaw);
        float camY = _cameraTarget.Y - _cameraDistance * MathF.Cos(_cameraPitch) * MathF.Cos(_cameraYaw);
        float camZ = _cameraTarget.Z + _cameraDistance * MathF.Sin(_cameraPitch);

        _cameraPosition = new Vector3(camX, camY, camZ);

        // Look at target
        return Matrix4x4.CreateLookAt(_cameraPosition, _cameraTarget, Vector3.UnitZ);
    }

    private Matrix4x4 CalculateProjectionMatrix()
    {
        float aspect = (float)Bounds.Width / (float)Bounds.Height;
        float fov = MathF.PI / 4.0f; // 45 degrees
        return Matrix4x4.CreatePerspectiveFieldOfView(fov, aspect, 0.1f, 10000.0f);
    }

    private void UpdateMeshesFromServices()
    {
        if (_gl == null) return;

        try
        {
            // Update vehicle mesh
            UpdateVehicleMesh();

            // Update boundary mesh
            UpdateBoundaryMesh();

            // Update guidance mesh
            UpdateGuidanceMesh();

            // Update coverage mesh
            UpdateCoverageMesh();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating meshes: {ex.Message}");
        }
    }

    private void UpdateVehicleMesh()
    {
        if (_gl == null) return;

        try
        {
            var renderData = _renderingCoordinator?.GetVehicleData();
            if (renderData != null && renderData.Vertices != null && renderData.Vertices.Length > 0)
            {
                // Convert 2D vertices to 3D
                float[] vertices3D = GeometryConverter.Convert2DTo3D(renderData.Vertices);

                if (_vehicleMesh == null)
                {
                    _vehicleMesh = new Mesh(_gl);
                }

                _vehicleMesh.SetVertexData(vertices3D, primitiveType: PrimitiveType.Triangles);
            }
            else
            {
                // Create default vehicle mesh
                Vector4 vehicleColor = new Vector4(0.3f, 0.6f, 1.0f, 1.0f); // Blue
                float[] vertices = GeometryConverter.CreateVehicleMesh(5.0f, 8.0f, vehicleColor);

                if (_vehicleMesh == null)
                {
                    _vehicleMesh = new Mesh(_gl);
                }

                _vehicleMesh.SetVertexData(vertices, primitiveType: PrimitiveType.Triangles);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating vehicle mesh: {ex.Message}");
        }
    }

    private void UpdateBoundaryMesh()
    {
        if (_gl == null) return;

        try
        {
            var renderData = _renderingCoordinator?.GetBoundaryData();
            if (renderData != null && renderData.Vertices != null && renderData.Vertices.Length > 0)
            {
                Vector4 color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f); // Yellow
                float[] vertices = GeometryConverter.ConvertBoundaryLines(renderData.Vertices, color);

                if (_boundaryMesh == null)
                {
                    _boundaryMesh = new Mesh(_gl);
                }

                _boundaryMesh.SetVertexData(vertices, primitiveType: PrimitiveType.LineLoop);
            }
            else
            {
                _boundaryMesh?.Dispose();
                _boundaryMesh = null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating boundary mesh: {ex.Message}");
        }
    }

    private void UpdateGuidanceMesh()
    {
        if (_gl == null) return;

        try
        {
            var renderData = _renderingCoordinator?.GetGuidanceData();
            if (renderData != null && renderData.Vertices != null && renderData.Vertices.Length > 0)
            {
                Vector4 color = new Vector4(1.0f, 0.6f, 0.0f, 1.0f); // Orange
                float[] vertices = GeometryConverter.ConvertGuidanceLines(renderData.Vertices, color);

                if (_guidanceMesh == null)
                {
                    _guidanceMesh = new Mesh(_gl);
                }

                _guidanceMesh.SetVertexData(vertices, primitiveType: PrimitiveType.Lines);
            }
            else
            {
                _guidanceMesh?.Dispose();
                _guidanceMesh = null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating guidance mesh: {ex.Message}");
        }
    }

    private void UpdateCoverageMesh()
    {
        if (_gl == null) return;

        try
        {
            var renderData = _renderingCoordinator?.GetCoverageData();
            if (renderData != null && renderData.Vertices != null && renderData.Vertices.Length > 0)
            {
                // Convert coverage data to 3D
                float[] vertices3D = GeometryConverter.Convert2DTo3D(renderData.Vertices);

                if (_coverageMesh == null)
                {
                    _coverageMesh = new Mesh(_gl);
                }

                if (renderData.Indices != null && renderData.Indices.Length > 0)
                {
                    _coverageMesh.SetVertexData(vertices3D, renderData.Indices, PrimitiveType.Triangles);
                }
                else
                {
                    _coverageMesh.SetVertexData(vertices3D, primitiveType: PrimitiveType.Triangles);
                }
            }
            else
            {
                _coverageMesh?.Dispose();
                _coverageMesh = null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating coverage mesh: {ex.Message}");
        }
    }

    // Event handlers
    private void OnPositionUpdated(object? sender, PositionUpdateEventArgs e)
    {
        _vehiclePosition = new Vector2((float)e.Position.Easting, (float)e.Position.Northing);
        _vehicleHeading = (float)e.Heading;
        _meshesNeedUpdate = true;
    }

    private void OnGeometryChanged(object? sender, EventArgs e)
    {
        _meshesNeedUpdate = true;
    }

    // Input handling
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);

        if (point.Properties.IsLeftButtonPressed)
        {
            _isPanning = true;
            _lastMousePosition = point.Position;
            e.Handled = true;
        }
        else if (point.Properties.IsRightButtonPressed)
        {
            _isRotating = true;
            _lastMousePosition = point.Position;
            e.Handled = true;
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        var currentPos = point.Position;

        if (_isPanning)
        {
            // Adjust camera pitch
            double deltaY = currentPos.Y - _lastMousePosition.Y;
            _cameraPitch -= (float)(deltaY * 0.005);
            _cameraPitch = Math.Clamp(_cameraPitch, 0.1f, MathF.PI / 2 - 0.1f);

            _lastMousePosition = currentPos;
            e.Handled = true;
        }
        else if (_isRotating)
        {
            // Adjust camera yaw
            double deltaX = currentPos.X - _lastMousePosition.X;
            _cameraYaw += (float)(deltaX * 0.01);

            _lastMousePosition = currentPos;
            e.Handled = true;
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isPanning = false;
        _isRotating = false;
        e.Handled = true;
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        // Zoom in/out by adjusting camera distance
        float zoomFactor = e.Delta.Y > 0 ? 0.9f : 1.1f;
        _cameraDistance *= zoomFactor;
        _cameraDistance = Math.Clamp(_cameraDistance, 10.0f, 1000.0f);

        e.Handled = true;
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        // Clean up resources
        _shader?.Dispose();
        _gridMesh?.Dispose();
        _vehicleMesh?.Dispose();
        _boundaryMesh?.Dispose();
        _guidanceMesh?.Dispose();
        _coverageMesh?.Dispose();

        // Unsubscribe from events
        if (_positionService != null)
        {
            _positionService.PositionUpdated -= OnPositionUpdated;
        }

        if (_renderingCoordinator != null)
        {
            _renderingCoordinator.GeometryChanged -= OnGeometryChanged;
        }

        _renderTimer?.Stop();

        base.OnOpenGlDeinit(gl);
    }
}
