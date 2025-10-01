using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using Silk.NET.OpenGL;

namespace AgValoniaGPS.Desktop.Controls;

public class OpenGLMapControl : OpenGlControlBase
{
    private GL? _gl;
    private uint _gridVao;
    private uint _gridVbo;
    private uint _vehicleVao;
    private uint _vehicleVbo;
    private uint _shaderProgram;
    private int _gridVertexCount;

    // Camera/viewport properties
    private double _cameraX = 0.0;
    private double _cameraY = 0.0;
    private double _zoom = 1.0;
    private double _rotation = 0.0; // Radians

    // GPS/Vehicle position
    private double _vehicleX = 0.0;      // Meters (world coordinates)
    private double _vehicleY = 0.0;      // Meters (world coordinates)
    private double _vehicleHeading = 0.0; // Radians

    // Mouse interaction state
    private bool _isPanning = false;
    private bool _isRotating = false;
    private Point _lastMousePosition;

    public OpenGLMapControl()
    {
        // Make control focusable and set to accept all pointer events
        Focusable = true;
        IsHitTestVisible = true;
        ClipToBounds = false;

        // Start render loop
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        timer.Tick += (s, e) => RequestNextFrameRendering();
        timer.Start();

        // Wire up mouse events for camera control
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerWheelChanged += OnPointerWheelChanged;
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);

        // Initialize Silk.NET OpenGL context
        _gl = GL.GetApi(gl.GetProcAddress);

        Console.WriteLine($"OpenGL Version: {_gl.GetStringS(StringName.Version)}");
        Console.WriteLine($"OpenGL Vendor: {_gl.GetStringS(StringName.Vendor)}");
        Console.WriteLine($"OpenGL Renderer: {_gl.GetStringS(StringName.Renderer)}");

        // Set clear color (dark background for grid visibility)
        _gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

        // Enable blending for transparency
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        // Initialize basic rendering resources
        try
        {
            InitializeShaders();
            InitializeGrid();
            InitializeVehicle();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR during initialization: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private void InitializeShaders()
    {
        if (_gl == null) return;

        // Simple vertex shader (2D positions with MVP transform) - OpenGL ES 3.0 compatible
        const string vertexShaderSource = @"#version 300 es
precision highp float;
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec4 aColor;

uniform mat4 uMVP;

out vec4 vColor;

void main()
{
    gl_Position = uMVP * vec4(aPosition, 0.0, 1.0);
    vColor = aColor;
}";

        // Simple fragment shader (pass through color) - OpenGL ES 3.0 compatible
        const string fragmentShaderSource = @"#version 300 es
precision highp float;
in vec4 vColor;
out vec4 FragColor;

void main()
{
    FragColor = vColor;
}";

        // Compile vertex shader
        uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, vertexShaderSource);
        _gl.CompileShader(vertexShader);

        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int)GLEnum.True)
        {
            string log = _gl.GetShaderInfoLog(vertexShader);
            throw new Exception($"Vertex shader compilation failed: {log}");
        }

        // Compile fragment shader
        uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, fragmentShaderSource);
        _gl.CompileShader(fragmentShader);

        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int)GLEnum.True)
        {
            string log = _gl.GetShaderInfoLog(fragmentShader);
            throw new Exception($"Fragment shader compilation failed: {log}");
        }

        // Link shader program
        _shaderProgram = _gl.CreateProgram();
        _gl.AttachShader(_shaderProgram, vertexShader);
        _gl.AttachShader(_shaderProgram, fragmentShader);
        _gl.LinkProgram(_shaderProgram);

        _gl.GetProgram(_shaderProgram, ProgramPropertyARB.LinkStatus, out int pStatus);
        if (pStatus != (int)GLEnum.True)
        {
            string log = _gl.GetProgramInfoLog(_shaderProgram);
            throw new Exception($"Shader program linking failed: {log}");
        }

        // Clean up shaders (no longer needed after linking)
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);
    }

    private void InitializeGrid()
    {
        if (_gl == null) return;

        // Create grid lines (10m spacing, 100m x 100m grid)
        var gridVertices = new System.Collections.Generic.List<float>();
        float gridSize = 500.0f; // 500m x 500m grid
        float spacing = 10.0f;   // 10m spacing
        float alpha = 0.3f;      // Semi-transparent

        // Vertical lines
        for (float x = -gridSize; x <= gridSize; x += spacing)
        {
            // Brighter lines every 50m
            float lineAlpha = (Math.Abs(x % 50.0f) < 0.1f) ? 0.5f : alpha;

            gridVertices.Add(x); gridVertices.Add(-gridSize); // Position
            gridVertices.Add(0.3f); gridVertices.Add(0.3f); gridVertices.Add(0.3f); gridVertices.Add(lineAlpha); // Gray color

            gridVertices.Add(x); gridVertices.Add(gridSize);
            gridVertices.Add(0.3f); gridVertices.Add(0.3f); gridVertices.Add(0.3f); gridVertices.Add(lineAlpha);
        }

        // Horizontal lines
        for (float y = -gridSize; y <= gridSize; y += spacing)
        {
            float lineAlpha = (Math.Abs(y % 50.0f) < 0.1f) ? 0.5f : alpha;

            gridVertices.Add(-gridSize); gridVertices.Add(y);
            gridVertices.Add(0.3f); gridVertices.Add(0.3f); gridVertices.Add(0.3f); gridVertices.Add(lineAlpha);

            gridVertices.Add(gridSize); gridVertices.Add(y);
            gridVertices.Add(0.3f); gridVertices.Add(0.3f); gridVertices.Add(0.3f); gridVertices.Add(lineAlpha);
        }

        // Axis lines (brighter)
        // X-axis (red)
        gridVertices.Add(-gridSize); gridVertices.Add(0.0f);
        gridVertices.Add(0.8f); gridVertices.Add(0.2f); gridVertices.Add(0.2f); gridVertices.Add(0.8f);
        gridVertices.Add(gridSize); gridVertices.Add(0.0f);
        gridVertices.Add(0.8f); gridVertices.Add(0.2f); gridVertices.Add(0.2f); gridVertices.Add(0.8f);

        // Y-axis (green)
        gridVertices.Add(0.0f); gridVertices.Add(-gridSize);
        gridVertices.Add(0.2f); gridVertices.Add(0.8f); gridVertices.Add(0.2f); gridVertices.Add(0.8f);
        gridVertices.Add(0.0f); gridVertices.Add(gridSize);
        gridVertices.Add(0.2f); gridVertices.Add(0.8f); gridVertices.Add(0.2f); gridVertices.Add(0.8f);

        _gridVertexCount = gridVertices.Count / 6; // 6 floats per vertex (x, y, r, g, b, a)

        // Create grid VAO/VBO
        _gridVao = _gl.GenVertexArray();
        _gl.BindVertexArray(_gridVao);

        _gridVbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _gridVbo);

        float[] gridArray = gridVertices.ToArray();
        unsafe
        {
            fixed (float* v = gridArray)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(gridArray.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
            }
        }

        // Position attribute
        unsafe
        {
            _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)0);
        }
        _gl.EnableVertexAttribArray(0);

        // Color attribute
        unsafe
        {
            _gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)(2 * sizeof(float)));
        }
        _gl.EnableVertexAttribArray(1);

        _gl.BindVertexArray(0);
    }

    private void InitializeVehicle()
    {
        if (_gl == null) return;

        // Create a simple triangle to represent the vehicle (pointing up/forward)
        // Scale: about 5 meters (typical tractor size)
        float size = 5.0f;

        float[] vehicleVertices = new float[]
        {
            // Triangle pointing up (forward direction)
            // Position (x, y), Color (orange)
             0.0f,  size,       1.0f, 0.5f, 0.0f, 1.0f,  // Front point
            -size * 0.7f, -size * 0.5f,  1.0f, 0.5f, 0.0f, 1.0f,  // Rear left
             size * 0.7f, -size * 0.5f,  1.0f, 0.5f, 0.0f, 1.0f   // Rear right
        };

        // Create vehicle VAO/VBO
        _vehicleVao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vehicleVao);

        _vehicleVbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vehicleVbo);

        unsafe
        {
            fixed (float* v = vehicleVertices)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vehicleVertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
            }
        }

        // Position attribute
        unsafe
        {
            _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)0);
        }
        _gl.EnableVertexAttribArray(0);

        // Color attribute
        unsafe
        {
            _gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)(2 * sizeof(float)));
        }
        _gl.EnableVertexAttribArray(1);

        _gl.BindVertexArray(0);
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        if (_gl == null) return;

        // Set viewport
        _gl.Viewport(0, 0, (uint)Bounds.Width, (uint)Bounds.Height);

        // Clear the screen
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        // Use shader program
        _gl.UseProgram(_shaderProgram);

        // Create simple orthographic projection matrix (2D)
        float aspect = (float)Bounds.Width / (float)Bounds.Height;
        float viewWidth = 200.0f * aspect / (float)_zoom;
        float viewHeight = 200.0f / (float)_zoom;

        // Create view matrix (translation + rotation)
        float[] view = CreateViewMatrix((float)_cameraX, (float)_cameraY, (float)_rotation);

        // Create orthographic projection
        float[] projection = CreateOrthographicMatrix(
            -viewWidth / 2,
            viewWidth / 2,
            -viewHeight / 2,
            viewHeight / 2
        );

        // Combine projection * view
        float[] mvp = MultiplyMatrices(projection, view);

        // Set MVP uniform
        int mvpLocation = _gl.GetUniformLocation(_shaderProgram, "uMVP");
        unsafe
        {
            fixed (float* m = mvp)
            {
                _gl.UniformMatrix4(_gl.GetUniformLocation(_shaderProgram, "uMVP"), 1, false, m);
            }
        }

        // Draw grid
        _gl.BindVertexArray(_gridVao);
        _gl.DrawArrays(PrimitiveType.Lines, 0, (uint)_gridVertexCount);
        _gl.BindVertexArray(0);

        // Draw vehicle with position and heading
        // Create model matrix for vehicle (translation + rotation)
        float[] vehicleModel = CreateModelMatrix((float)_vehicleX, (float)_vehicleY, (float)_vehicleHeading);
        float[] vehicleMvp = MultiplyMatrices(mvp, vehicleModel);

        unsafe
        {
            fixed (float* m = vehicleMvp)
            {
                _gl.UniformMatrix4(_gl.GetUniformLocation(_shaderProgram, "uMVP"), 1, false, m);
            }
        }

        _gl.BindVertexArray(_vehicleVao);
        _gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
        _gl.BindVertexArray(0);
    }

    private float[] CreateOrthographicMatrix(float left, float right, float bottom, float top)
    {
        // Simple orthographic projection matrix
        float[] matrix = new float[16];
        matrix[0] = 2.0f / (right - left);
        matrix[5] = 2.0f / (top - bottom);
        matrix[10] = -1.0f;
        matrix[12] = -(right + left) / (right - left);
        matrix[13] = -(top + bottom) / (top - bottom);
        matrix[15] = 1.0f;
        return matrix;
    }

    private float[] CreateViewMatrix(float x, float y, float rotation)
    {
        // Create view matrix with translation and rotation
        // View matrix = inverse of camera transform
        float cos = (float)Math.Cos(-rotation);
        float sin = (float)Math.Sin(-rotation);

        float[] matrix = new float[16];
        // Rotation around Z axis
        matrix[0] = cos;
        matrix[1] = sin;
        matrix[4] = -sin;
        matrix[5] = cos;
        matrix[10] = 1.0f;
        matrix[15] = 1.0f;

        // Translation (camera position negated for view matrix)
        matrix[12] = -x * cos - y * (-sin);
        matrix[13] = -x * sin + y * cos;

        return matrix;
    }

    private float[] CreateModelMatrix(float x, float y, float rotation)
    {
        // Create model matrix for an object (translation + rotation)
        float cos = (float)Math.Cos(rotation);
        float sin = (float)Math.Sin(rotation);

        float[] matrix = new float[16];
        // Rotation around Z axis
        matrix[0] = cos;
        matrix[1] = sin;
        matrix[4] = -sin;
        matrix[5] = cos;
        matrix[10] = 1.0f;
        matrix[15] = 1.0f;

        // Translation
        matrix[12] = x;
        matrix[13] = y;

        return matrix;
    }

    private float[] MultiplyMatrices(float[] a, float[] b)
    {
        // Multiply two 4x4 matrices (column-major order)
        float[] result = new float[16];

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                result[col * 4 + row] =
                    a[0 * 4 + row] * b[col * 4 + 0] +
                    a[1 * 4 + row] * b[col * 4 + 1] +
                    a[2 * 4 + row] * b[col * 4 + 2] +
                    a[3 * 4 + row] * b[col * 4 + 3];
            }
        }

        return result;
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        if (_gl != null)
        {
            _gl.DeleteBuffer(_gridVbo);
            _gl.DeleteVertexArray(_gridVao);
            _gl.DeleteBuffer(_vehicleVbo);
            _gl.DeleteVertexArray(_vehicleVao);
            _gl.DeleteProgram(_shaderProgram);
        }

        base.OnOpenGlDeinit(gl);
    }

    // Public methods for camera control (to be called from UI)
    public void SetCamera(double x, double y, double zoom, double rotation)
    {
        _cameraX = x;
        _cameraY = y;
        _zoom = zoom;
        _rotation = rotation;
        RequestNextFrameRendering();
    }

    public void Pan(double deltaX, double deltaY)
    {
        _cameraX += deltaX;
        _cameraY += deltaY;
        RequestNextFrameRendering();
    }

    public void Zoom(double delta)
    {
        _zoom *= delta;
        _zoom = Math.Clamp(_zoom, 0.1, 100.0);
        RequestNextFrameRendering();
    }

    public void Rotate(double deltaRadians)
    {
        _rotation += deltaRadians;
        RequestNextFrameRendering();
    }

    public double GetZoom() => _zoom;

    public void SetVehiclePosition(double x, double y, double heading)
    {
        _vehicleX = x;
        _vehicleY = y;
        _vehicleHeading = heading;
        RequestNextFrameRendering();
    }

    // Mouse event handlers for camera control
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);

        if (point.Properties.IsLeftButtonPressed)
        {
            _isPanning = true;
            _lastMousePosition = point.Position;
            e.Pointer.Capture(this);
            e.Handled = true;
        }
        else if (point.Properties.IsRightButtonPressed)
        {
            _isRotating = true;
            _lastMousePosition = point.Position;
            e.Pointer.Capture(this);
            e.Handled = true;
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        var currentPos = point.Position;

        if (_isPanning)
        {
            // Calculate delta in screen space
            double deltaX = currentPos.X - _lastMousePosition.X;
            double deltaY = currentPos.Y - _lastMousePosition.Y;

            // Convert screen space delta to world space (accounting for zoom and aspect ratio)
            float aspect = (float)Bounds.Width / (float)Bounds.Height;
            double worldDeltaX = -deltaX * (200.0 * aspect / _zoom) / Bounds.Width;
            double worldDeltaY = deltaY * (200.0 / _zoom) / Bounds.Height;

            Pan(worldDeltaX, worldDeltaY);
            _lastMousePosition = currentPos;
            e.Handled = true;
        }
        else if (_isRotating)
        {
            // Calculate rotation based on horizontal mouse movement
            double deltaX = currentPos.X - _lastMousePosition.X;
            double rotationDelta = deltaX * 0.01; // 0.01 radians per pixel

            Rotate(rotationDelta);
            _lastMousePosition = currentPos;
            e.Handled = true;
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isPanning || _isRotating)
        {
            _isPanning = false;
            _isRotating = false;
            e.Pointer.Capture(null);
            e.Handled = true;
        }
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        // Zoom in/out based on mouse wheel delta
        double zoomFactor = e.Delta.Y > 0 ? 1.1 : 0.9;
        Zoom(zoomFactor);
        e.Handled = true;
    }
}
