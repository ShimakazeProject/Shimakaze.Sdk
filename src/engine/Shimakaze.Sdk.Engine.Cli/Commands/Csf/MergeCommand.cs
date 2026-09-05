using DotMake.CommandLine;

using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Engine.Csf;

namespace Shimakaze.Sdk.Engine.Cli.Commands.Csf;

[CliCommand(Description = nameof(Resource.Command_Csf_Merge_Description), Parent = typeof(CsfCommand))]
internal sealed class MergeCommand
{
    [CliOption(Description = nameof(Resource.Command_Csf_Merge_Input_Description), Alias = "i", AllowMultipleArgumentsPerToken = true)]
    public required List<FileInfo> Input { get; set; }

    [CliOption(Description = nameof(Resource.Command_Csf_Merge_Output_Description), Alias = "o")]
    public required FileInfo Output { get; set; }

    [CliOption(Description = nameof(Resource.Command_Csf_Merge_InputFormat_Description), Alias = "if", Required = false, AllowMultipleArgumentsPerToken = true)]
    public List<CsfFormat?> InputFormat { get; set; } = [];

    [CliOption(Description = nameof(Resource.Command_Csf_Merge_OutputFormat_Description), Alias = "of", Required = false)]
    public CsfFormat OutputFormat { get; set; } = CsfFormat.Csf;

    public async Task RunAsync()
    {
        var inputs = Input
            .Select((item, index) => (index, item))
            .LeftJoin(
                InputFormat
                    .Select((item, index) => (index, item)),
                i => i.index,
                i => i.index,
                (a, b) => (File: a.item, Format: b.item ?? CsfTool.GuessInputFormat(a.item)));

        List<CsfData> list = [];
        foreach (var (file, format) in inputs)
        {
            await using var ifs = file.OpenRead();
            var data = CsfTool.LoadFrom(ifs, format);
            list.Add(data);
        }

        var merged = CsfMerger.Merge(list);
        await using var ofs = Output.Create();
        CsfTool.SaveTo(merged, ofs, OutputFormat);
    }
}
