using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

using Shimakaze.Sdk;
using Shimakaze.Sdk.Engine.Common.Pixels;

namespace Shimakaze.Sdk.Engine.Common;

/// <summary>
/// Represents an abstract image with width and height dimensions.
/// </summary>
/// <param name="Width">The width of the image in pixels.</param>
/// <param name="Height">The height of the image in pixels.</param>
public abstract record class Image(int Width, int Height)
{
    /// <summary>
    /// Gets the pixel color at the specified coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate of the pixel.</param>
    /// <param name="y">The y-coordinate of the pixel.</param>
    /// <returns>The <see cref="BGRA32"/> color value at the specified pixel.</returns>
    public abstract BGRA32 GetPixel(int x, int y);

    /// <summary>
    /// Loads an image from the specified file path using ImageMagick.
    /// </summary>
    /// <param name="path">The file path of the image to load.</param>
    /// <returns>A <see cref="SoftwareImage"/> instance containing the loaded image data.</returns>
    /// <exception cref="InvalidOperationException">Thrown when ImageMagick cannot be started.</exception>
    public static SoftwareImage Load(string path)
    {
        ProcessStartInfo info = new()
        {
            FileName = "magick",
            Arguments = $"identify -format \"%w:%h\" \"{path}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        ProcessStartInfo data = new()
        {
            FileName = "magick",
            Arguments = $"\"{path}\" -depth 8 BGRA:-",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var pInfo = Process.Start(info) ?? throw new InvalidOperationException("Cannot start magick");
        Debug.WriteLine(pInfo.StartInfo.Arguments);

        using var pData = Process.Start(data) ?? throw new InvalidOperationException("Cannot start magick");
        Debug.WriteLine(pData.StartInfo.Arguments);

        using MemoryStream ms = new();

        string[] output = pInfo.StandardOutput.ReadToEnd().Split(':', 2);
        pData.StandardOutput.BaseStream.CopyTo(ms);

        pInfo.WaitForExit();
        Debug.Assert(pInfo.ExitCode is 0);

        pData.WaitForExit();
        Debug.Assert(pData.ExitCode is 0);

        int w = int.Parse(output[0], CultureInfo.InvariantCulture);
        int h = int.Parse(output[1], CultureInfo.InvariantCulture);

        var pixels = MemoryMarshal.Cast<byte, BGRA32>(ms.GetBuffer()).ToArray();

        return new(w, h, ImmutableCollectionsMarshal.AsImmutableArray(pixels));
    }

    /// <summary>
    /// Saves the image to the specified file path using ImageMagick.
    /// </summary>
    /// <param name="path">The destination file path for the image.</param>
    /// <param name="arguments">Additional arguments to pass to ImageMagick.</param>
    /// <exception cref="InvalidOperationException">Thrown when ImageMagick cannot be started.</exception>
    public void SaveTo(string path, string arguments = "")
    {
        var image = ToSoftware();

        ProcessStartInfo data = new()
        {
            FileName = "magick",
            Arguments = $"-size {Width}x{Height} -depth 8 BGRA:- {arguments} \"{path}\"",
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var pData = Process.Start(data) ?? throw new InvalidOperationException("Cannot start magick");
        Debug.WriteLine(pData.StartInfo.Arguments);

        var stream = pData.StandardInput.BaseStream;
        stream.Write(image.Pixels.AsSpan());
        stream.Flush();
        stream.Close();

        pData.WaitForExit();

        Debug.Assert(pData.ExitCode is 0);
    }

    /// <summary>
    /// Converts the image to a <see cref="SoftwareImage"/> representation.
    /// </summary>
    /// <returns>A <see cref="SoftwareImage"/> containing the raw pixel data.</returns>
    public abstract SoftwareImage ToSoftware();

    /// <summary>
    /// Converts the image to a palette-based <see cref="PaletteImage"/> with the specified color count.
    /// </summary>
    /// <param name="count">The maximum number of colors in the palette.</param>
    /// <returns>A <see cref="PaletteImage"/> using a palette with at most <paramref name="count"/> colors.</returns>
    public abstract PaletteImage ToPalette(int count);
}
