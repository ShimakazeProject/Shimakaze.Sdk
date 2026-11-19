using DotMake.CommandLine;

using Shimakaze.Sdk.Ra2lyn.Analyzer.Common.Types;
using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Engine.Ra2lyn;

namespace Shimakaze.Sdk.Engine.Cli.Commands.Ra2lyn;

/// <summary>
/// 编译 INI 源文件：前端解析成 IR，后端完成分类/引用/发现/可达性分析后写回 INI。
/// </summary>
[CliCommand(Description = nameof(Resource.Command_Ra2lyn_Build_Description), Parent = typeof(Ra2lynCommand))]
internal sealed class BuildCommand
{
    /// <summary>
    /// 要编译的 INI 源文件。
    /// </summary>
    [CliOption(Description = nameof(Resource.Command_Ra2lyn_Build_Input_Description), Alias = "i")]
    public required List<FileInfo> Input { get; set; }

    /// <summary>
    /// 编译输出目录。
    /// </summary>
    [CliOption(Description = nameof(Resource.Command_Ra2lyn_Build_Output_Description), Alias = "o", Required = false)]
    public DirectoryInfo? Output { get; set; }

    /// <summary>
    /// 规则 XML 文件。
    /// </summary>
    [CliOption(Description = nameof(Resource.Command_Ra2lyn_Build_Rules_Description), Alias = "r", Required = false, AllowMultipleArgumentsPerToken = true)]
    public List<FileInfo> Rules { get; set; } = [];

    /// <summary>
    /// 预处理器宏，形如 <c>--defines=A,B,C</c>；列出的名字在条件指令中为真。
    /// </summary>
    [CliOption(Description = nameof(Resource.Command_Ra2lyn_Build_Defines_Description), Alias = "d", Required = false, AllowMultipleArgumentsPerToken = true)]
    public List<string> Defines { get; set; } = [];

    /// <summary>
    /// 规则组；缺省按文件扩展名推断。
    /// </summary>
    [CliOption(Description = nameof(Resource.Command_Ra2lyn_Build_Group_Description), Alias = "g", Required = false)]
    public string? Group { get; set; }

    /// <summary>
    /// 是否丢弃不可达实体。
    /// </summary>
    [CliOption(Description = nameof(Resource.Command_Ra2lyn_Build_TreeShaking_Description), Required = false)]
    public bool TreeShaking { get; set; }

    /// <summary>
    /// 值校验模式。
    /// </summary>
    [CliOption(Description = nameof(Resource.Command_Ra2lyn_Build_ValidationMode_Description), Required = false)]
    public ValidationMode ValidationMode { get; set; } = ValidationMode.Loose;

    /// <summary>
    /// 输出换行符。
    /// </summary>
    [CliOption(Description = nameof(Resource.Command_Ra2lyn_Build_NewLine_Description), Required = false)]
    public Ra2lynNewLine NewLine { get; set; } = Ra2lynNewLine.Crlf;

    /// <summary>
    /// 执行编译。
    /// </summary>
    /// <remarks>
    /// 命令处理函数的返回类型必须是 <see cref="Task"/>、<see cref="Task{TResult}"/>（整数退出码）或 void；
    /// 返回实数任务泛型会被生成器判定为非处理函数，子命令因此只显示帮助。诊断统一写到标准输出。
    /// </remarks>
    public async Task RunAsync()
    {
        Ra2lynCompiler compiler = new();
        Ra2lynCompileResult result = await compiler.CompileAsync(
            Input,
            Rules,
            Output ?? new DirectoryInfo("."),
            new Ra2lynCompileOptions
            {
                Group = Group,
                Defines = [.. Defines.SelectMany(static value => value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))],
                TreeShaking = TreeShaking,
                ValidationMode = ValidationMode,
                NewLine = NewLine is Ra2lynNewLine.Lf ? "\n" : "\r\n",
            });

        string diagnostics = Ra2lynCompiler.FormatDiagnostics(result);
        if (diagnostics.Length is not 0)
            Console.WriteLine(diagnostics);
    }
}

/// <summary>
/// 输出换行符选项。
/// </summary>
internal enum Ra2lynNewLine
{
    /// <summary>
    /// Windows 换行 <c>\r\n</c>。
    /// </summary>
    Crlf,

    /// <summary>
    /// Unix 换行 <c>\n</c>。
    /// </summary>
    Lf,
}
