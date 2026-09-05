using System.Diagnostics.CodeAnalysis;

using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Engine.Csf;

namespace Shimakaze.Sdk.Engine.Cli.Commands.Csf;

[CliCommand(Description = nameof(Resource.Command_Csf_Build_Description), Parent = typeof(CsfCommand))]
internal sealed class BuildCommand
{
    [CliOption(Description = nameof(Resource.Command_Csf_Build_Input_Description), Alias = "i")]
    public required FileInfo Input { get; set; }

    [CliOption(Description = nameof(Resource.Command_Csf_Build_Output_Description), Alias = "o", Required = false)]
    public FileInfo? Output { get; set; } = null;

    [CliOption(Description = nameof(Resource.Command_Csf_Build_InputFormat_Description), Alias = "if", Required = false)]
    public CsfFormat? InputFormat { get; set; }

    [CliOption(Description = nameof(Resource.Command_Csf_Build_OutputFormat_Description), Alias = "of", Required = false)]
    public CsfFormat? OutputFormat { get; set; }

    public async Task RunAsync()
    {
        InputFormat ??= CsfTool.GuessInputFormat(Input);
        OutputFormat ??= CsfTool.GuessOutputFormat(InputFormat.Value);
        InitOutput(OutputFormat.Value);

        await using var ifs = Input.OpenRead();
        var data = CsfTool.LoadFrom(ifs, InputFormat.Value);

        await using var ofs = Output.Create();
        CsfTool.SaveTo(data, ofs, OutputFormat.Value);
    }

    [MemberNotNull(nameof(Output))]
    private void InitOutput(CsfFormat outputFormat)
    {
        if (Output is not null)
            return;

        string path = Input.DirectoryName ?? throw new DirectoryNotFoundException();
        string name = Input.Name.Split('.', 2).First();

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
