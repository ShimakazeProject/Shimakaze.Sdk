using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Engine.Mix;

namespace Shimakaze.Sdk.Engine.Cli.Commands.Mix;

[CliCommand(Description = nameof(Resource.Command_Mix_Exports_Description), Alias = "x", Parent = typeof(MixCommand))]
internal sealed class ExportsCommand
{
    [CliOption(Description = nameof(Resource.Command_Mix_Exports_Input_Description))]
    public required FileInfo Input { get; set; }

    [CliOption(Description = nameof(Resource.Command_Mix_Exports_Output_Description))]
    public required DirectoryInfo Output { get; set; }

    [CliOption(Description = nameof(Resource.Command_Mix_Exports_NameMap_Description))]
    public FileInfo? NameMap { get; set; } = default;

    [CliOption(Description = nameof(Resource.Command_Mix_Exports_IsTDMode_Description))]
    public bool IsTDMode { get; set; }

    public async Task RunAsync()
    {
        await using var stream = Input.OpenRead();
        MixExporter exporter = new(stream);

        if (NameMap is { Exists: true })
        {
            using var reader = NameMap.OpenText();
            await exporter.ParseNameMapAsync(reader);
        }

        await exporter.Export(Output.FullName);
    }
}
