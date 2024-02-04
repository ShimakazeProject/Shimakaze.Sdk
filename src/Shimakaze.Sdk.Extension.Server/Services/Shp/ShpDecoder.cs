using Shimakaze.Sdk.Extension.Server.Models.PIXI;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Shp;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Shimakaze.Sdk.Extension.Server.Services.Shp;
internal sealed class ShpDecoder
{
    public static async Task<(SpritesheetData Data, Image<Rgb24> Image)> GetSpritesheetDataAsync(string shpPath, string palPath, bool hasShadow, CancellationToken cancellationToken = default)
    {
        var pal = ReadPaletteAsync(shpPath);
        var shp = ReadShapeAsync(palPath);
        return await GetSpritesheetDataAsync(await shp, await pal, hasShadow, cancellationToken);
    }

    private static async Task<(SpritesheetData Data, Image<Rgb24> Image)> GetSpritesheetDataAsync(ShapeImage shp, Palette pal, bool hasShadow, CancellationToken cancellationToken = default)
    {
        Image<Rgb24>?[] srcFrames = await Task.WhenAll(shp.Frames.Select(i => ParseFrameAsync(i, pal)));

        Dictionary<string, List<string>> animations = [];
        Dictionary<string, SpritesheetFrameData> framesData = new()
        {
            ["empty"] = SpritesheetFrameData.Empty
        };

        int objectEnd = srcFrames.Length;
        if (hasShadow) objectEnd /= 2;

        // 插入的图像的位置
        Image<Rgb24>?[] objectFrames = srcFrames.Take(objectEnd).ToArray();
        Image<Rgb24>?[] shadowFrames = srcFrames.Skip(objectEnd).ToArray();
        int width = Math.Max(
            objectFrames.Select(i => i?.Width ?? 0).Sum(),
            shadowFrames.Select(i => i?.Width ?? 0).Sum()
        );
        int objectHeight = objectFrames.Select(i => i?.Height ?? 0).Max();
        int shadowHeight = shadowFrames.Select(i => i?.Height ?? 0).Max();
        Image<Rgb24> spritesheet = new(width, objectHeight + shadowHeight);

        // 构建对象 spritesheet
        animations["object"] = [];
        BuildSpritesheet(spritesheet, objectFrames, framesData, animations["object"], 0, objectEnd, 0);
        if (hasShadow)
        {
            // 构建影子 spritesheet
            animations["shadow"] = [];
            BuildSpritesheet(spritesheet, shadowFrames, framesData, animations["shadow"], objectEnd, srcFrames.Length, objectHeight);
        }

        return (new(
            framesData,
            new(Format: "RGB888", Size: new(spritesheet.Width, spritesheet.Height)),
            animations.ToDictionary(i => i.Key, i => i.Value.ToArray())),
            spritesheet);
    }

    private static void BuildSpritesheet(
        Image<Rgb24> spritesheet,
        Image<Rgb24>?[] frames,
        Dictionary<string, SpritesheetFrameData> framesData,
        List<string> anim,
        int start,
        int end,
        int y)
    {
        int x = 0;
        for (int i = start; i < end; i++)
        {
            using Image<Rgb24>? frame = frames[i];
            if (frame is null)
            {
                anim.Add("empty");
            }
            else
            {
                string key = $"f{i:D5}";
                SpritesheetFrameData spritesheetFrameData = new(new(x, y, frame.Width, frame.Height));
                framesData.Add(key, spritesheetFrameData);
                anim.Add(key);

                spritesheet.Mutate(i => i.DrawImage(frame, new Point(spritesheetFrameData.Frame.X, spritesheetFrameData.Frame.Y), 1));
                x += spritesheetFrameData.Frame.W;
            }
        }
    }

    private static async Task<Palette> ReadPaletteAsync(string pal)
    {
        await using Stream paletteStream = File.OpenRead(pal);
        return PaletteReader.Read(paletteStream);
    }

    private static async Task<ShapeImage> ReadShapeAsync(string shp)
    {
        await using Stream shapeStream = File.OpenRead(shp);
        return ShapeReader.Read(shapeStream);
    }

    private static async Task<Image<Rgb24>?> ParseFrameAsync(ShapeImageFrame frame, Palette palette)
    {
        if (frame is { Width: 0 } or { Height: 0 })
            return default;

        await using MemoryStream ms = new();
        foreach (var index in frame.Indexes)
            ms.Write(palette[index]);

        return Image.LoadPixelData<Rgb24>(
            ms.ToArray(),
            frame.Width,
            frame.Height);
    }

}
