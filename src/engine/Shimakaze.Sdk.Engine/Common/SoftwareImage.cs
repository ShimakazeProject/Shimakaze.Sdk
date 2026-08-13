using System.Collections.Immutable;
using System.Drawing;
using System.Runtime.InteropServices;

using Shimakaze.Sdk.Engine.Common.Pixels;

namespace Shimakaze.Sdk.Engine.Common;

/// <summary>
/// Represents an image backed by raw pixel data in software memory.
/// </summary>
/// <param name="Width">The width of the image in pixels.</param>
/// <param name="Height">The height of the image in pixels.</param>
/// <param name="Pixels">The raw pixel data as an immutable array of <see cref="RGBA32"/> values.</param>
public sealed record class SoftwareImage(int Width, int Height, ImmutableArray<RGBA32> Pixels)
    : Image(Width, Height)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SoftwareImage"/> class from a size and pixel array.
    /// </summary>
    /// <param name="size">The size of the image.</param>
    /// <param name="pixels">The raw pixel data.</param>
    internal SoftwareImage(Size size, RGBA32[] pixels) : this(size.Width, size.Height, ImmutableCollectionsMarshal.AsImmutableArray(pixels))
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SoftwareImage"/> class from width, height and pixel array.
    /// </summary>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <param name="pixels">The raw pixel data.</param>
    public SoftwareImage(int width, int height, RGBA32[] pixels) : this(width, height, ImmutableCollectionsMarshal.AsImmutableArray(pixels))
    { }

    /// <inheritdoc />
    public override RGBA32 GetPixel(int x, int y) => Pixels[(y * Width) + x];

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
    /// Generates a color palette from the image using median cut quantization.
    /// </summary>
    /// <param name="count">The maximum number of colors in the palette.</param>
    /// <param name="pixels">Optional pixel data to use instead of the image's own pixels.</param>
    /// <returns>An immutable array of <see cref="RGBA32"/> palette colors.</returns>
    private ImmutableArray<RGBA32> GetPalette(int count, ImmutableArray<RGBA32>? pixels = null)
    {
        var sourcePixels = pixels ?? Pixels;
        var uniqueColors = sourcePixels.Distinct().ToArray();
        
        if (uniqueColors.Length <= count)
        {
            return ImmutableCollectionsMarshal.AsImmutableArray(uniqueColors);
        }

        return MedianCutQuantize(uniqueColors, count);
    }

    /// <summary>
    /// Performs median cut color quantization to reduce colors to the specified count.
    /// </summary>
    private static ImmutableArray<RGBA32> MedianCutQuantize(RGBA32[] colors, int count)
    {
        var boxes = new List<ColorBox> { new(colors) };
        
        while (boxes.Count < count)
        {
            var largestBox = boxes.OrderByDescending(b => b.Volume).First();
            if (largestBox.Volume == 0 || largestBox.Colors.Length <= 1)
                break;
            
            boxes.Remove(largestBox);
            var (box1, box2) = largestBox.Split();
            boxes.Add(box1);
            boxes.Add(box2);
        }

        var palette = boxes.Select(b => b.AverageColor).ToArray();
        return ImmutableCollectionsMarshal.AsImmutableArray(palette);
    }

    /// <summary>
    /// Represents a color box for median cut quantization.
    /// </summary>
    private sealed class ColorBox(RGBA32[] colors)
    {
        public RGBA32[] Colors { get; } = colors;
        public int MinR { get; } = colors.Min(c => c.R);
        public int MaxR { get; } = colors.Max(c => c.R);
        public int MinG { get; } = colors.Min(c => c.G);
        public int MaxG { get; } = colors.Max(c => c.G);
        public int MinB { get; } = colors.Min(c => c.B);
        public int MaxB { get; } = colors.Max(c => c.B);
        public int MinA { get; } = colors.Min(c => c.A);
        public int MaxA { get; } = colors.Max(c => c.A);

        public int Volume => (MaxR - MinR) * (MaxG - MinG) * (MaxB - MinB) * (MaxA - MinA);
        
        public RGBA32 AverageColor
        {
            get
            {
                if (Colors.Length == 0) return new RGBA32(0, 0, 0, 0);
                long r = 0, g = 0, b = 0, a = 0;
                foreach (var c in Colors)
                {
                    r += c.R;
                    g += c.G;
                    b += c.B;
                    a += c.A;
                }
                return new RGBA32((byte)(r / Colors.Length), (byte)(g / Colors.Length), (byte)(b / Colors.Length), (byte)(a / Colors.Length));
            }
        }

        public (ColorBox, ColorBox) Split()
        {
            int rangeR = MaxR - MinR;
            int rangeG = MaxG - MinG;
            int rangeB = MaxB - MinB;
            int rangeA = MaxA - MinA;
            
            int maxRange = Math.Max(Math.Max(rangeR, rangeG), Math.Max(rangeB, rangeA));
            
            var sortedColors = maxRange switch
            {
                var r when r == rangeR => Colors.OrderBy(c => c.R).ToArray(),
                var g when g == rangeG => Colors.OrderBy(c => c.G).ToArray(),
                var b when b == rangeB => Colors.OrderBy(c => c.B).ToArray(),
                _ => Colors.OrderBy(c => c.A).ToArray(),
            };
            
            int mid = sortedColors.Length / 2;
            var box1Colors = new RGBA32[mid];
            var box2Colors = new RGBA32[sortedColors.Length - mid];
            Array.Copy(sortedColors, 0, box1Colors, 0, mid);
            Array.Copy(sortedColors, mid, box2Colors, 0, sortedColors.Length - mid);
            
            return (new ColorBox(box1Colors), new ColorBox(box2Colors));
        }
    }
}
