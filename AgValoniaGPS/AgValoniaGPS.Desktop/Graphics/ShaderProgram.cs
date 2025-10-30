using System;
using System.IO;
using System.Numerics;
using Silk.NET.OpenGL;

namespace AgValoniaGPS.Desktop.Graphics;

/// <summary>
/// Manages an OpenGL shader program with vertex and fragment shaders.
/// Provides methods for compilation, linking, and setting uniforms.
/// </summary>
public class ShaderProgram : IDisposable
{
    private readonly GL _gl;
    private uint _programId;
    private uint _vertexShaderId;
    private uint _fragmentShaderId;
    private bool _disposed;

    public uint ProgramId => _programId;

    public ShaderProgram(GL gl)
    {
        _gl = gl ?? throw new ArgumentNullException(nameof(gl));
    }

    /// <summary>
    /// Loads and compiles shaders from files.
    /// </summary>
    public void LoadFromFiles(string vertexPath, string fragmentPath)
    {
        string vertexSource = File.ReadAllText(vertexPath);
        string fragmentSource = File.ReadAllText(fragmentPath);

        CompileShaders(vertexSource, fragmentSource);
    }

    /// <summary>
    /// Compiles shaders from source strings.
    /// </summary>
    public void CompileShaders(string vertexSource, string fragmentSource)
    {
        // Compile vertex shader
        _vertexShaderId = CompileShader(ShaderType.VertexShader, vertexSource);

        // Compile fragment shader
        _fragmentShaderId = CompileShader(ShaderType.FragmentShader, fragmentSource);

        // Link program
        LinkProgram();
    }

    private uint CompileShader(ShaderType type, string source)
    {
        uint shaderId = _gl.CreateShader(type);
        _gl.ShaderSource(shaderId, source);
        _gl.CompileShader(shaderId);

        // Check compilation status
        _gl.GetShader(shaderId, ShaderParameterName.CompileStatus, out int status);
        if (status != (int)GLEnum.True)
        {
            string log = _gl.GetShaderInfoLog(shaderId);
            _gl.DeleteShader(shaderId);
            throw new Exception($"{type} compilation failed: {log}");
        }

        return shaderId;
    }

    private void LinkProgram()
    {
        _programId = _gl.CreateProgram();
        _gl.AttachShader(_programId, _vertexShaderId);
        _gl.AttachShader(_programId, _fragmentShaderId);
        _gl.LinkProgram(_programId);

        // Check linking status
        _gl.GetProgram(_programId, ProgramPropertyARB.LinkStatus, out int status);
        if (status != (int)GLEnum.True)
        {
            string log = _gl.GetProgramInfoLog(_programId);
            throw new Exception($"Shader program linking failed: {log}");
        }

        // Shaders are no longer needed after linking
        _gl.DetachShader(_programId, _vertexShaderId);
        _gl.DetachShader(_programId, _fragmentShaderId);
        _gl.DeleteShader(_vertexShaderId);
        _gl.DeleteShader(_fragmentShaderId);
    }

    /// <summary>
    /// Activates this shader program for rendering.
    /// </summary>
    public void Use()
    {
        _gl.UseProgram(_programId);
    }

    // Uniform setters
    public void SetUniform(string name, int value)
    {
        int location = _gl.GetUniformLocation(_programId, name);
        if (location >= 0)
        {
            _gl.Uniform1(location, value);
        }
    }

    public void SetUniform(string name, float value)
    {
        int location = _gl.GetUniformLocation(_programId, name);
        if (location >= 0)
        {
            _gl.Uniform1(location, value);
        }
    }

    public void SetUniform(string name, bool value)
    {
        int location = _gl.GetUniformLocation(_programId, name);
        if (location >= 0)
        {
            _gl.Uniform1(location, value ? 1 : 0);
        }
    }

    public void SetUniform(string name, Vector2 value)
    {
        int location = _gl.GetUniformLocation(_programId, name);
        if (location >= 0)
        {
            _gl.Uniform2(location, value.X, value.Y);
        }
    }

    public void SetUniform(string name, Vector3 value)
    {
        int location = _gl.GetUniformLocation(_programId, name);
        if (location >= 0)
        {
            _gl.Uniform3(location, value.X, value.Y, value.Z);
        }
    }

    public void SetUniform(string name, Vector4 value)
    {
        int location = _gl.GetUniformLocation(_programId, name);
        if (location >= 0)
        {
            _gl.Uniform4(location, value.X, value.Y, value.Z, value.W);
        }
    }

    public unsafe void SetUniform(string name, Matrix4x4 value)
    {
        int location = _gl.GetUniformLocation(_programId, name);
        if (location >= 0)
        {
            // Matrix4x4 in .NET is row-major, OpenGL expects column-major
            // Transpose when uploading
            var transposed = Matrix4x4.Transpose(value);
            _gl.UniformMatrix4(location, 1, false, (float*)&transposed);
        }
    }

    public unsafe void SetUniformMat3(string name, Matrix4x4 matrix)
    {
        int location = _gl.GetUniformLocation(_programId, name);
        if (location >= 0)
        {
            // Extract 3x3 from 4x4 matrix (upper-left corner)
            float[] mat3 = new float[9]
            {
                matrix.M11, matrix.M12, matrix.M13,
                matrix.M21, matrix.M22, matrix.M23,
                matrix.M31, matrix.M32, matrix.M33
            };

            fixed (float* ptr = mat3)
            {
                _gl.UniformMatrix3(location, 1, false, ptr);
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_programId != 0)
            {
                _gl.DeleteProgram(_programId);
                _programId = 0;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
