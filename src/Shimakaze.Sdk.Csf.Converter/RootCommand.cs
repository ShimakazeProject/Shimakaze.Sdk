using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;

using DotMake.CommandLine;

using Sharprompt;

using Shimakaze.Sdk.Csf.Json;
using Shimakaze.Sdk.Csf.Xml;
using Shimakaze.Sdk.Csf.Yaml;

namespace Shimakaze.Sdk.Csf.Converter;

[CliCommand(Description = "Shimakaze.Sdk Csf 编译器")]
internal sealed class RootCommand
{
    [CliArgument(Description = "输入的文件")]
    public required FileInfo Input { get; set; }

    [CliArgument(Description = "输出的文件", Required = false)]
    public FileInfo? Output { get; set; }

    [CliOption(Description = "输入文件的格式", Required = false)]
    public SupportedFormat InputFormat { get; set; }

    [CliOption(Description = "输出的文件格式", Required = false)]
    public SupportedFormat OutputFormat { get; set; }

    [CliOption(Description = "不要启用交互模式", Required = false)]
    public bool Quiet { get; set; }


    private static readonly JsonSerializerOptions Options = new()
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public async Task RunAsync()
    {
        InitInputFormat();
        InitOutputFormat();
        InitOutput();

        await using var ifs = Input.OpenRead();
        await using var ofs = Output.Create();

        Task<CsfDocument> reader = InputFormat switch
        {
            SupportedFormat.Csf => Task.Run(() => CsfReader.Read(ifs)),
            SupportedFormat.Yaml => Task.Run(() =>
            {
                using StreamReader sr = new(ifs);
                return CsfYamlV1Reader.Read(sr);
            }),
            SupportedFormat.JsonV2 => CsfJsonV2Reader.ReadAsync(ifs, Options),
            SupportedFormat.JsonV1 => CsfJsonV1Reader.ReadAsync(ifs, Options),
            SupportedFormat.Xml => Task.Run(() =>
            {
                using StreamReader sr = new(ifs);
                return CsfXmlV1Reader.Read(sr);
            }),
            _ => throw new NotSupportedException(),
        };

        Func<CsfDocument, Task> writer = OutputFormat switch
        {
            SupportedFormat.Yaml => async csf => await Task.Run(async () =>
            {
                await using StreamWriter sw = new(ofs);
                CsfYamlV1Writer.Write(sw, csf);
            }),
            SupportedFormat.JsonV2 => async csf => await CsfJsonV2Writer.WriteAsync(ofs, csf, Options),
            SupportedFormat.JsonV1 => async csf => await CsfJsonV1Writer.WriteAsync(ofs, csf, Options),
            SupportedFormat.Xml => async csf =>
            {
                await using StreamWriter sw = new(ofs);
                CsfXmlV1Writer.Write(sw, csf, new() { Indent = true });
            }
            ,
            SupportedFormat.Csf => async csf => await Task.Run(() => CsfWriter.Write(ofs, csf)),
            _ => throw new NotSupportedException()
        };

        await writer(await reader);
    }

    private void InitInputFormat()
    {
        if (InputFormat is not SupportedFormat.Auto)
            return;

        if (Input.Name.EndsWith(".csf", StringComparison.OrdinalIgnoreCase))
        {
            InputFormat = SupportedFormat.Csf;
        }
        else if (Input.Name.EndsWith(".csf.yaml", StringComparison.OrdinalIgnoreCase)
            || Input.Name.EndsWith(".csf.yml", StringComparison.OrdinalIgnoreCase))
        {
            InputFormat = SupportedFormat.Yaml;
        }
        else if (Input.Name.EndsWith(".v2.csf.json", StringComparison.OrdinalIgnoreCase)
            || Input.Name.EndsWith(".csf.v2.json", StringComparison.OrdinalIgnoreCase))
        {
            InputFormat = SupportedFormat.JsonV2;
        }
        else if (Input.Name.EndsWith(".v1.csf.json", StringComparison.OrdinalIgnoreCase)
            || Input.Name.EndsWith(".csf.v1.json", StringComparison.OrdinalIgnoreCase))
        {
            InputFormat = SupportedFormat.JsonV1;
        }
        else if (Input.Name.EndsWith(".csf.xaml", StringComparison.OrdinalIgnoreCase)
            || Input.Name.EndsWith(".csf.xml", StringComparison.OrdinalIgnoreCase))
        {
            InputFormat = SupportedFormat.Xml;
        }
        else if (!Quiet)
        {
            InputFormat = Prompt.Select(
                "请选择当前文件的格式",
                Enum.GetValues<SupportedFormat>().Where(i => i is not SupportedFormat.Auto),
                textSelector: GetSupportedFormatName);
        }
        else
        {
            Console.Error.WriteLine("无法分析出当前文件的格式");
            Environment.Exit(1);
        }

    }

    private void InitOutputFormat()
    {
        if (OutputFormat is not SupportedFormat.Auto)
            return;

        OutputFormat = InputFormat is SupportedFormat.Csf
            ? SupportedFormat.Yaml
            : SupportedFormat.Csf;

        if (!Quiet)
        {
            OutputFormat = Prompt.Select(
                "请选择要转换的格式",
                Enum.GetValues<SupportedFormat>().Where(i => i is not SupportedFormat.Auto && i != InputFormat),
                defaultValue: OutputFormat,
                textSelector: GetSupportedFormatName);
        }
    }

    [MemberNotNull(nameof(Output))]
    private void InitOutput()
    {
        if (Output is not null)
            return;

        string output = GetSupportedFormatExt(OutputFormat, Input.FullName);

        if (!Quiet)
            output = Prompt.Input<string>("请输入生成的文件的路径", output);

        Output = new(output);
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

    private static string GetSupportedFormatName(SupportedFormat format) => format switch
    {
        SupportedFormat.Yaml => "Shimakaze.Sdk 定义的 Yaml 格式",
        SupportedFormat.JsonV2 => "Shimakaze.Sdk 定义的 Json 格式 第2版",
        SupportedFormat.JsonV1 => "Shimakaze.Sdk 定义的 Json 格式 第1版",
        SupportedFormat.Xml => "Shimakaze.Sdk 定义的 Xml 格式",
        SupportedFormat.Csf => "游戏引擎所使用的 Csf 二进制格式",
        _ => "未知",
    };
}