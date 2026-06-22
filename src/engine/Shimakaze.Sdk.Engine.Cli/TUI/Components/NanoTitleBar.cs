using System.Drawing;

namespace Shimakaze.Sdk.Engine.Cli.TUI.Components;

internal class NanoTitleBar : ITUIElement
{
    public string? Left { get; set; }
    public string? Center { get; set; }
    public string? Right { get; set; }

    public Size Measure(Size max) => new(max.Width, 1);

    public void OnRender(TextWriter writer, Size size)
    {
        var width = size.Width;
        writer.Write("\e[7m");
        for (int i = 0; i < width; i++)
            writer.Write(' ');

        if (!string.IsNullOrWhiteSpace(Left))
        {
            writer.Write("\e[3G");
            writer.Write(Left);
        }

        if (!string.IsNullOrWhiteSpace(Center))
        {
            var start = (width - NanoFramework.GetDisplayWidth(Center)) / 2;
            writer.Write($"\e[{start + 1}G");
            writer.Write(Center);
        }

        if (!string.IsNullOrWhiteSpace(Right))
        {
            var start = width - NanoFramework.GetDisplayWidth(Right) - 2;
            writer.Write($"\e[{start + 1}G");
            writer.Write(Right);
        }

        writer.Write("\e[0m");
    }
}
