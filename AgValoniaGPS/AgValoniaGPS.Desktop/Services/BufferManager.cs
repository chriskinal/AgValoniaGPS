using System;
using Silk.NET.OpenGL;

namespace AgValoniaGPS.Desktop.Services;

/// <summary>
/// Manages OpenGL buffer objects (VBOs, VAOs, EBOs) for efficient geometry rendering.
/// Provides convenient methods for buffer creation, updating, and configuration.
/// </summary>
public class BufferManager : IDisposable
{
    private readonly GL _gl;
    private bool _disposed;

    public BufferManager(GL gl)
    {
        _gl = gl ?? throw new ArgumentNullException(nameof(gl));
    }

    /// <summary>
    /// Creates a vertex buffer object (VBO) and uploads data.
    /// </summary>
    /// <param name="vertices">Vertex data array</param>
    /// <param name="usage">Buffer usage hint (Static, Dynamic, Stream)</param>
    /// <returns>VBO handle</returns>
    public uint CreateVertexBuffer(float[] vertices, BufferUsageARB usage)
    {
        if (vertices == null || vertices.Length == 0)
            throw new ArgumentException("Vertices array cannot be null or empty", nameof(vertices));

        uint vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

        unsafe
        {
            fixed (float* ptr = vertices)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), ptr, usage);
            }
        }

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        return vbo;
    }

    /// <summary>
    /// Creates an element buffer object (EBO/index buffer) and uploads data.
    /// </summary>
    /// <param name="indices">Index data array</param>
    /// <param name="usage">Buffer usage hint (Static, Dynamic, Stream)</param>
    /// <returns>EBO handle</returns>
    public uint CreateIndexBuffer(uint[] indices, BufferUsageARB usage)
    {
        if (indices == null || indices.Length == 0)
            throw new ArgumentException("Indices array cannot be null or empty", nameof(indices));

        uint ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

        unsafe
        {
            fixed (uint* ptr = indices)
            {
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), ptr, usage);
            }
        }

        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        return ebo;
    }

    /// <summary>
    /// Creates a vertex array object (VAO).
    /// </summary>
    /// <returns>VAO handle</returns>
    public uint CreateVertexArray()
    {
        return _gl.GenVertexArray();
    }

    /// <summary>
    /// Binds a vertex array object for configuration or rendering.
    /// </summary>
    /// <param name="vaoId">VAO handle (0 to unbind)</param>
    public void BindVertexArray(uint vaoId)
    {
        _gl.BindVertexArray(vaoId);
    }

    /// <summary>
    /// Updates data in an existing vertex buffer.
    /// </summary>
    /// <param name="vboId">VBO handle</param>
    /// <param name="vertices">New vertex data</param>
    /// <param name="offset">Offset in buffer (in floats, not bytes)</param>
    public void UpdateVertexBuffer(uint vboId, float[] vertices, int offset = 0)
    {
        if (vertices == null || vertices.Length == 0)
            throw new ArgumentException("Vertices array cannot be null or empty", nameof(vertices));

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, vboId);

        unsafe
        {
            fixed (float* ptr = vertices)
            {
                _gl.BufferSubData(BufferTargetARB.ArrayBuffer, offset * sizeof(float),
                    (nuint)(vertices.Length * sizeof(float)), ptr);
            }
        }

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
    }

    /// <summary>
    /// Configures a vertex attribute pointer.
    /// </summary>
    /// <param name="location">Attribute location (from shader)</param>
    /// <param name="size">Number of components per attribute (1-4)</param>
    /// <param name="type">Data type of components</param>
    /// <param name="stride">Byte offset between consecutive attributes</param>
    /// <param name="offset">Byte offset of first component</param>
    public void ConfigureVertexAttribute(int location, int size, VertexAttribPointerType type,
        int stride, int offset)
    {
        unsafe
        {
            _gl.VertexAttribPointer((uint)location, size, type, false, (uint)stride, (void*)offset);
        }
        _gl.EnableVertexAttribArray((uint)location);
    }

    /// <summary>
    /// Deletes a buffer object.
    /// </summary>
    /// <param name="bufferId">Buffer handle to delete</param>
    public void DeleteBuffer(uint bufferId)
    {
        _gl.DeleteBuffer(bufferId);
    }

    /// <summary>
    /// Deletes a vertex array object.
    /// </summary>
    /// <param name="vaoId">VAO handle to delete</param>
    public void DeleteVertexArray(uint vaoId)
    {
        _gl.DeleteVertexArray(vaoId);
    }

    public void Dispose()
    {
        if (_disposed) return;

        // Note: Individual buffers/VAOs should be deleted explicitly before disposing manager
        // This is just cleanup
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
