using System.Text;

using Shimakaze.Sdk.Engine.Cli.Components;
using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Engine.Tmp;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Tmp;

namespace Shimakaze.Sdk.Engine.Cli.TUI.App;

internal sealed class TmpViewer : Application
{
    private readonly StringBuilder _buffer;
    private readonly StringWriter _writer;
    private readonly TmpRenderer _renderer;
    private readonly SixelImage _sixel = new() { Center = true };
    private bool _disposedValue;

    public TmpViewer(TemplateFile template, Palette palette)
    {
        _buffer = new();
        _writer = new(_buffer);

        _renderer = new(template, palette);
        _sixel.SetImage(_renderer.RenderAsImage());
    }

    protected override void OnEvent(EventArgs eventArgs)
    {
        if (eventArgs is ConsoleKeyEventArgs keyEventArgs)
            OnKeyEvent(keyEventArgs.KeyInfo);
    }

    private void OnKeyEvent(ConsoleKeyInfo key)
    {
        switch (key)
        {
            case { Key: ConsoleKey.F5 }:
                Console.Clear();
                break;
            case { Key: ConsoleKey.Escape }:
            case { Key: ConsoleKey.Q }:
            case { Key: ConsoleKey.S }:
                Environment.Exit(0);
                break;
            case { Key: ConsoleKey.T }:
                Console.Clear();
                _renderer.UseTransparent = !_renderer.UseTransparent;
                _sixel.SetImage(_renderer.RenderAsImage());
                break;
        }
    }


    protected override void Update()
    {
        int i = Console.WindowHeight - 1;
        _buffer.Clear();

        _writer.Write("\e[1;1H");
        _writer.Write(_sixel);
        _writer.Write($"\e[{i};1H");

        WriteHelp(_writer);

        Console.Write(_writer);
    }

    private void WriteHelp(TextWriter writer)
    {
        string[] fields =
        [
            $"\e[30m\e[47m Escape \e[0m {Resource.TUI_ShpViewer_Exit}",
            _renderer.UseTransparent ? $"\e[92m\e[47m T \e[0m {Resource.TUI_ShpViewer_DisableTransparent}" : $"\e[30m\e[47m T \e[0m {Resource.TUI_ShpViewer_EnableTransparent}",
        ];

        int t = Console.WindowWidth / fields.Length;
        for (int i = 0; i < fields.Length; i++)
        {
            writer.Write($"\e[{t * i}G");
            writer.Write(fields[i]);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;

        if (disposing)
        {
            _sixel.Dispose();
            _writer.Dispose();
        }

        _disposedValue = true;
        base.Dispose(disposing);
    }
}
