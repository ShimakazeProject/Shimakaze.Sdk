using System.Collections.Immutable;
using System.Runtime.InteropServices;

using Shimakaze.Sdk.Engine.Common.Pixels;

namespace Shimakaze.Sdk.Engine.Common;

/// <summary>
/// Represents an image that uses a color palette and index-based pixel data.
/// </summary>
/// <param name="Width">The width of the image in pixels.</param>
/// <param name="Height">The height of the image in pixels.</param>
/// <param name="Palette">The color palette as an array of <see cref="RGBA32"/> values.</param>
/// <param name="Indexes">The palette index for each pixel.</param>
public sealed record class PaletteImage(int Width, int Height, ImmutableArray<RGBA32> Palette, ImmutableArray<byte> Indexes)
    : Image(Width, Height)
{
    /// <inheritdoc />
    public override RGBA32 GetPixel(int x, int y) => Palette[Indexes[(y * Width) + x]];

    /// <inheritdoc />
    public override PaletteImage ToPalette(int count)
    {
        if (Palette.Length <= count)
            return this;

        return ToSoftware().ToPalette(count);
    }

    /// <inheritdoc />
    public override SoftwareImage ToSoftware()
    {
        RGBA32[] pixels = GC.AllocateUninitializedArray<RGBA32>(Indexes.Length);
        for (int i = 0; i < Indexes.Length; i++)
            pixels[i] = Palette[Indexes[i]];

        return new(Width, Height, ImmutableCollectionsMarshal.AsImmutableArray(pixels));
    }
}
