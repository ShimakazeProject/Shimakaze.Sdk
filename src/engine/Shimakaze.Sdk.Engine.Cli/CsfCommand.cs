using System.Diagnostics.CodeAnalysis;

using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Engine.Csf;

namespace Shimakaze.Sdk.Engine.Cli;

[CliCommand(Description = nameof(Resource.CsfCommand_Description), Parent = typeof(RootCommand))]
internal sealed class CsfCommand
{
    [CliArgument(Description = nameof(Resource.CsfCommand_Input_Description))]
    public required FileInfo Input { get; set; }

    [CliArgument(Description =  nameof(Resource.CsfCommand_Output_Description), Required = false)]
    public FileInfo? Output { get; set; } = null;

    [CliOption(Description = nameof(Resource.CsfCommand_InputFormat_Description), Required = false)]
    public CsfFormat? InputFormat { get; set; }

    [CliOption(Description = nameof(Resource.CsfCommand_OutputFormat_Description), Required = false)]
    public CsfFormat? OutputFormat { get; set; }

    public async Task RunAsync()
    {
        if (InputFormat is null)
            InputFormat = CsfTool.GuessInputFormat(Input);
        if (OutputFormat is null)
            OutputFormat = CsfTool.GuessOutputFormat(InputFormat.Value);
        InitOutput(OutputFormat.Value);

        await using var ifs = Input.OpenRead();
        var data = await CsfTool.LoadFromAsync(ifs, InputFormat.Value);

        await using var ofs = Output.Create();
        await CsfTool.SaveToAsync(data, ofs, OutputFormat.Value);
    }

    [MemberNotNull(nameof(Output))]
    private void InitOutput(CsfFormat outputFormat)
    {
        if (Output is not null)
            return;

        var path = Input.DirectoryName ?? throw new DirectoryNotFoundException();
        var name = Input.Name.Split('.', 2).First();

        string output = GetSupportedFormatExt(outputFormat, Path.Combine(path, name));

        Output = new(output);
    }

    private static string GetSupportedFormatExt(CsfFormat format, string? prefix)
    {
        prefix ??= string.Empty;
        return format switch
        {
            CsfFormat.Yaml => $"{prefix}.yaml",
            CsfFormat.JsonV2 => $"{prefix}.v2.csf.json",
            CsfFormat.JsonV1 => $"{prefix}.v1.csf.json",
            CsfFormat.Xml => $"{prefix}.xml",
            CsfFormat.Csf => $"{prefix}.csf",
            _ => throw new NotSupportedException(),
        };
    }
}
