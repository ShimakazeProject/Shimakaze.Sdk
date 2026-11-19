using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Shimakaze.Sdk.Analyzer.Common.Types;
using Shimakaze.Sdk.Ra2lyn.Common.Source;

using Ra2lynEngineCompiler = Shimakaze.Sdk.Engine.Ra2lyn.Ra2lynCompiler;
using Ra2lynCompileOptions = Shimakaze.Sdk.Engine.Ra2lyn.Ra2lynCompileOptions;
using Ra2lynCompileResult = Shimakaze.Sdk.Engine.Ra2lyn.Ra2lynCompileResult;
using Task = Microsoft.Build.Utilities.Task;

namespace Shimakaze.Sdk.Engine.MSBuild;

/// <summary>
/// Ra2lyn 编译器：把 INI 源文件按规则编译成输出目录中的产物。
/// </summary>
public sealed class Ra2lynCompiler : Task
{
    /// <summary>
    /// 待编译的 INI 源文件列表。
    /// </summary>
    [Required]
    public ITaskItem[] Sources { get; set; } = [];

    /// <summary>
    /// 规则 XML 文件列表（可选）。
    /// </summary>
    public ITaskItem[] RuleFiles { get; set; } = [];

    /// <summary>
    /// 输出目录。
    /// </summary>
    [Required]
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// 预处理器宏（可选，默认空）。
    /// </summary>
    public string[] Defines { get; set; } = [];

    /// <summary>
    /// 是否启用 TreeShaking（可选，默认 false）。
    /// </summary>
    public bool TreeShaking { get; set; }

    /// <summary>
    /// 校验模式（可选，默认 "Loose"）。
    /// </summary>
    public string ValidationMode { get; set; } = "Loose";

    /// <summary>
    /// 组名 → 输出文件名映射（可选）。每项 Identity 为组名，TargetName 元数据为输出文件名。
    /// 未在映射中的组回退到 <c>{组名}.ini</c>。
    /// </summary>
    public ITaskItem[] GroupOutputNames { get; set; } = [];

    /// <summary>
    /// 写出的文件列表。
    /// </summary>
    [Output]
    public ITaskItem[]? OutputFiles { get; set; }

    public override bool Execute()
    {
        if (Sources.Length is 0)
        {
            Log.LogMessage(MessageImportance.Low, "没有需要编译的 INI 文件。");
            return true;
        }

        if (!Enum.TryParse<ValidationMode>(ValidationMode, ignoreCase: true, out var validationMode))
        {
            Log.LogError(
                "Ra2lyn",
                "RA2LYN0001",
                string.Empty,
                string.Empty,
                0,
                0,
                0,
                0,
                "不支持的校验模式：{0}",
                ValidationMode);
            return false;
        }

        FileInfo[] sources = [.. Sources.Select(static item => new FileInfo(item.GetMetadata("FullPath")))];
        FileInfo[] ruleFiles = [.. RuleFiles.Select(static item => new FileInfo(item.GetMetadata("FullPath")))];
        DirectoryInfo outputDir = new(OutputPath);

        Ra2lynCompileOptions options = new()
        {
            Defines = Defines,
            TreeShaking = TreeShaking,
            ValidationMode = validationMode,
            GroupOutputNames = GroupOutputNames.ToDictionary(
                static item => item.ItemSpec,
                static item => item.GetMetadata("TargetName"),
                StringComparer.OrdinalIgnoreCase),
        };

        Ra2lynCompileResult result;
        try
        {
            result = new Ra2lynEngineCompiler().CompileAsync(sources, ruleFiles, outputDir, options)
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex, true, true, null);
            return false;
        }

        LogDiagnostics(result);

        if (result.HasError)
        {
            return false;
        }

        OutputFiles = [.. result.FilePaths.Select(static path => new TaskItem(path))];
        return true;
    }

    private void LogDiagnostics(Ra2lynCompileResult result)
    {
        foreach (var diagnostic in result.Diagnostics)
        {
            string file = string.Empty;
            int lineNumber = 0;
            int columnNumber = 0;

            if (diagnostic.Span.Path is { } path)
            {
                file = path;
                SourceFile? source = result.Sources.FirstOrDefault(
                    item => string.Equals(item.Path, path, StringComparison.Ordinal));
                if (source is not null)
                {
                    (int line, int column) = SourceLocator.GetPosition(source, diagnostic.Span.Start);
                    lineNumber = line + 1;
                    columnNumber = column + 1;
                }
            }

            switch (diagnostic.Severity)
            {
                case DiagnosticSeverity.Error:
                    Log.LogError(
                        "Ra2lyn",
                        diagnostic.Code,
                        string.Empty,
                        file,
                        lineNumber,
                        columnNumber,
                        0,
                        0,
                        diagnostic.Message);
                    break;

                case DiagnosticSeverity.Warning:
                    Log.LogWarning(
                        "Ra2lyn",
                        diagnostic.Code,
                        string.Empty,
                        file,
                        lineNumber,
                        columnNumber,
                        0,
                        0,
                        diagnostic.Message);
                    break;

                default:
                    Log.LogMessage(
                        MessageImportance.Normal,
                        "{0}: {1}",
                        diagnostic.Code,
                        diagnostic.Message);
                    break;
            }
        }
    }
}
