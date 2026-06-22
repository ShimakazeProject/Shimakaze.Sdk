using System.Collections.Immutable;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

using Shimakaze.Sdk.Engine.Common.Pixels;

namespace Shimakaze.Sdk.Engine.Common;

internal sealed record class SoftwareImage(int Width, int Height, ImmutableArray<BGRA32> Pixels)
    : Image(Width, Height)
{
    internal SoftwareImage(Size size, BGRA32[] pixels) : this(size.Width, size.Height, ImmutableCollectionsMarshal.AsImmutableArray(pixels))
    { }

    public override BGRA32 GetPixel(int x, int y) => Pixels[(y * Width) + x];

    public override PaletteImage ToPalette(int count)
    {
        var colors = GetPalette(count);
        var palette = GetPalette(count, colors);
        var indexes = GC.AllocateUninitializedArray<byte>(colors.Length);
        for (int i = 0; i < colors.Length; i++)
            indexes[i] = (byte)palette.IndexOf(colors[i]);

        return new(Width, Height, palette, ImmutableCollectionsMarshal.AsImmutableArray(indexes));
    }

    public override SoftwareImage ToSoftware() => this;

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
