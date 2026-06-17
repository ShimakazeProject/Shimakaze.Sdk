using System.Collections.Immutable;
using System.Runtime.InteropServices;

using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Tmp;

namespace Shimakaze.Sdk.Engine.Tmp;

internal sealed class TmpRenderer(TemplateFile template, Palette palette)
{
    public TemplateFile Template { get; } = template;
    public Palette Palette { get; } = palette;

    /// <summary>
    /// 将 TMP 模板渲染为 <see cref="Image"/>
    /// </summary>
    /// <returns>渲染后的图像</returns>
    public Image Render()
    {
        int tileW = (int)Template.Header.BlockImageWidth;
        int tileH = (int)Template.Header.BlockImageHeight;
        int halfW = tileW / 2;
        int halfH = tileH / 2;

        // 高度偏移系数：每单位高度对应的像素偏移量
        int heightOffset = halfH;

        // 第一遍：计算所有瓦片的边界框
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;
        foreach (var tile in Template.Tiles)
        {
            int tx = tile.Header.X;
            int ty = tile.Header.Y - tile.Header.Height * heightOffset;
            minX = Math.Min(minX, tx);
            minY = Math.Min(minY, ty);
            maxX = Math.Max(maxX, tx + tileW);
            maxY = Math.Max(maxY, ty + tileH);

            // 考虑 ExtraData 的边界
            if (tile.Header.Flags.HasFlag(TemplateTileCellFlags.HasExtraData) && tile.Extra.Length > 0)
            {
                int extraX = tile.Header.ExtraX;
                int extraY = tile.Header.ExtraY - tile.Header.Height * heightOffset;
                int extraWidth = (int)tile.Header.ExtraWidth;
                int extraHeight = (int)tile.Header.ExtraHeight;
                minX = Math.Min(minX, extraX);
                minY = Math.Min(minY, extraY);
                maxX = Math.Max(maxX, extraX + extraWidth);
                maxY = Math.Max(maxY, extraY + extraHeight);
            }
        }

        int pixelsWidth = maxX - minX;
        int pixelsHeight = maxY - minY;

        // 预转换调色板为 BGRA32 查找表，避免循环内反复创建对象
        var paletteTable = Palette.Cast<DisplayColor>().Select(i => (BGRA32)i).ToImmutableArray();

        BGRA32[] pixels = new BGRA32[pixelsWidth * pixelsHeight];

        foreach (var tile in Template.Tiles)
        {
            var indexes = tile.Tile.AsSpan();

            int tileX = tile.Header.X - minX;
            int tileY = tile.Header.Y - minY - tile.Header.Height * heightOffset;

            // 绘制等距瓦片（菱形格子本体数据）
            int tilePos = 0;
            int width = 4;
            for (int y = 0; y < 29; y++)
            {
                int outX = tileX + halfW - width / 2;
                int outY = tileY + y;

                for (int x = 0; x < width; x++)
                {
                    if (tilePos >= indexes.Length)
                        continue;

                    int colorIndex = indexes[tilePos];
                    if (colorIndex > 0 && colorIndex < paletteTable.Length)
                    {
                        int pixelIndex = outY * pixelsWidth + (outX + x);
                        if (pixelIndex >= 0 && pixelIndex < pixels.Length)
                            pixels[pixelIndex] = paletteTable[colorIndex];
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
                int extraX = tile.Header.ExtraX - minX;
                int extraY = tile.Header.ExtraY - minY - tile.Header.Height * heightOffset;
                int extraWidth = (int)tile.Header.ExtraWidth;
                int extraHeight = (int)tile.Header.ExtraHeight;

                for (int y = 0; y < extraHeight; y++)
                {
                    for (int x = 0; x < extraWidth; x++)
                    {
                        int extraPos = y * extraWidth + x;
                        if (extraPos >= extraData.Length)
                            continue;

                        int colorIndex = extraData[extraPos];
                        if (colorIndex > 0 && colorIndex < paletteTable.Length)
                        {
                            int outX = extraX + x;
                            int outY = extraY + y;
                            int pixelIndex = outY * pixelsWidth + outX;
                            if (pixelIndex >= 0 && pixelIndex < pixels.Length)
                                pixels[pixelIndex] = paletteTable[colorIndex];
                        }
                    }
                }
            }
        }

        return new Image(pixelsWidth, pixelsHeight, ImmutableCollectionsMarshal.AsImmutableArray(pixels));
    }
}
