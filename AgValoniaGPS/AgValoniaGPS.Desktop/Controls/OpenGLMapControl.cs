using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using Silk.NET.OpenGL;

namespace AgValoniaGPS.Desktop.Controls;

public class OpenGLMapControl : OpenGlControlBase
{
    private GL? _gl;
    private uint _vao;
    private uint _vbo;
    private uint _shaderProgram;

    // Camera/viewport properties
    private double _cameraX = 0.0;
    private double _cameraY = 0.0;
    private double _zoom = 1.0;
    private double _rotation = 0.0; // Radians

    public OpenGLMapControl()
    {
        // Start render loop
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        timer.Tick += (s, e) => RequestNextFrameRendering();
        timer.Start();
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);

        // Initialize Silk.NET OpenGL context
        _gl = GL.GetApi(gl.GetProcAddress);

        Console.WriteLine($"OpenGL Version: {_gl.GetStringS(StringName.Version)}");
        Console.WriteLine($"OpenGL Vendor: {_gl.GetStringS(StringName.Vendor)}");
        Console.WriteLine($"OpenGL Renderer: {_gl.GetStringS(StringName.Renderer)}");

        // Set clear color (dark gray background)
        _gl.ClearColor(0.15f, 0.15f, 0.15f, 1.0f);

        // Enable blending for transparency
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        // Initialize basic rendering resources
        InitializeShaders();
        InitializeBuffers();
    }

    private void InitializeShaders()
    {
        if (_gl == null) return;

        // Simple vertex shader (2D positions with MVP transform)
        const string vertexShaderSource = @"
            #version 330 core
            layout (location = 0) in vec2 aPosition;
            layout (location = 1) in vec4 aColor;

            uniform mat4 uMVP;

            out vec4 vColor;

            void main()
            {
                gl_Position = uMVP * vec4(aPosition, 0.0, 1.0);
                vColor = aColor;
            }
        ";

        // Simple fragment shader (pass through color)
        const string fragmentShaderSource = @"
            #version 330 core
            in vec4 vColor;
            out vec4 FragColor;

            void main()
            {
                FragColor = vColor;
            }
        ";

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

        Console.WriteLine("Shaders compiled and linked successfully");
    }

    private void InitializeBuffers()
    {
        if (_gl == null) return;

        // Create VAO
        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        // Create VBO
        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        // Test data: simple triangle (position + color)
        float[] vertices = new float[]
        {
            // Position (x, y), Color (r, g, b, a)
             0.0f,  50.0f,  1.0f, 0.0f, 0.0f, 1.0f,  // Top (red)
            -50.0f, -50.0f,  0.0f, 1.0f, 0.0f, 1.0f,  // Bottom left (green)
             50.0f, -50.0f,  0.0f, 0.0f, 1.0f, 1.0f   // Bottom right (blue)
        };

        unsafe
        {
            fixed (float* v = vertices)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
            }
        }

        // Position attribute (location 0)
        unsafe
        {
            _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)0);
        }
        _gl.EnableVertexAttribArray(0);

        // Color attribute (location 1)
        unsafe
        {
            _gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)(2 * sizeof(float)));
        }
        _gl.EnableVertexAttribArray(1);

        _gl.BindVertexArray(0);

        Console.WriteLine("Buffers initialized");
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        if (_gl == null) return;

        // Clear the screen
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        // Use shader program
        _gl.UseProgram(_shaderProgram);

        // Create simple orthographic projection matrix (2D)
        // This is a basic implementation - will be replaced with proper camera later
        float aspect = (float)Bounds.Width / (float)Bounds.Height;
        float viewWidth = 200.0f * aspect / (float)_zoom;
        float viewHeight = 200.0f / (float)_zoom;

        // Simple orthographic projection (looking at origin)
        float[] mvp = CreateOrthographicMatrix(
            (float)_cameraX - viewWidth / 2,
            (float)_cameraX + viewWidth / 2,
            (float)_cameraY - viewHeight / 2,
            (float)_cameraY + viewHeight / 2
        );

        // Set MVP uniform
        int mvpLocation = _gl.GetUniformLocation(_shaderProgram, "uMVP");
        unsafe
        {
            fixed (float* m = mvp)
            {
                _gl.UniformMatrix4(_gl.GetUniformLocation(_shaderProgram, "uMVP"), 1, false, m);
            }
        }

        // Draw triangle
        _gl.BindVertexArray(_vao);
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

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        if (_gl != null)
        {
            _gl.DeleteBuffer(_vbo);
            _gl.DeleteVertexArray(_vao);
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
}
