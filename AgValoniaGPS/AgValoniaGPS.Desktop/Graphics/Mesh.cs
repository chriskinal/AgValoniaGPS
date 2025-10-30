using System;
using Silk.NET.OpenGL;

namespace AgValoniaGPS.Desktop.Graphics;

/// <summary>
/// Represents a 3D mesh with vertex data and optional indices.
/// Manages VAO, VBO, and EBO for efficient GPU rendering.
/// </summary>
public class Mesh : IDisposable
{
    private readonly GL _gl;
    private uint _vao;
    private uint _vbo;
    private uint _ebo;
    private int _vertexCount;
    private int _indexCount;
    private bool _hasIndices;
    private bool _disposed;
    private PrimitiveType _primitiveType;

    public uint VAO => _vao;
    public int VertexCount => _vertexCount;
    public int IndexCount => _indexCount;
    public bool HasIndices => _hasIndices;
    public PrimitiveType PrimitiveType => _primitiveType;

    public Mesh(GL gl)
    {
        _gl = gl ?? throw new ArgumentNullException(nameof(gl));
        _primitiveType = PrimitiveType.Triangles;

        // Generate buffers
        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();
    }

    /// <summary>
    /// Sets up the mesh with interleaved vertex data.
    /// Format: position (3 floats), normal (3 floats), color (4 floats), texCoord (2 floats)
    /// Total: 12 floats per vertex
    /// </summary>
    public unsafe void SetVertexData(float[] vertices, uint[]? indices = null, PrimitiveType primitiveType = PrimitiveType.Triangles)
    {
        _primitiveType = primitiveType;
        _vertexCount = vertices.Length / 12; // 12 floats per vertex
        _hasIndices = indices != null && indices.Length > 0;
        _indexCount = indices?.Length ?? 0;

        _gl.BindVertexArray(_vao);

        // Upload vertex data
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (float* v = vertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
        }

        // Set up vertex attributes (interleaved)
        int stride = 12 * sizeof(float);

        // Position attribute (location 0)
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)stride, (void*)0);
        _gl.EnableVertexAttribArray(0);

        // Normal attribute (location 1)
        _gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, (uint)stride, (void*)(3 * sizeof(float)));
        _gl.EnableVertexAttribArray(1);

        // Color attribute (location 2)
        _gl.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, (uint)stride, (void*)(6 * sizeof(float)));
        _gl.EnableVertexAttribArray(2);

        // TexCoord attribute (location 3)
        _gl.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, (uint)stride, (void*)(10 * sizeof(float)));
        _gl.EnableVertexAttribArray(3);

        // Upload indices if provided
        if (_hasIndices)
        {
            if (_ebo == 0)
            {
                _ebo = _gl.GenBuffer();
            }

            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
            fixed (uint* i = indices)
            {
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);
            }
        }

        _gl.BindVertexArray(0);
    }

    /// <summary>
    /// Updates vertex data dynamically (for animated/moving objects).
    /// </summary>
    public unsafe void UpdateVertexData(float[] vertices)
    {
        _vertexCount = vertices.Length / 12;

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (float* v = vertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.DynamicDraw);
        }
    }

    /// <summary>
    /// Renders the mesh using the currently active shader.
    /// </summary>
    public unsafe void Draw()
    {
        _gl.BindVertexArray(_vao);

        if (_hasIndices)
        {
            _gl.DrawElements(_primitiveType, (uint)_indexCount, DrawElementsType.UnsignedInt, null);
        }
        else
        {
            _gl.DrawArrays(_primitiveType, 0, (uint)_vertexCount);
        }

        _gl.BindVertexArray(0);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_vao != 0)
            {
                _gl.DeleteVertexArray(_vao);
                _vao = 0;
            }
            if (_vbo != 0)
            {
                _gl.DeleteBuffer(_vbo);
                _vbo = 0;
            }
            if (_ebo != 0)
            {
                _gl.DeleteBuffer(_ebo);
                _ebo = 0;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
