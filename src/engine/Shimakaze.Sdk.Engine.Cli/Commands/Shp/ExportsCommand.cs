using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Engine.Common.Pixels;
using Shimakaze.Sdk.Engine.Shp;
using Shimakaze.Sdk.Shp;

namespace Shimakaze.Sdk.Engine.Cli.Commands.Shp;

[CliCommand(Description = nameof(Resource.Command_Shp_Exports_Description), Parent = typeof(ShpCommand))]
internal sealed class ExportsCommand
{
    [CliOption(Description = nameof(Resource.Command_Shp_Exports_Shp_Description), Alias = "i", Aliases = ["input"])]
    public required FileInfo Shp { get; set; }

    [CliOption(Description = nameof(Resource.Command_Shp_Exports_Palette_Description))]
    public required FileInfo Palette { get; set; }

    [CliOption(Description = nameof(Resource.Command_Shp_Exports_Output_Description))]
    public required string Output { get; set; }

    [CliOption(Description = nameof(Resource.Command_Shp_Exports_Index_Description))]
    public int Index { get; set; } = 0;

    [CliOption(Description = nameof(Resource.Command_Shp_Exports_FrameCounts_Description))]
    public int FrameCounts { get; set; } = -1;

    [CliOption(Description = nameof(Resource.Command_Shp_Exports_Format_Description))]
    public string Format { get; set; } = "webp";

    [CliOption(Description = nameof(Resource.Command_Shp_Exports_Transparent_Description))]
    public bool Transparent { get; set; }

    [CliOption(Description = nameof(Resource.Command_Shp_Exports_Shadow_Description))]
    public bool Shadow { get; set; }

    [CliOption(Description = nameof(Resource.Command_Shp_Exports_ShadowColor_Description))]
    public int? ShadowColor { get; set; }

    [CliOption(Description = nameof(Resource.Command_Shp_Exports_HouseColor_Description))]
    public int? HouseColor { get; set; }

    public async Task RunAsync()
    {
        ShapeImage shp;
        await using (var fs = Shp.OpenRead())
            shp = ShapeImage.ReadFrom(fs);

        Pal.Palette pal;
        await using (var fs = Palette.OpenRead())
            pal = Pal.Palette.ReadFrom(fs);

        ShapeRenderer renderer = new(shp, pal)
        {
            HasShadow = Shadow,
        };
        if (FrameCounts is -1)
            FrameCounts = renderer.Count;
        if (Transparent)
            renderer.Palette[0] = RGBA32.Transparent;
        if (ShadowColor.HasValue)
            renderer.Palette[1] = new(unchecked((uint)ShadowColor.Value));
        if (HouseColor.HasValue)
            renderer.UpdateHouseColor(new(unchecked((uint)HouseColor.Value)));

        int end = Index + FrameCounts;
        for (int i = Index; i < end; i++)
        {
            var frameRenderer = renderer.GetFrame(i);
            var image = frameRenderer.RenderAsImage();

            string output = Path.GetFullPath(Output);
            if (FrameCounts is > 1)
                output += $"{i:D4}.{Format}".ToLowerInvariant();

            if (Path.GetDirectoryName(output) is { } dirpath && !Directory.Exists(dirpath))
                Directory.CreateDirectory(dirpath);

            image.SaveTo(output);
        }
    }
}
