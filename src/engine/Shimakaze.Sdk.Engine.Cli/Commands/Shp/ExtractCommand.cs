using System.Runtime.InteropServices;

using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Engine.Shp;
using Shimakaze.Sdk.Shp;

namespace Shimakaze.Sdk.Engine.Cli.Commands.Shp;

[CliCommand(Description = nameof(Resource.Command_Shp_Extract_Description), Parent = typeof(ShpCommand))]
internal sealed class ExtractCommand
{
    [CliOption(Description = nameof(Resource.Command_Shp_Extract_Shp_Description))]
    public required FileInfo Shp { get; set; }

    [CliOption(Description = nameof(Resource.Command_Shp_Extract_Palette_Description))]
    public required FileInfo Palette { get; set; }

    [CliOption(Description = nameof(Resource.Command_Shp_Extract_Output_Description))]
    public required string Output { get; set; }

    [CliOption(Description = nameof(Resource.Command_Shp_Extract_Index_Description))]
    public int Index { get; set; } = 0;

    [CliOption(Description = nameof(Resource.Command_Shp_Extract_FrameCounts_Description))]
    public int FrameCounts { get; set; } = 1;

    [CliOption(Description = nameof(Resource.Command_Shp_Extract_Format_Description))]
    public string Format { get; set; } = "webp";

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

        foreach ((int i, Image image) in ParseFrames(extractor))
        {
            var output = Path.GetFullPath(Output);
            if (FrameCounts is > 1)
                output += $"{i:D4}.{Format}".ToLowerInvariant();

            if (Path.GetDirectoryName(output) is { } dirpath && !Directory.Exists(dirpath))
                Directory.CreateDirectory(dirpath);

            image.SaveTo(output);
        }
    }

    private IEnumerable<(int Index, Image Bitmap)> ParseFrames(ShpExtractor extractor)
    {
        var shp = extractor.Shape;

        var end = Index + FrameCounts;

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

            Image bitmap = new(shp.Metadata.Width, shp.Metadata.Height, ImmutableCollectionsMarshal.AsImmutableArray(canvas));
            yield return (i, bitmap);
        }
    }
}


