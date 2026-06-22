using System.Drawing;
using System.Runtime.InteropServices;

using Shimakaze.Sdk.Engine.Common;
using Shimakaze.Sdk.Engine.Common.Pixels;

namespace Shimakaze.Sdk.Engine.Cli.TUI.Components.Images;

internal sealed class KittyImage : TrueImageElement
{
    private RGBA32[] _pixels = [];
    public override Image? Image
    {
        get => base.Image;
        set
        {
            base.Image = value;
            if (value is null)
                return;

            var width = value.Width;
            var height = value.Height;
            _pixels = GC.AllocateUninitializedArray<RGBA32>(width * height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    _pixels[y * width * x] = (RGBA32)value.GetPixel(x, y);
            }
        }
    }

    protected override void OnTrueRender(TextWriter writer, Size px)
    {
        if (Image is null)
            return;

        var width = px.Width;
        var height = px.Height;
        Span<RGBA32> colors = GC.AllocateUninitializedArray<RGBA32>(width * height);

        for (int y = 0; y < height; y++)
            _pixels.AsSpan(y * Image.Width, width).CopyTo(colors[(y * width)..]);

        ReadOnlySpan<char> base64 = Convert.ToBase64String(MemoryMarshal.AsBytes(colors));

        int blocks = base64.Length / 4096;
        if (base64.Length % 4096 is not 0)
            blocks++;
        if (blocks > 1)
        {
            var start = 0;
            int len = base64.Length;
            List<Range> ranges = [];
            while (len > 0)
            {
                var s = int.Min(len, 4096);
                ranges.Add(start..(start + s));
                start += s;
                len -= s;
            }

            for (int i = 0; i < ranges.Count; i++)
            {
                if (i is 0)
                    writer.Write($"\e_Gf=32,s={width},v={height},t=d,m=1;");
                else if (i + 1 != ranges.Count)
                    writer.Write($"\e_Gm=1;");
                else
                    writer.Write($"\e_Gm=0;");

                writer.Write(base64[ranges[i]]);

                writer.Write($"\e\\");
            }
        }
        else
        {
            writer.Write($"\e_Gf=32,s={width},v={height},t=d;");
            writer.Write(base64);
            writer.Write($"\e\\");
        }
    }
}
