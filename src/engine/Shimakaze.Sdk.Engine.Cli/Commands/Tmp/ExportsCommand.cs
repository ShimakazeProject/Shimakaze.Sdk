using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Engine.Tmp;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Tmp;

namespace Shimakaze.Sdk.Engine.Cli.Commands.Tmp;

[CliCommand(Description = nameof(Resource.Command_Tmp_Exports_Description), Alias = "x", Parent = typeof(TmpCommand))]
internal sealed class ExportsCommand
{
    [CliOption(Description = nameof(Resource.Command_Tmp_Exports_Template_Description), Alias = "i", Aliases = ["input"])]
    public required FileInfo Template { get; set; }

    [CliOption(Description = nameof(Resource.Command_Tmp_Exports_Palette_Description))]
    public required FileInfo Palette { get; set; }

    [CliOption(Description = nameof(Resource.Command_Tmp_Exports_Output_Description))]
    public required string Output { get; set; }

    public async Task RunAsync()
    {
        Palette palette;
        await using (var fs = Palette.OpenRead())
            palette = Pal.Palette.ReadFrom(fs);

        TemplateFile template;
        await using (var fs = Template.OpenRead())
            template = TemplateFile.ReadFrom(fs);

        TemplateRenderer renderer = new(template, palette);
        var image = renderer.RenderAsImage();

        string output = Path.GetFullPath(Output);
        if (Path.GetDirectoryName(output) is { } dirpath && !Directory.Exists(dirpath))
            Directory.CreateDirectory(dirpath);

        image.SaveTo(output);
    }
}
