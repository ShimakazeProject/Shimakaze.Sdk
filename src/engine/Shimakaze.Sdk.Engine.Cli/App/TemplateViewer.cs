using System.Drawing;

using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Engine.Cli.TUI;
using Shimakaze.Sdk.Engine.Cli.TUI.Components.Images;
using Shimakaze.Sdk.Engine.Common.Pixels;
using Shimakaze.Sdk.Engine.Tmp;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Tmp;

namespace Shimakaze.Sdk.Engine.Cli.App;

internal sealed class TemplateViewer(TemplateFile template, Palette palette) : NanoFramework
{
    private readonly TemplateRenderer _renderer = new(template, palette);
    private readonly ImageElement _image = ImageElement.Create();
    private BGRA32 _pal0;
    private bool _disposedValue;

    private bool UseTransparent
    {
        get;
        set
        {
            field = value;
            _renderer.Palette[0] = value ? BGRA32.Transparent : _pal0;
        }
    }

    protected override void OnInitialize()
    {
        _pal0 = _renderer.Palette[0];

        base.OnInitialize();

        _image.Image = _renderer.RenderAsImage();

        ShortKeyManager.Regist(0, new ShortKey(ConsoleKey.F5, ConsoleModifiers.None), Console.Clear);
        ShortKeyManager.Regist(0, new ShortKey(ConsoleKey.Escape, ConsoleModifiers.None), Exit);
        ShortKeyManager.Regist(0, new NamedShortKey(ConsoleKey.Q, ConsoleModifiers.None, Resource.TUI_ShpViewer_Exit), Exit);
        ShortKeyManager.Regist((int)ShapeViewerMode.Normal, new NamedSwitchShortKey(ConsoleKey.T, ConsoleModifiers.None, i => i ? Resource.TUI_ShpViewer_DisableTransparent : Resource.TUI_ShpViewer_EnableTransparent, () => UseTransparent), SwitchTransparent);
    }

    private void Exit()
    {
        Shutdown();
    }

    private void SwitchTransparent()
    {
        Console.Clear();
        UseTransparent = !UseTransparent;
    }

    public override void OnClientRender(TextWriter writer, Size size)
    {
        var imageSize = _image.Measure(size);
        var p = (size.Width - imageSize.Width) / 2;
        Console.SetCursorPosition(p, 1);
        writer.Write($"\e[{p + 1}G");
        _image.OnRender(writer, imageSize);
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;

        if (disposing)
        {
            _image.Dispose();
        }

        _disposedValue = true;
        base.Dispose(disposing);
    }
}
