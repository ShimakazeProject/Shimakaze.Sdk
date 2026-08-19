using System.Text;
using System.Text.Json;

using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Engine.Ini;
using Shimakaze.Sdk.Inilyn;
using Shimakaze.Sdk.Inilyn.Analyzer.Analysis;
using Shimakaze.Sdk.Inilyn.Compilation;

namespace Shimakaze.Sdk.Engine.Cli.Commands.Ini;

[CliCommand(Description = nameof(Resource.Command_Ini_Compile_Description), Parent = typeof(IniCommand))]
internal sealed class CompileCommand
{
    [CliOption(Description = nameof(Resource.Command_Ini_Compile_Input_Description), Alias = "i", Required = true, AllowMultipleArgumentsPerToken = true)]
    public required List<FileInfo> Input { get; set; }

    [CliOption(Description = nameof(Resource.Command_Ini_Compile_Output_Description), Alias = "o")]
    public FileInfo? Output { get; set; }

    [CliOption(Description = nameof(Resource.Command_Ini_Compile_SourceMap_Description), Alias = "s", Aliases = ["sourcemap"])]
    public FileInfo? SourceMap { get; set; }

    [CliOption(Description = nameof(Resource.Command_Ini_Compile_Verbose_Description), Alias = "v")]
    public bool Verbose { get; set; }

    [CliOption(Description = nameof(Resource.Command_Ini_Compile_NoTreeShaking_Description))]
    public bool NoTreeShaking { get; set; }

    [CliOption(Description = nameof(Resource.Command_Ini_Compile_Rules_Description), Alias = "r", AllowMultipleArgumentsPerToken = true)]
    public List<FileInfo>? Rules { get; set; }

    [CliOption(Description = nameof(Resource.Command_Ini_Compile_Assets_Description), Alias = "a", AllowMultipleArgumentsPerToken = true)]
    public List<FileInfo>? Assets { get; set; }

    [CliOption(Description = nameof(Resource.Command_Ini_Compile_Group_Description), Alias = "g")]
    public string? Group { get; set; }

    public async Task<int> RunAsync()
    {
        await Task.CompletedTask;

        if (Input.Count == 0)
        {
            Console.Error.WriteLine("错误：未指定输入文件。");
            return 1;
        }

        List<InilynFile> files = [];
        foreach (var file in Input)
        {
            if (!file.Exists)
            {
                Console.Error.WriteLine($"错误：文件不存在 - {file.FullName}");
                continue;
            }

            files.Add(InilynFile.Create(file.FullName));
        }

        if (files.Count == 0)
        {
            Console.Error.WriteLine("错误：没有有效的输入文件。");
            return 1;
        }

        if (Verbose)
        {
            Console.WriteLine($"正在编译 {files.Count} 个文件...");
            foreach (var file in files)
            {
                Console.WriteLine($"  - {file.FilePath}");
            }
        }

        IniCompilationOptions options = new()
        {
            // TreeShaking 功能存在问题，暂不启用
            EnableTreeShaking = false,
        };

        IniCompilation compilation = IniCompilation.Create(files, options);
        var result = compilation.Emit();

        IniCliHelper.ReportDiagnostics(result.Diagnostics, Verbose);

        if (!result.Success)
        {
            Console.Error.WriteLine("编译失败。");
            return 1;
        }

        // 确定输出目录
        string outputDir = Output is not null
            ? Output.Extension.Length > 0
                ? Path.GetDirectoryName(Output.FullName)
                    ?? Path.GetDirectoryName(Input[0].FullName)
                    ?? "."
                : Output.FullName
            : Path.GetDirectoryName(Input[0].FullName) ?? ".";

        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        UTF8Encoding utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);
        if (Output?.Extension.Length > 0 && result.OutputFiles.Count > 1)
        {
            string merged = IniTool.MergeOutputFiles(result.OutputFiles);

            await File.WriteAllTextAsync(Output.FullName, merged, utf8NoBom);

            if (Verbose)
            {
                Console.WriteLine($"已生成（合并）：{Output.FullName}");
            }
        }
        else
        {
            foreach (var (fileName, content) in result.OutputFiles)
            {
                string outputPath = Output?.Extension.Length > 0 && result.OutputFiles.Count == 1
                    ? Output.FullName
                    : Path.Combine(outputDir, fileName);

                await File.WriteAllTextAsync(outputPath, content, utf8NoBom);

                if (Verbose)
                {
                    Console.WriteLine($"已生成：{outputPath}");
                }
            }
        }

        if (SourceMap is not null)
        {
            string sourceMapPath = SourceMap.FullName;
            string sourceMapJson = JsonSerializer.Serialize(result.SourceMap, SourceMapJsonContext.Default.SourceMap);
            await File.WriteAllTextAsync(sourceMapPath, sourceMapJson, utf8NoBom);

            if (Verbose)
            {
                Console.WriteLine($"已生成 SourceMap：{sourceMapPath}");
            }
        }

        Console.WriteLine("编译完成。");

        // 编译后分析
        if (Rules is { Count: > 0 })
        {
            string group = string.IsNullOrWhiteSpace(Group) ? "Rule" : Group.Trim();
            var ruleSet = IniCliHelper.LoadRuleSet(Rules);
            if (ruleSet is null)
            {
                return 1;
            }

            var assets = IniCliHelper.LoadAssets(Assets ?? []);
            List<InilynAnalysisInput> analysisInputs = [];
            foreach ((string fileName, string content) in result.OutputFiles)
            {
                analysisInputs.Add(new InilynAnalysisInput(group, fileName, content));
            }

            bool hasErrors = IniCliHelper.RunAnalysis(ruleSet, analysisInputs, assets, Verbose);
            return hasErrors ? 1 : 0;
        }

        return 0;
    }
}
