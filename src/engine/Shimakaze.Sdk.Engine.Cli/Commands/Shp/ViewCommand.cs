using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Cli.App;
using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Shp;

namespace Shimakaze.Sdk.Engine.Cli.Commands.Shp;

[CliCommand(Description = nameof(Resource.Command_Shp_View_Description), Parent = typeof(ShpCommand))]
internal sealed class ViewCommand
{
    [CliOption(Description = nameof(Resource.Command_Shp_View_Shp_Description), Alias = "i", Aliases = ["input"])]
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

        await viewer.Run();
    }
}
