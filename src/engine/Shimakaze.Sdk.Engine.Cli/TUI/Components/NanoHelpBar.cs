using System.Drawing;

namespace Shimakaze.Sdk.Engine.Cli.TUI.Components;

internal class NanoHelpBar(ShortKeyManager shortKeyManager) : ITUIElement
{
    private int _fieldWidth;
    private int _fieldPerLine;
    private int _maxFieldCount;

    public string? Title { get; set; }

    public Size Measure(Size max)
    {
        shortKeyManager.MeasureField(out var x, out _fieldWidth);
        const int padding = 2;

        _fieldPerLine = (max.Width + padding) / (_fieldWidth + padding);
        if ((max.Width + padding) % (_fieldWidth + padding) > 0)
            _fieldPerLine++;

        _fieldWidth = max.Width / _fieldPerLine;
        var line = int.Min((int)double.Ceiling((double)x / _fieldPerLine), max.Height);
        _maxFieldCount = line * _fieldPerLine;

        if (!string.IsNullOrWhiteSpace(Title))
            line++;

        return new(max.Width, line);
    }

    public void OnRender(TextWriter writer, Size size)
    {
        if (!string.IsNullOrWhiteSpace(Title))
        {
            writer.Write("\e[1G");
            for (int i = 0; i < size.Width; i++)
                writer.Write(' ');

            var length = NanoFramework.GetDisplayWidth(Title);

            var start = (size.Width - length) / 2;
            writer.Write($"\e[{start + 1}G");
            writer.Write("\e[7m");
            writer.Write(Title.Replace("\e[0m", "\e[0m\e[7m"));
            writer.Write("\e[0m");
            if (_maxFieldCount is not 0)
                writer.WriteLine();
        }

        shortKeyManager.RenderField(writer, _fieldWidth, _fieldPerLine, _maxFieldCount);
    }
}
