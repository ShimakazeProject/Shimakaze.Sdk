using System.Drawing;

namespace Shimakaze.Sdk.Engine.Cli.TUI.Components;

interface ITUIElement
{
    Size Measure(Size max);
    void OnRender(TextWriter writer, Size size);
}
