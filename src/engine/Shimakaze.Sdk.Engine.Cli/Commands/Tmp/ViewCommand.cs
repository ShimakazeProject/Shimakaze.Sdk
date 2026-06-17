using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Engine.Cli.TUI.App;
using Shimakaze.Sdk.Engine.Tmp;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Tmp;

namespace Shimakaze.Sdk.Engine.Cli.Commands.Tmp;

[CliCommand(Description = nameof(Resource.Command_Tmp_View_Description), Parent = typeof(TmpCommand))]
internal sealed class ViewCommand
{
    [CliOption(Description = nameof(Resource.Command_Tmp_View_Template_Description), Alias = "i", Aliases = ["input"])]
    public required FileInfo Template { get; set; }

    [CliOption(Description = nameof(Resource.Command_Tmp_View_Palette_Description))]
    public required FileInfo Palette { get; set; }

    public async Task RunAsync()
    {
        Palette palette;
        await using (var fs = Palette.OpenRead())
            palette = Pal.Palette.ReadFrom(fs);

        TemplateFile template;
        await using (var fs = Template.OpenRead())
            template = TemplateFile.ReadFrom(fs);

        using TmpViewer viewer = new(template, palette);
        await viewer.Run();
    }
}
