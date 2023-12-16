using Shimakaze.Sdk.Graphic.Pixel;

namespace Shimakaze.Sdk.Graphic.Pal;

internal static class Rgb24Extensions
{
    public static Rgb24 AsNormalColor(this Rgb24 pixel) => new(
        unchecked((byte)(pixel.Red << 2)),
        unchecked((byte)(pixel.Green << 2)),
        unchecked((byte)(pixel.Blue << 2)));
}