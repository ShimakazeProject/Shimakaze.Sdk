using System.Collections.Immutable;
using System.Runtime.InteropServices;

using Shimakaze.Sdk.Engine.Common.Pixels;

namespace Shimakaze.Sdk.Engine.Common;

internal sealed record class PaletteImage(int Width, int Height, ImmutableArray<BGRA32> Palette, ImmutableArray<byte> Indexes)
    : Image(Width, Height)
{
    public override BGRA32 GetPixel(int x, int y) => Palette[Indexes[(y * Width) + x]];

    public override PaletteImage ToPalette(int count)
    {
        if (Palette.Length <= count)
            return this;

        return ToSoftware().ToPalette(count);
    }

    public override SoftwareImage ToSoftware()
    {
        BGRA32[] pixels = GC.AllocateUninitializedArray<BGRA32>(Indexes.Length);
        for (int i = 0; i < Indexes.Length; i++)
            pixels[i] = Palette[Indexes[i]];

        return new(Width, Height, ImmutableCollectionsMarshal.AsImmutableArray(pixels));
    }
}
