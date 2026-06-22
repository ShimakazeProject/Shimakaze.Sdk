using System.Drawing;

namespace Shimakaze.Sdk.Engine.Cli.TUI.Components.Images;

internal abstract class TrueImageElement : ImageElement
{
    public override Size Measure(Size max)
    {
        if (Image is not { } image)
            return max;

        var minw = image.Width / TerminalImageSupport.CellSize.Width;
        if (image.Width % TerminalImageSupport.CellSize.Width is not 0)
            minw++;
        var minh = image.Height / TerminalImageSupport.CellSize.Height;
        if (image.Height % TerminalImageSupport.CellSize.Height is not 0)
            minh++;

        return new(int.Min(minw, max.Width), int.Min(minh, max.Height));
    }

    public override void OnRender(TextWriter writer, Size size)
    {
        if (Image is not { } image)
            return;

        Size px = new(int.Min(image.Width, size.Width * TerminalImageSupport.CellSize.Width), int.Min(image.Height, size.Height * TerminalImageSupport.CellSize.Height));
        OnTrueRender(writer, px);
    }

    protected abstract void OnTrueRender(TextWriter writer, Size px);
}
