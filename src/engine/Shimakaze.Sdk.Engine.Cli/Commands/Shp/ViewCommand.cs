using System.Text;

using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Engine.Cli.Sixel;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Shp;

namespace Shimakaze.Sdk.Engine.Cli.Commands.Shp;

[CliCommand(Description = nameof(Resource.Command_Shp_View_Description), Parent = typeof(ShpCommand))]
internal sealed class ViewCommand
{
    [CliOption(Description = nameof(Resource.Command_Shp_View_Shp_Description))]
    public required FileInfo Shp { get; set; }

    [CliOption(Description = nameof(Resource.Command_Shp_View_Palette_Description))]
    public required FileInfo Palette { get; set; }

    public async Task RunAsync()
    {
        Palette palette;
        using (var fs = Palette.OpenRead())
            palette = Pal.Palette.ReadFrom(fs);

        ShapeImage shp;
        using (var fs = Shp.OpenRead())
            shp = ShapeImage.ReadFrom(fs);

        using ShpViewer viewer = new(shp, palette);

        Console.CancelKeyPress += (_, e) =>
        {
            viewer.Dispose();
            Environment.Exit(0);
        };
        while (true)
        {
            Console.WriteLine(viewer.Update());

            var key = Console.ReadKey();
            switch (key)
            {
                case { Key: ConsoleKey.S }:
                    viewer.Shadow = !viewer.Shadow;
                    break;
                case { Key: ConsoleKey.Tab, Modifiers: ConsoleModifiers.Shift }:
                case { Key: ConsoleKey.UpArrow }:
                case { Key: ConsoleKey.LeftArrow }:
                    viewer.Current--;
                    break;
                case { Key: ConsoleKey.Tab }:
                case { Key: ConsoleKey.DownArrow }:
                case { Key: ConsoleKey.RightArrow }:
                    viewer.Current++;
                    break;
                default:
                    break;
            }
        }
    }
}

internal sealed class ShpViewer : IDisposable
{
    private readonly StringBuilder _buffer = new();
    private readonly StringWriter _bufferWriter;
    private readonly SixelWriter _sixel;
    private readonly ShapeImage _shp;
    private readonly Palette _pal;
    private bool _disposedValue;

    public bool Shadow { get; set; } = true;
    public int Max { get; private set; }
    public int Current
    {
        get => field;
        set => field = value > Max
            ? int.Min(value, Max)
            : int.Max(value, 0);
    }

    public ShpViewer(ShapeImage shp, Palette pal)
    {
        _shp = shp;
        _pal = pal;
        _bufferWriter = new(_buffer);
        _sixel = new(_bufferWriter);
        Console.Write("\e[?1049h");
    }

    public string Update()
    {
        Max = _shp.Frames.Count;
        if (Shadow)
            Max /= 2;
        Max--;

        _buffer.Clear();
        _bufferWriter.Write("\e[2J");
        if (Shadow)
        {
            _bufferWriter.Write($"\e[1;1H");
            _sixel.Begin(_shp.Metadata.Width, _shp.Metadata.Height);
            //for (int i = 0; i < palette.Colors.Length; i++)
            //    sixel.RegistColor(i, palette[i]);
            _sixel.RegistColor(1, 0x3f3f3f);

            PrintImage(_sixel, _shp.Metadata, _shp.Frames[Current + Max + 1]);
            _sixel.End();
        }
        _bufferWriter.Write($"\e[1;1H");

        _sixel.Begin(_shp.Metadata.Width, _shp.Metadata.Height);
        for (int i = 0; i < _pal.Colors.Length; i++)
            _sixel.RegistColor(i, _pal[i]);

        PrintImage(_sixel, _shp.Metadata, _shp.Frames[Current]);
        _sixel.End();

        return _bufferWriter.ToString();
    }

    private static void PrintImage(SixelWriter sixel, ShapeFileHeader fileMetadata, ShapeImageFrame frame)
    {
        for (ushort i = 0; i < frame.Metadata.Y; i++)
        {
            sixel.WritePixel(null, frame.Width);
            sixel.NewLine();
        }
        for (int y = 0; y < frame.Metadata.Height; y++)
        {
            sixel.WritePixel(null, frame.Metadata.X);

            for (int x = 0; x < frame.Metadata.Width; x++)
            {
                int i = y * frame.Metadata.Width + x;
                var v = frame.Indexes.Span[i];
                if (v is 0)
                    sixel.WritePixel(null, 1);
                else
                    sixel.WritePixel(v, 1);
            }

            sixel.WritePixel(null, fileMetadata.Width - frame.Metadata.Width - frame.Metadata.X);
            sixel.NewLine();
        }
        var maxY = fileMetadata.Height - frame.Metadata.Height - frame.Metadata.Y;
        for (ushort i = 0; i < maxY; i++)
        {
            sixel.WritePixel(null, frame.Width);
            sixel.NewLine();
        }
    }

    private void Dispose(bool disposing)
    {
        Console.Write("\e[?1049l");
        if (!_disposedValue)
        {
            if (disposing)
            {
                _sixel.Dispose();
                _bufferWriter.Dispose();
            }

            // TODO: 释放未托管的资源(未托管的对象)并重写终结器
            // TODO: 将大型字段设置为 null
            _disposedValue = true;
        }
    }

    // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
    // ~ShpViewer()
    // {
    //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        //GC.SuppressFinalize(this);
    }
}
