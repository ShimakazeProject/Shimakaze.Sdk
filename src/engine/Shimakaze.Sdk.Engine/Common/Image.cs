using Shimakaze.Sdk.Engine.Common.Pixels;

using StbImageSharp;

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
    /// <returns>The <see cref="RGBA32"/> color value at the specified pixel.</returns>
    public abstract RGBA32 GetPixel(int x, int y);

    /// <summary>
    /// Loads an image from the specified file path using StbImageSharp.
    /// </summary>
    /// <param name="path">The file path of the image to load.</param>
    /// <returns>A <see cref="SoftwareImage"/> instance containing the loaded image data.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the image cannot be loaded.</exception>
    public static SoftwareImage Load(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        var result = ImageResult.FromMemory(bytes, ColorComponents.RedGreenBlueAlpha);

        if (result.Data == null)
        {
            throw new InvalidOperationException($"Failed to load image from {path}");
        }

        var pixels = new RGBA32[result.Width * result.Height];
        for (int i = 0; i < result.Data.Length; i += 4)
        {
            int index = i / 4;
            pixels[index] = new RGBA32(result.Data[i], result.Data[i + 1], result.Data[i + 2], result.Data[i + 3]);
        }

        return new SoftwareImage(result.Width, result.Height, pixels);
    }

    /// <summary>
    /// Saves the image to the specified file path as PNG using StbImageWriteSharp.
    /// </summary>
    /// <param name="path">The destination file path for the image.</param>
    /// <param name="arguments">Additional arguments (not used with StbImageWriteSharp).</param>
    /// <exception cref="InvalidOperationException">Thrown when the image cannot be saved.</exception>
    public void SaveTo(string path, string arguments = "")
    {
        var image = ToSoftware();
        byte[] bytes = new byte[image.Width * image.Height * 4];

        for (int i = 0; i < image.Pixels.Length; i++)
        {
            var pixel = image.Pixels[i];
            int offset = i * 4;
            bytes[offset] = pixel.R;
            bytes[offset + 1] = pixel.G;
            bytes[offset + 2] = pixel.B;
            bytes[offset + 3] = pixel.A;
        }

        using var stream = File.OpenWrite(path);
        var writer = new StbImageWriteSharp.ImageWriter();
        writer.WritePng(bytes, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
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
