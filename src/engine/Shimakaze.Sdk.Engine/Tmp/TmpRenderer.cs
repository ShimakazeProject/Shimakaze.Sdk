using System.Drawing;

using Shimakaze.Sdk.Engine.Common;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Tmp;

namespace Shimakaze.Sdk.Engine.Tmp;

internal sealed class TmpRenderer : Renderer
{
    private readonly int _tileW;
    private readonly int _tileH;
    private readonly int _halfW;
    private readonly int _halfH;
    private readonly int _heightOffset;
    private readonly int _minX;
    private readonly int _minY;
    private readonly int _maxX;
    private readonly int _maxY;
    private readonly int _pixelsWidth;
    private readonly int _pixelsHeight;
    private readonly BGRA32[] _palette;

    public override Size Size { get; }
    public TemplateFile Template { get; }
    public bool UseTransparent { get; set; }

    public TmpRenderer(TemplateFile template, Palette palette)
    {
        Template = template;
        _palette = [.. palette.Cast<DisplayColor>().Select(i => (BGRA32)i)];

        _tileW = (int)Template.Header.BlockImageWidth;
        _tileH = (int)Template.Header.BlockImageHeight;
        _halfW = _tileW / 2;
        _halfH = _tileH / 2;
        _heightOffset = _halfH;

        // 计算所有瓦片的边界框
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;
        foreach (var tile in Template.Tiles)
        {
            int tx = tile.Header.X;
            int ty = tile.Header.Y - (tile.Header.Height * _heightOffset);
            minX = Math.Min(minX, tx);
            minY = Math.Min(minY, ty);
            maxX = Math.Max(maxX, tx + _tileW);
            maxY = Math.Max(maxY, ty + _tileH);

            // 考虑 ExtraData 的边界
            if (tile.Header.Flags.HasFlag(TemplateTileCellFlags.HasExtraData) && tile.Extra.Length > 0)
            {
                int extraX = tile.Header.ExtraX;
                int extraY = tile.Header.ExtraY - (tile.Header.Height * _heightOffset);
                int extraWidth = (int)tile.Header.ExtraWidth;
                int extraHeight = (int)tile.Header.ExtraHeight;
                minX = Math.Min(minX, extraX);
                minY = Math.Min(minY, extraY);
                maxX = Math.Max(maxX, extraX + extraWidth);
                maxY = Math.Max(maxY, extraY + extraHeight);
            }
        }

        _minX = minX;
        _minY = minY;
        _maxX = maxX;
        _maxY = maxY;
        _pixelsWidth = maxX - minX;
        _pixelsHeight = maxY - minY;

        Size = new(_pixelsWidth, _pixelsHeight);
    }

    public override BGRA32[] CreateBuffer()
    {
        var buffer = base.CreateBuffer();
        var bg = UseTransparent
            ? BGRA32.Transparent
            : _palette[0];
        buffer.AsSpan().Fill(bg);
        return buffer;
    }

    public override void RenderTo(BGRA32[] canvas)
    {
        foreach (var tile in Template.Tiles)
        {
            var indexes = tile.Tile.AsSpan();

            int tileX = tile.Header.X - _minX;
            int tileY = tile.Header.Y - _minY - (tile.Header.Height * _heightOffset);

            // 绘制等距瓦片（菱形格子本体数据）
            int tilePos = 0;
            int width = 4;
            for (int y = 0; y < 29; y++)
            {
                int outX = tileX + _halfW - (width / 2);
                int outY = tileY + y;

                for (int x = 0; x < width; x++)
                {
                    if (tilePos >= indexes.Length)
                        continue;

                    int colorIndex = indexes[tilePos];
                    if (colorIndex > 0 && colorIndex < _palette.Length)
                    {
                        int pixelIndex = (outY * _pixelsWidth) + outX + x;
                        if (pixelIndex >= 0 && pixelIndex < canvas.Length)
                            canvas[pixelIndex] = _palette[colorIndex];
                    }
                    tilePos++;
                }

                // 前 14 行宽度增加，后 14 行宽度减少（共 29 行）
                if (y < 14)
                    width += 4;
                else
                    width -= 4;
            }

            // 绘制 Extra 数据（菱形以外的扩展数据）
            if (tile.Header.Flags.HasFlag(TemplateTileCellFlags.HasExtraData) && tile.Extra.Length > 0)
            {
                var extraData = tile.Extra.AsSpan();
                int extraX = tile.Header.ExtraX - _minX;
                int extraY = tile.Header.ExtraY - _minY - (tile.Header.Height * _heightOffset);
                int extraWidth = (int)tile.Header.ExtraWidth;
                int extraHeight = (int)tile.Header.ExtraHeight;

                for (int y = 0; y < extraHeight; y++)
                {
                    for (int x = 0; x < extraWidth; x++)
                    {
                        int extraPos = (y * extraWidth) + x;
                        if (extraPos >= extraData.Length)
                            continue;

                        int colorIndex = extraData[extraPos];
                        if (colorIndex > 0 && colorIndex < _palette.Length)
                        {
                            int outX = extraX + x;
                            int outY = extraY + y;
                            int pixelIndex = (outY * _pixelsWidth) + outX;
                            if (pixelIndex >= 0 && pixelIndex < canvas.Length)
                                canvas[pixelIndex] = _palette[colorIndex];
                        }
                    }
                }
            }
        }
    }
}
