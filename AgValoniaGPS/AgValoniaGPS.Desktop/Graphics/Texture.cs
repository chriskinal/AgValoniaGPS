using System;
using System.IO;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace AgValoniaGPS.Desktop.Graphics;

/// <summary>
/// Manages OpenGL texture loading and binding.
/// </summary>
public class Texture : IDisposable
{
    private readonly GL _gl;
    private uint _handle;
    private bool _disposed;

    public uint Handle => _handle;
    public int Width { get; private set; }
    public int Height { get; private set; }

    public Texture(GL gl, string filePath)
    {
        _gl = gl ?? throw new ArgumentNullException(nameof(gl));

        // Load image using StbImageSharp
        StbImage.stbi_set_flip_vertically_on_load(1); // OpenGL expects bottom-left origin

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Texture file not found: {filePath}");

        using var stream = File.OpenRead(filePath);
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        Width = image.Width;
        Height = image.Height;

        // Generate and bind texture
        _handle = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _handle);

        // Upload texture data
        unsafe
        {
            fixed (byte* ptr = image.Data)
            {
                _gl.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    InternalFormat.Rgba,
                    (uint)Width,
                    (uint)Height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    ptr
                );
            }
        }

        // Set texture parameters
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);

        // Generate mipmaps
        _gl.GenerateMipmap(TextureTarget.Texture2D);

        // Unbind
        _gl.BindTexture(TextureTarget.Texture2D, 0);

        Console.WriteLine($"[Texture] Loaded texture from {filePath}: {Width}x{Height}");
    }

    public void Bind(TextureUnit unit = TextureUnit.Texture0)
    {
        _gl.ActiveTexture(unit);
        _gl.BindTexture(TextureTarget.Texture2D, _handle);
    }

    public void Unbind()
    {
        _gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Dispose()
    {
        if (_disposed) return;

        _gl.DeleteTexture(_handle);
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
