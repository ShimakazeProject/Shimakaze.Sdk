using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Engine.Shp;
using Shimakaze.Sdk.Shp;

using SkiaSharp;

namespace Shimakaze.Sdk.Engine.Cli.Commands.Shp;

[CliCommand(Description = nameof(Resource.Command_Shp_Extract_Description), Parent = typeof(ShpCommand))]
internal class ExtractCommand
{
    [CliOption(Description = nameof(Resource.Command_Shp_Extract_Shp_Description))]
    public required FileInfo Shp { get; set; }

    [CliOption(Description = nameof(Resource.Command_Shp_Extract_Palette_Description))]
    public required FileInfo Palette { get; set; }

    [CliOption(Description = nameof(Resource.Command_Shp_Extract_Output_Description))]
    public required string Output { get; set; }

    [CliOption(Description = nameof(Resource.Command_Shp_Extract_Index_Description))]
    public int Index { get; set; }

    [CliOption(Description = nameof(Resource.Command_Shp_Extract_EndIndex_Description), Required = false)]
    public int? EndIndex { get; set; } = null;

    [CliOption(Description = nameof(Resource.Command_Shp_Extract_Format_Description))]
    public SKEncodedImageFormat Format { get; set; }

    [CliOption(Description = nameof(Resource.Command_Shp_Extract_Quality_Description))]
    public int Quality { get; set; } = 90;

    [CliOption(Description = nameof(Resource.Command_Shp_Extract_Transparent_Description))]
    public bool Transparent { get; set; }

    [CliOption(Description = nameof(Resource.Command_Shp_Extract_Shadow_Description))]
    public bool Shadow { get; set; }

    [CliOption(Description = nameof(Resource.Command_Shp_Extract_ShadowColor_Description))]
    public int? ShadowColor { get; set; }

    public async Task RunAsync()
    {
        ShapeImage shp;
        await using (var fs = Shp.OpenRead())
            shp = ShapeImage.ReadFrom(fs);

        Pal.Palette pal;
        await using (var fs = Palette.OpenRead())
            pal = Pal.Palette.ReadFrom(fs);

        ShpExtractor extractor = new(shp, pal);

        var end = EndIndex ?? Index;

        for (int i = Index; i < end; i++)
        {
            var canvas = extractor.CreateCanvas(Transparent);
            if (Shadow)
            {
                if (ShadowColor.HasValue)
                    extractor.SetColor(1, new(unchecked((uint)ShadowColor.Value)));

                extractor.DrawFrame(canvas, shp.Frames[shp.Frames.Count / 2 + i], []);
            }

            extractor.DrawFrame(canvas, shp.Frames[i], []);

            using SKBitmap bitmap = new(shp.Metadata.Width, shp.Metadata.Height, SKColorType.Rgba8888, SKAlphaType.Premul)
            {
                Pixels = canvas,
            };

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(Format, Quality);

            var output = Path.GetFullPath(Output);
            if (EndIndex.HasValue)
                output = Path.Combine(output, $"{i:D4}.{Format}".ToLowerInvariant());

            if (Path.GetDirectoryName(output) is { } dirpath && !Directory.Exists(dirpath))
                Directory.CreateDirectory(dirpath);

            await using var fs = File.Create(output);
            data.SaveTo(fs);
        }
    }
}


