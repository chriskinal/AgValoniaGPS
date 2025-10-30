using System;
using System.Collections.Generic;
using System.Numerics;
using Silk.NET.OpenGL;

namespace AgValoniaGPS.Desktop.Services;

/// <summary>
/// Manages GLSL shader compilation, linking, and uniform setting.
/// Provides a shader program cache for efficient shader reuse.
/// </summary>
public class ShaderManager : IDisposable
{
    private readonly GL _gl;
    private readonly Dictionary<string, ShaderProgram> _programs = new();
    private ShaderProgram? _activeProgram;

    public ShaderManager(GL gl)
    {
        _gl = gl ?? throw new ArgumentNullException(nameof(gl));
    }

    /// <summary>
    /// Loads and compiles a shader program from vertex and fragment shader source.
    /// </summary>
    /// <param name="name">Unique name for the shader program</param>
    /// <param name="vertexShaderSource">Vertex shader GLSL source code</param>
    /// <param name="fragmentShaderSource">Fragment shader GLSL source code</param>
    /// <returns>Compiled shader program</returns>
    public ShaderProgram LoadProgram(string name, string vertexShaderSource, string fragmentShaderSource)
    {
        if (_programs.ContainsKey(name))
        {
            return _programs[name];
        }

        var program = new ShaderProgram(_gl, vertexShaderSource, fragmentShaderSource);
        _programs[name] = program;
        return program;
    }

    /// <summary>
    /// Gets a previously loaded shader program by name.
    /// </summary>
    public ShaderProgram? GetProgram(string name)
    {
        return _programs.TryGetValue(name, out var program) ? program : null;
    }

    /// <summary>
    /// Activates a shader program for rendering.
    /// </summary>
    public void UseProgram(string name)
    {
        if (_programs.TryGetValue(name, out var program))
        {
            program.Use();
            _activeProgram = program;
        }
    }

    /// <summary>
    /// Sets a mat4 uniform on the active shader program.
    /// </summary>
    public void SetUniform(string name, Matrix4x4 value)
    {
        _activeProgram?.SetUniform(name, value);
    }

    /// <summary>
    /// Sets a vec4 uniform on the active shader program.
    /// </summary>
    public void SetUniform(string name, Vector4 value)
    {
        _activeProgram?.SetUniform(name, value);
    }

    /// <summary>
    /// Sets a float uniform on the active shader program.
    /// </summary>
    public void SetUniform(string name, float value)
    {
        _activeProgram?.SetUniform(name, value);
    }

    public void Dispose()
    {
        foreach (var program in _programs.Values)
        {
            program.Dispose();
        }
        _programs.Clear();
    }
}

/// <summary>
/// Represents a compiled and linked GLSL shader program.
/// </summary>
public class ShaderProgram : IDisposable
{
    private readonly GL _gl;
    private readonly uint _programId;
    private readonly uint _vertexShaderId;
    private readonly uint _fragmentShaderId;
    private bool _disposed;

    public uint ProgramId => _programId;

    public ShaderProgram(GL gl, string vertexShaderSource, string fragmentShaderSource)
    {
        _gl = gl ?? throw new ArgumentNullException(nameof(gl));

        // Compile vertex shader
        _vertexShaderId = CompileShader(ShaderType.VertexShader, vertexShaderSource);

        // Compile fragment shader
        _fragmentShaderId = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);

        // Link program
        _programId = LinkProgram(_vertexShaderId, _fragmentShaderId);
    }

    private uint CompileShader(ShaderType type, string source)
    {
        uint shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);

        // Check compilation status
        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
        if (status != (int)GLEnum.True)
        {
            string log = _gl.GetShaderInfoLog(shader);
            throw new Exception($"{type} shader compilation failed: {log}");
        }

        return shader;
    }

    private uint LinkProgram(uint vertexShader, uint fragmentShader)
    {
        uint program = _gl.CreateProgram();
        _gl.AttachShader(program, vertexShader);
        _gl.AttachShader(program, fragmentShader);
        _gl.LinkProgram(program);

        // Check link status
        _gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int status);
        if (status != (int)GLEnum.True)
        {
            string log = _gl.GetProgramInfoLog(program);
            throw new Exception($"Shader program linking failed: {log}");
        }

        return program;
    }

    public void Use()
    {
        _gl.UseProgram(_programId);
    }

    public int GetUniformLocation(string name)
    {
        return _gl.GetUniformLocation(_programId, name);
    }

    public int GetAttributeLocation(string name)
    {
        return _gl.GetAttribLocation(_programId, name);
    }

    public void SetUniform(string name, Matrix4x4 value)
    {
        int location = GetUniformLocation(name);
        if (location >= 0)
        {
            unsafe
            {
                _gl.UniformMatrix4(location, 1, false, (float*)&value);
            }
        }
    }

    public void SetUniform(string name, Vector4 value)
    {
        int location = GetUniformLocation(name);
        if (location >= 0)
        {
            _gl.Uniform4(location, value.X, value.Y, value.Z, value.W);
        }
    }

    public void SetUniform(string name, float value)
    {
        int location = GetUniformLocation(name);
        if (location >= 0)
        {
            _gl.Uniform1(location, value);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _gl.DeleteShader(_vertexShaderId);
        _gl.DeleteShader(_fragmentShaderId);
        _gl.DeleteProgram(_programId);

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
