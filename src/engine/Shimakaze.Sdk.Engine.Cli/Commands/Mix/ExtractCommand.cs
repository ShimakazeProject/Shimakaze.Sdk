using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Engine.Mix;

namespace Shimakaze.Sdk.Engine.Cli.Commands.Mix;

[CliCommand(Description = nameof(Resource.Command_Mix_Extract_Description), Alias = "x", Parent = typeof(MixCommand))]
internal sealed class ExtractCommand
{
    [CliOption(Description = nameof(Resource.Command_Mix_Extract_Input_Description))]
    public required FileInfo Input { get; set; }

    [CliOption(Description = nameof(Resource.Command_Mix_Extract_Output_Description))]
    public required DirectoryInfo Output { get; set; }

    [CliOption(Description = nameof(Resource.Command_Mix_Extract_NameMap_Description))]
    public FileInfo? NameMap { get; set; } = default;

    [CliOption(Description = nameof(Resource.Command_Mix_Extract_IsTDMode_Description))]
    public bool IsTDMode { get; set; }

    public async Task RunAsync()
    {
        Dictionary<uint, string> namemap;
        if (NameMap is { Exists: true })
        {
            using var reader = NameMap.OpenText();
            namemap = await MixExtractor.ParseNameMapAsync(reader);
        }
        else
        {
            namemap = [];
        }

        await using FileStream stream = Input.OpenRead();
        await MixExtractor.Extract(stream, Output.FullName, namemap, IsTDMode);
    }
}
