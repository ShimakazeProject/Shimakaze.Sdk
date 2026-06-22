using System.Drawing;

using Shimakaze.Sdk.Engine.Cli.Sixel;
using Shimakaze.Sdk.Engine.Common;

namespace Shimakaze.Sdk.Engine.Cli.TUI.Components.Images;

internal sealed class SixelImage : TrueImageElement
{
    public override Image? Image
    {
        get => base.Image;
        set => base.Image = value?.ToPalette(256);
    }

    public override void OnRender(TextWriter writer, Size size)
    {
        if (Image is not PaletteImage paletted)
            throw new NotSupportedException();

        if (paletted.Palette.Any(i => i.A is 0))
        {
            for (int i = 0; i < size.Height; i++)
            {
                writer.Write($"\e[{size.Width}X");
                writer.Write($"\e[B");
            }
            writer.Write($"\e[{size.Height}A");
        }
        base.OnRender(writer, size);
    }

    protected override void OnTrueRender(TextWriter writer, Size px)
    {
        if (Image is null)
            return;

        if (Image is not PaletteImage paletted)
            throw new NotSupportedException();

        using SixelWriter sixel = new(writer, true);

        var width = px.Width;
        var height = px.Height;
        sixel.Begin(width, height);
        int transparent = -1;
        for (int i = 0; i < paletted.Palette.Length; i++)
        {
            if (paletted.Palette[i] is { A: not 0 } c)
                sixel.RegistColor((byte)i, c);
            else
                transparent = i;
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte index = paletted.Indexes[(y * Image.Width) + x];
                sixel.WritePixel(index == transparent ? null : index, 1);
            }

            sixel.NewLine();
        }
        sixel.End();
    }
}
