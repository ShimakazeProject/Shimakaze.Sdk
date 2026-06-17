using System.Text;

using Shimakaze.Sdk.Engine.Cli.Components;
using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Tmp;

namespace Shimakaze.Sdk.Engine.Cli.TUI.App;

internal sealed class TmpViewer : Application
{
    private readonly StringBuilder _buffer;
    private readonly StringWriter _writer;
    private readonly TmpImage _tmpImage;
    private bool _disposedValue;

    public TmpViewer(TemplateFile template, Palette palette)
    {
        _buffer = new();
        _writer = new(_buffer);

        _tmpImage = new(template, palette)
        {
            Center = true,
        };
    }

    protected override void OnEvent(EventArgs eventArgs)
    {
        if (eventArgs is ConsoleKeyEventArgs { KeyInfo.Key: ConsoleKey.Escape or ConsoleKey.Q })
            Environment.Exit(0);

        if (eventArgs is ConsoleKeyEventArgs { KeyInfo.Key: ConsoleKey.F5 })
            Console.Clear();
    }

    protected override void Update()
    {
        int i = Console.WindowHeight - 1;
        _buffer.Clear();

        _writer.Write("\e[1;1H");
        _writer.Write(_tmpImage);
        _writer.Write($"\e[{i};1H");

        WriteHelp(_writer);

        Console.Write(_writer);
    }

    private static void WriteHelp(TextWriter writer)
    {
        string[] fields =
        [
            $"\e[30m\e[47m Escape \e[0m {Resource.TUI_ShpViewer_Exit}",
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
            _tmpImage.Dispose();
            _writer.Dispose();
        }

        _disposedValue = true;
        base.Dispose(disposing);
    }
}
