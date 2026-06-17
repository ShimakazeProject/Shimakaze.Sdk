using System.Drawing;
using System.Text;

using Shimakaze.Sdk.Engine.Cli.Sixel;

namespace Shimakaze.Sdk.Engine.Cli.Components;

internal sealed class SixelImage : IDisposable
{
    private const int CellW = 10;
    private const int CellH = 20;

    private readonly StringBuilder _buffer;
    private readonly StringWriter _writer;
    private readonly SixelWriter _sixel;
    private bool _disposedValue;

    public int Width { get; set; }
    public int Height { get; set; }
    public Color[] Palette { get; set; } = [];
    public short[] Indexes { get; set; } = [];
    public bool Center { get; set; }
    public SixelImage()
    {
        _buffer = new();
        _writer = new(_buffer);
        _sixel = new(_writer);
    }

    public SixelImage(int width, int height, Color[] palette, short[] indexes)
        : this()
    {
        Width = width;
        Height = height;
        Palette = palette;
        Indexes = indexes;
    }

    public override string ToString()
    {
        _buffer.Clear();
        var width = Width;
        var height = Height;

        if (Center)
        {
            var l = (Console.WindowWidth - (width / CellW)) / 2;
            _writer.Write($"\e[{l}G");
        }

        _sixel.Begin(width, height);
        for (int i = 0; i < Palette.Length; i++)
        {
            ref readonly var c = ref Palette[i];
            _sixel.RegistColor((byte)i, c);
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var raw = Indexes[y * Width + x];
                byte? index = raw is >= 0 ? (byte)raw : null;
                _sixel.WritePixel(index, 1);
            }

            _sixel.NewLine();
        }
        _sixel.End();

        return _buffer.ToString();
    }

    private void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;

        if (disposing)
        {
            _sixel.Dispose();
            _writer.Dispose();
        }

        _disposedValue = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        //GC.SuppressFinalize(this);
    }
}
