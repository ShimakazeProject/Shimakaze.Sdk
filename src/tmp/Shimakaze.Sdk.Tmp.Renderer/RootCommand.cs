using System.Diagnostics.CodeAnalysis;

using DotMake.CommandLine;

using Sharprompt;

using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Tmp;

using SkiaSharp;

namespace Shimakaze.Sdk.Tmp.Renderer;

[CliCommand(Description = "Shimakaze.Sdk Tmp 渲染器")]
internal sealed class RootCommand
{
    [CliArgument(Description = "模板文件", Required = true)]
    public string? Template { get; set; }

    [CliArgument(Description = "参考的调色板文件", Required = true)]
    public string? Palette { get; set; }

    [CliArgument(Description = "输出的 BMP 文件", Required = true)]
    public string? Output { get; set; }

    [CliOption(Description = "使用安静模式，不提示用户交互")]
    public bool Quiet { get; set; }

    [MemberNotNull(nameof(Template), nameof(Palette), nameof(Output))]
    public void Assert()
    {
        ArgumentNullException.ThrowIfNull(Template);
        ArgumentNullException.ThrowIfNull(Palette);
        ArgumentNullException.ThrowIfNull(Output);
    }

    public void UsePrompt()
    {
        if (Template is null || !File.Exists(Template))
        {
        Template:
            Template = Prompt.Input<string>("请输入图像列表文件路径");
            if (Template is null || !File.Exists(Template))
            {
                Console.Error.WriteLine("无效的图像列表文件路径");
                goto Template;
            }
        }

        if (Palette is null || !File.Exists(Palette))
        {
        Palette:
            Palette = Prompt.Input<string>("请输入参考的调色板文件路径");
            if (Palette is null || !File.Exists(Palette))
            {
                Console.Error.WriteLine("无效的调色板文件路径");
                goto Palette;
            }
        }

        if (Output is null)
        {
        Output:
            Output = Prompt.Input<string>("请输入输出的 SHP(TS) 文件路径");
            if (Output is null)
            {
                Console.Error.WriteLine("无效的 SHP(TS) 文件路径");
                goto Output;
            }
            if (File.Exists(Output)
                && !Prompt.Confirm("已存在同名文件，是否覆盖？", false))
                goto Output;
        }
    }

    public async Task RunAsync()
    {
        if (!Quiet)
            UsePrompt();

        Assert();
        Palette palette;
        await using (var fs = File.OpenRead(Palette))
            palette = Pal.Palette.ReadFrom(fs);

        TemplateFile template;
        await using (var fs = File.OpenRead(Template))
            template = TemplateFile.ReadFrom(fs);

        int tileW = (int)template.Header.BlockImageWidth;
        int tileH = (int)template.Header.BlockImageHeight;
        int halfW = tileW / 2;
        int halfH = tileH / 2;

        // 高度偏移系数：每单位高度对应的像素偏移量
        // 等距瓦片的视觉高度是 tileH/2 = 15，所以每级高度偏移15像素
        int heightOffset = halfH; // 15

        // 先计算所有瓦片的边界框
        // TileHeader.X 和 Y 是像素偏移量，不是网格坐标
        // 需要考虑 Header.Height 对 Y 坐标的影响
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;
        foreach (var tile in template.Tiles)
        {
            int tx = tile.Header.X;
            // 高度越高，瓦片画得越高（Y坐标越小）
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
        SKColor[] pixels = GC.AllocateUninitializedArray<SKColor>(pixelsWidth * pixelsHeight);

        foreach (var tile in template.Tiles)
        {
            var indexes = tile.Tile.AsSpan();

            // TileHeader.X 和 Y 是像素偏移量，直接使用
            // 根据 Header.Height 调整 Y 坐标，高度越高，画得越高
            int tileX = tile.Header.X - minX;
            int tileY = tile.Header.Y - minY - tile.Header.Height * heightOffset;

            // 绘制等距瓦片（菱形格子本体数据）
            int tilePos = 0;
            int width = 4;
            for (int y = 0; y < 29; y++)
            {
                // 等距瓦片最宽处为60像素，居中放置需要偏移 halfW
                int outX = tileX + halfW - width / 2;
                int outY = tileY + y;

                for (int x = 0; x < width; x++)
                {
                    if (tilePos >= indexes.Length)
                        continue;

                    int colorIndex = indexes[tilePos];
                    // 跳过透明色（索引0）
                    if (colorIndex == 0)
                    {
                        tilePos++;
                        continue;
                    }

                    if (colorIndex < palette.Colors.Length)
                    {
                        DisplayColor color = palette[colorIndex];
                        int pixelIndex = outY * pixelsWidth + (outX + x);
                        if (pixelIndex >= 0 && pixelIndex < pixels.Length)
                        {
                            pixels[pixelIndex] = new SKColor(color.Red, color.Green, color.Blue);
                        }
                    }
                    tilePos++;
                }

                // 前 15 行宽度增加，后 14 行宽度减少（共 29 行，底部一行是空的）
                if (y < 14)
                    width += 4;
                else
                    width -= 4;
            }

            // 绘制 Extra 数据（菱形以外的扩展数据）
            // Extra 数据是矩形图像，需要根据 ExtraX 和 ExtraY 偏移绘制
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
                        // 跳过透明色（索引0）
                        if (colorIndex == 0)
                            continue;

                        if (colorIndex >= palette.Colors.Length)
                            continue;

                        DisplayColor color = palette[colorIndex];
                        int outX = extraX + x;
                        int outY = extraY + y;
                        int pixelIndex = outY * pixelsWidth + outX;
                        if (pixelIndex >= 0 && pixelIndex < pixels.Length)
                        {
                            pixels[pixelIndex] = new SKColor(color.Red, color.Green, color.Blue);
                        }
                    }
                }
            }
        }

        using SKBitmap bitmap = new(pixelsWidth, pixelsHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        bitmap.Pixels = pixels;

        await using (var fs = File.Create(Output))
            bitmap.Encode(fs, SKEncodedImageFormat.Png, 100);
    }
}
