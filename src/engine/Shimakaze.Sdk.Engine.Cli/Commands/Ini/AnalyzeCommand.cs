using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Inilyn.Analyzer.Analysis;
using Shimakaze.Sdk.Inilyn.Analyzer.RuleSet;

namespace Shimakaze.Sdk.Engine.Cli.Commands.Ini;

[CliCommand(Description = nameof(Resource.Command_Ini_Analyze_Description), Parent = typeof(IniCommand))]
internal sealed class AnalyzeCommand
{
    [CliOption(Description = nameof(Resource.Command_Ini_Analyze_Rules_Description), Alias = "r", Required = true, AllowMultipleArgumentsPerToken = true)]
    public required List<FileInfo> Rules { get; set; }

    [CliOption(Description = nameof(Resource.Command_Ini_Analyze_Input_Description), Alias = "i", Required = true, AllowMultipleArgumentsPerToken = true)]
    public required List<FileInfo> Input { get; set; }

    [CliOption(Description = nameof(Resource.Command_Ini_Analyze_Group_Description), Alias = "g", AllowMultipleArgumentsPerToken = true)]
    public List<string>? Group { get; set; }

    [CliOption(Description = nameof(Resource.Command_Ini_Analyze_Assets_Description), Alias = "a", AllowMultipleArgumentsPerToken = true)]
    public List<FileInfo>? Assets { get; set; }

    [CliOption(Description = nameof(Resource.Command_Ini_Analyze_Verbose_Description), Alias = "v")]
    public bool Verbose { get; set; }

    public async Task<int> RunAsync()
    {
        await Task.CompletedTask;

        if (Rules.Count == 0 || Input.Count == 0)
        {
            Console.Error.WriteLine("错误：需要 --rules 与 --input。");
            return 1;
        }

        var ruleSet = IniCliHelper.LoadRuleSet(Rules);
        if (ruleSet is null)
        {
            return 1;
        }

        var assets = IniCliHelper.LoadAssets(Assets ?? []);

        List<InilynAnalysisInput> inputs = [];
        for (int i = 0; i < Input.Count; i++)
        {
            string group = Group is not null && i < Group.Count && !string.IsNullOrWhiteSpace(Group[i])
                ? Group[i].Trim()
                : (Group is { Count: > 0 } ? Group[^1].Trim() : "Rule");

            if (!Input[i].Exists)
            {
                Console.Error.WriteLine($"错误：文件不存在 - {Input[i].FullName}");
                continue;
            }

            inputs.Add(new InilynAnalysisInput(group, Input[i].FullName, File.ReadAllText(Input[i].FullName)));
        }

        if (inputs.Count == 0)
        {
            Console.Error.WriteLine("错误：没有可分析的文件。");
            return 1;
        }

        bool hasErrors = IniCliHelper.RunAnalysis(ruleSet, inputs, assets, Verbose);
        if (!hasErrors)
        {
            Console.WriteLine("分析通过。");
        }

        return hasErrors ? 1 : 0;
    }
}
