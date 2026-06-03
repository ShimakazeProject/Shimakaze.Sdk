using System.Diagnostics.CodeAnalysis;

using DotMake.CommandLine;

using Shimakaze.Sdk.Csf.Json;
using Shimakaze.Sdk.Csf.Xml;
using Shimakaze.Sdk.Csf.Yaml;

using Spectre.Console;

namespace Shimakaze.Sdk.Csf.Converter;

[CliCommand(Description = "Shimakaze.Sdk Csf 编译器")]
internal sealed class RootCommand
{
    [CliArgument(Description = "输入的文件")]
    public required FileInfo Input { get; set; }

    [CliArgument(Description = "输出的文件", Required = false)]
    public FileInfo? Output { get; set; } = default;

    [CliOption(Description = "输入文件的格式", Required = false)]
    public SupportedFormat InputFormat { get; set; }

    [CliOption(Description = "输出的文件格式", Required = false)]
    public SupportedFormat OutputFormat { get; set; }

    [CliOption(Description = "不要启用交互模式", Required = false)]
    public bool Quiet { get; set; }

    public async Task RunAsync()
    {
        CancellationToken cancellationToken = default;

        await InitInputFormatAsync(cancellationToken);
        await InitOutputFormatAsync(cancellationToken);
        await InitOutputAsync(cancellationToken);

        await using FileStream ifs = Input.OpenRead();
        await using FileStream ofs = Output.Create();

        Task<CsfData> reader = InputFormat switch
        {
            SupportedFormat.Csf => Task.Run(() => CsfReader.ReadAllData(ifs)),
            SupportedFormat.Yaml => Task.Run(() =>
            {
                using StreamReader sr = new(ifs);
                return CsfYamlV1Reader.Read(sr);
            }),
            SupportedFormat.JsonV2 => CsfJsonV2.ReadAllDataAsync(ifs, cancellationToken: cancellationToken),
            SupportedFormat.JsonV1 => CsfJsonV1.ReadAllDataAsync(ifs, cancellationToken: cancellationToken),
            SupportedFormat.Xml => Task.Run(() =>
            {
                using StreamReader sr = new(ifs);
                return CsfXmlV1Reader.Read(sr);
            }),
            _ => throw new NotSupportedException(),
        };

        Func<CsfData, Task> writer = OutputFormat switch
        {
            SupportedFormat.Yaml => async csf => await Task.Run(async () =>
            {
                await using StreamWriter sw = new(ofs);
                CsfYamlV1Writer.Write(sw, csf);
            }),
            SupportedFormat.JsonV2 => async csf => await CsfJsonV2.WriteAllDataAsync(ofs, csf),
            SupportedFormat.JsonV1 => async csf => await CsfJsonV1.WriteAllDataAsync(ofs, csf),
            SupportedFormat.Xml => async csf =>
            {
                await using StreamWriter sw = new(ofs);
                CsfXmlV1Writer.Write(sw, csf);
            }
            ,
            SupportedFormat.Csf => async csf => await Task.Run(() => CsfWriter.WriteAllData(ofs, csf)),
            _ => throw new NotSupportedException()
        };

        await writer(await reader);
    }
    public async Task InitInputFormatAsync(CancellationToken cancellationToken)
    {
        if (InputFormat is not SupportedFormat.Auto)
            return;

        if (Input.Name.EndsWith(".csf", StringComparison.OrdinalIgnoreCase))
            InputFormat = SupportedFormat.Csf;
        else if (Input.Name.EndsWith(".csf.yaml", StringComparison.OrdinalIgnoreCase)
            || Input.Name.EndsWith(".csf.yml", StringComparison.OrdinalIgnoreCase))
            InputFormat = SupportedFormat.Yaml;
        else if (Input.Name.EndsWith(".v2.csf.json", StringComparison.OrdinalIgnoreCase)
            || Input.Name.EndsWith(".csf.v2.json", StringComparison.OrdinalIgnoreCase))
            InputFormat = SupportedFormat.JsonV2;
        else if (Input.Name.EndsWith(".v1.csf.json", StringComparison.OrdinalIgnoreCase)
            || Input.Name.EndsWith(".csf.v1.json", StringComparison.OrdinalIgnoreCase))
            InputFormat = SupportedFormat.JsonV1;
        else if (Input.Name.EndsWith(".csf.xaml", StringComparison.OrdinalIgnoreCase)
            || Input.Name.EndsWith(".csf.xml", StringComparison.OrdinalIgnoreCase))
            InputFormat = SupportedFormat.Xml;
        else if (!Quiet)
            InputFormat = await AnsiConsole.PromptAsync(new SelectionPrompt<SupportedFormat>()
                    .Title("请选择当前文件的格式")
                    .AddChoices(Enum.GetValues<SupportedFormat>().Where(i => i is not SupportedFormat.Auto))
                    .UseConverter(GetSupportedFormatName), cancellationToken);
        else
            throw new InvalidDataException("无法分析出当前文件的格式");
    }

    public async Task InitOutputFormatAsync(CancellationToken cancellationToken)
    {
        if (OutputFormat is not SupportedFormat.Auto)
            return;

        OutputFormat = InputFormat is SupportedFormat.Csf
            ? SupportedFormat.Yaml
            : SupportedFormat.Csf;

        if (!Quiet)
        {
            OutputFormat = await AnsiConsole.PromptAsync(new SelectionPrompt<SupportedFormat>()
                    .Title("请选择要转换的格式")
                    .AddChoices(OutputFormat)
                    .AddChoices(Enum.GetValues<SupportedFormat>().Where(i => i is not SupportedFormat.Auto && i != InputFormat && i != OutputFormat))
                    .UseConverter(GetSupportedFormatName), cancellationToken);
        }
    }

    [MemberNotNull(nameof(Output))]
    private async Task InitOutputAsync(CancellationToken cancellationToken)
    {
        if (Output is not null)
            return;

        string output = GetSupportedFormatExt(OutputFormat, Input.FullName);

        Output = new(output);

        if (!Quiet)
        {
            output = await AnsiConsole.AskAsync("请输入生成的文件的路径", output, cancellationToken);
            output = output.Trim('"');
            Output = new(output);
        }
    }

    private static string GetSupportedFormatExt(SupportedFormat format, string? prefix)
    {
        prefix ??= string.Empty;
        return format switch
        {
            SupportedFormat.Yaml => $"{prefix}.yaml",
            SupportedFormat.JsonV2 => $"{prefix}.v2.csf.json",
            SupportedFormat.JsonV1 => $"{prefix}.v1.csf.json",
            SupportedFormat.Xml => $"{prefix}.xml",
            SupportedFormat.Csf => $"{prefix}.csf",
            _ => throw new NotSupportedException(),
        };
    }

    private static string GetSupportedFormatName(SupportedFormat format)
    {
        return format switch
        {
            SupportedFormat.Yaml => "Shimakaze.Sdk 定义的 Yaml 格式",
            SupportedFormat.JsonV2 => "Shimakaze.Sdk 定义的 Json 格式 第2版",
            SupportedFormat.JsonV1 => "Shimakaze.Sdk 定义的 Json 格式 第1版",
            SupportedFormat.Xml => "Shimakaze.Sdk 定义的 Xml 格式",
            SupportedFormat.Csf => "游戏引擎所使用的 Csf 二进制格式",
            _ => "未知",
        };
    }
}
