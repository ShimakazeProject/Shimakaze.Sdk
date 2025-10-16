using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Engine.Shp;
using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Engine.Cli.Commands.Shp;

[CliCommand(Description = nameof(Resource.Command_Shp_Build_Description), Parent = typeof(ShpCommand))]
internal sealed class BuildCommand
{
    [CliOption(Description = nameof(Resource.Command_Shp_Build_Input_Description), AllowMultipleArgumentsPerToken = true)]
    public required List<FileInfo> Input { get; set; }

    [CliOption(Description = nameof(Resource.Command_Shp_Build_House_Description), Required = false, AllowMultipleArgumentsPerToken = true)]
    public List<FileInfo?>? House { get; set; } = null;

    [CliOption(Description = nameof(Resource.Command_Shp_Build_Shadow_Description), Required = false, AllowMultipleArgumentsPerToken = true)]
    public List<FileInfo?>? Shadow { get; set; } = null;

    [CliOption(Description = nameof(Resource.Command_Shp_Build_Output_Description))]
    public required FileInfo Output { get; set; }

    [CliOption(Description = nameof(Resource.Command_Shp_Build_Palette_Description))]
    public required FileInfo Palette { get; set; }

    [CliOption(Description = nameof(Resource.Command_Shp_Build_StartIndex_Description))]
    public int StartIndex { get; set; } = -1;

    [CliOption(Description = nameof(Resource.Command_Shp_Build_EndIndex_Description))]
    public int EndIndex { get; set; } = 240;

    private IEnumerable<ShpFrameSource> GetShpFrameSources()
    {
        for (int i = 0; i < Input.Count; i++)
        {
            var obj = Input[i];
            var house = House?.Count > i ? House[i] : null;
            var shadow = Shadow?.Count > i ? Shadow[i] : null;

            yield return new(obj, shadow, house);
        }
    }

    public async Task RunAsync()
    {
        if (Output is not { Directory.Exists: true })
        {
            if (Output.Directory is null)
                throw new DirectoryNotFoundException(Output.FullName);

            Output.Directory.Create();
        }

        Palette palette;
        await using (var fs = Palette.OpenRead())
            palette = Pal.Palette.ReadFrom(fs);

        await using var output = Output.Create();
        ShpMaker.Build(GetShpFrameSources(), output, palette, StartIndex, EndIndex);
        await output.FlushAsync();
    }
}
