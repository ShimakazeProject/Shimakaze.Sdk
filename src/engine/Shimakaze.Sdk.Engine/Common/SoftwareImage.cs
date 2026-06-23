using System.Collections.Immutable;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

using Shimakaze.Sdk.Engine.Common.Pixels;

namespace Shimakaze.Sdk.Engine.Common;

/// <summary>
/// Represents an image backed by raw pixel data in software memory.
/// </summary>
/// <param name="Width">The width of the image in pixels.</param>
/// <param name="Height">The height of the image in pixels.</param>
/// <param name="Pixels">The raw pixel data as an immutable array of <see cref="BGRA32"/> values.</param>
public sealed record class SoftwareImage(int Width, int Height, ImmutableArray<BGRA32> Pixels)
    : Image(Width, Height)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SoftwareImage"/> class from a size and pixel array.
    /// </summary>
    /// <param name="size">The size of the image.</param>
    /// <param name="pixels">The raw pixel data.</param>
    internal SoftwareImage(Size size, BGRA32[] pixels) : this(size.Width, size.Height, ImmutableCollectionsMarshal.AsImmutableArray(pixels))
    { }

    /// <inheritdoc />
    public override BGRA32 GetPixel(int x, int y) => Pixels[(y * Width) + x];

    /// <inheritdoc />
    public override PaletteImage ToPalette(int count)
    {
        var colors = GetPalette(count);
        var palette = GetPalette(count, colors);
        var indexes = GC.AllocateUninitializedArray<byte>(colors.Length);
        for (int i = 0; i < colors.Length; i++)
            indexes[i] = (byte)palette.IndexOf(colors[i]);

        return new(Width, Height, palette, ImmutableCollectionsMarshal.AsImmutableArray(indexes));
    }

    /// <inheritdoc />
    public override SoftwareImage ToSoftware() => this;

    /// <summary>
    /// Generates a color palette from the image using ImageMagick with optional quantized colors.
    /// </summary>
    /// <param name="count">The maximum number of colors in the palette.</param>
    /// <param name="pixels">Optional pixel data to use instead of the image's own pixels.</param>
    /// <returns>An immutable array of <see cref="BGRA32"/> palette colors.</returns>
    private ImmutableArray<BGRA32> GetPalette(int count, ImmutableArray<BGRA32>? pixels = null)
    {
        ProcessStartInfo data = new()
        {
            FileName = "magick",
            Arguments = $"-size {Width}x{Height} -depth 8 BGRA:- -colors {count} {(pixels is not null ? "-unique-colors" : string.Empty)} BGRA:-",
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var pData = Process.Start(data) ?? throw new InvalidOperationException("Cannot start magick");
        Debug.WriteLine(pData.StartInfo.Arguments);

        var stream = pData.StandardInput.BaseStream;
        stream.Write((pixels ?? Pixels).AsSpan());
        stream.Flush();
        stream.Close();

        using MemoryStream ms = new();
        pData.StandardOutput.BaseStream.CopyTo(ms);

        pData.WaitForExit();

        Debug.Assert(pData.ExitCode is 0);

        var result = MemoryMarshal.Cast<byte, BGRA32>(ms.GetBuffer()).ToArray();

        return ImmutableCollectionsMarshal.AsImmutableArray(result);
    }
}
