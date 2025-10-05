using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Engine.Shp;
using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Engine.Cli;

[CliCommand(Description = nameof(Resource.ShpCommand_Description), Parent = typeof(RootCommand))]
internal sealed class ShpCommand
{
    [CliOption(Description = nameof(Resource.ShpCommand_Input_Description), AllowMultipleArgumentsPerToken = true)]
    public required List<FileInfo> Input { get; set; }

    [CliOption(Description = nameof(Resource.ShpCommand_House_Description), Required = false, AllowMultipleArgumentsPerToken = true)]
    public List<FileInfo?>? House { get; set; } = null;

    [CliOption(Description = nameof(Resource.ShpCommand_Shadow_Description), Required = false, AllowMultipleArgumentsPerToken = true)]
    public List<FileInfo?>? Shadow { get; set; } = null;

    [CliOption(Description = nameof(Resource.ShpCommand_Output_Description))]
    public required FileInfo Output { get; set; }

    [CliOption(Description = nameof(Resource.ShpCommand_Palette_Description))]
    public required FileInfo Palette { get; set; }

    [CliOption(Description = nameof(Resource.ShpCommand_StartIndex_Description))]
    public int StartIndex { get; set; } = -1;

    [CliOption(Description = nameof(Resource.ShpCommand_EndIndex_Description))]
    public int EndIndex { get; set; } = 240;

    public async Task RunAsync()
    {
        List<ShpFrameSource> sources = new(Input.Count);
        for (int i = 0; i < Input.Count; i++)
        {
            var obj = Input[i];
            var house = House?.Count > i ? House[i] : null;
            var shadow = Shadow?.Count > i ? Shadow[i] : null;

            sources.Add(new(obj, house, shadow));
        }

        Palette palette;
        await using (var fs = Palette.OpenRead())
            palette = Pal.Palette.ReadFrom(fs);

        await using var output = Output.Create();
        ShpMaker.Build(sources, output, palette, StartIndex, EndIndex);
    }
}
