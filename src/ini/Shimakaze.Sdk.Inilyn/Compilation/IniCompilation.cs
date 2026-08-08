using Shimakaze.Sdk.Inilyn.CodeGeneration;
using Shimakaze.Sdk.Inilyn.Semantic;
using Shimakaze.Sdk.Inilyn.SourceMapping;
using Shimakaze.Sdk.Inilyn.Symbols;
using Shimakaze.Sdk.Inilyn.Syntax;
using Shimakaze.Sdk.Inilyn.Syntax.Nodes;
using Shimakaze.Sdk.Inilyn.Text;
using Shimakaze.Sdk.Inilyn.TreeShaking;

namespace Shimakaze.Sdk.Inilyn.Compilation;

/// <summary>
/// INI 编译器。
/// </summary>
/// <remarks>
/// <para>
/// 编排完整的编译管线：SourceText → Lexer → Parser → SymbolBuilder → SemanticAnalyzer → TreeShaking → CodeGeneration。
/// </para>
/// <para>
/// 支持多文件编译。每个文件独立解析和符号化后，合并为全局符号表进行语义分析，
/// 再按文件独立进行 TreeShaking 和代码生成。
/// </para>
/// </remarks>
public sealed class IniCompilation
{
    private readonly IReadOnlyList<InilynFile> _files;
    private readonly IniCompilationOptions _options;

    private IniCompilation(IReadOnlyList<InilynFile> files, IniCompilationOptions options)
    {
        _files = files;
        _options = options;
    }

    /// <summary>
    /// 创建编译实例。
    /// </summary>
    /// <param name="files">待编译的文件列表。</param>
    /// <returns>编译实例。</returns>
    public static IniCompilation Create(IReadOnlyList<InilynFile> files)
        => new(files, new IniCompilationOptions());

    /// <summary>
    /// 创建编译实例。
    /// </summary>
    /// <param name="files">待编译的文件列表。</param>
    /// <param name="options">编译选项。</param>
    /// <returns>编译实例。</returns>
    public static IniCompilation Create(IReadOnlyList<InilynFile> files, IniCompilationOptions options)
        => new(files, options);

    /// <summary>
    /// 执行编译并输出结果。
    /// </summary>
    /// <returns>编译结果。</returns>
    public InilynCompilationResult Emit()
    {
        List<Diagnostic> allDiagnostics = [];
        Dictionary<string, string> outputFiles = new(StringComparer.OrdinalIgnoreCase);
        SourceMap sourceMap = new();

        // 1. 每个文件独立：SourceText → Lexer → Parser → SymbolBuilder
        Dictionary<InilynFile, (IniSymbolTable Table, IniSyntaxTree SyntaxTree)> perFileResults = [];

        foreach (var file in _files)
        {
            var result = BuildSymbolsForFile(file);
            perFileResults[file] = (result.Table, result.SyntaxTree);
            allDiagnostics.AddRange(result.Diagnostics);
        }

        // 2. 从所有文件的语法树生成 SourceMap
        foreach (var file in _files)
        {
            if (perFileResults.TryGetValue(file, out var fileResult))
            {
                BuildSourceMap(fileResult.SyntaxTree, sourceMap);
            }
        }

        // 3. 合并所有文件的符号表到全局符号表
        var globalSymbolTable = MergeSymbolTables(perFileResults.Values.Select(r => r.Table));

        // 4. 全局语义分析
        var globalModel = IniSemanticAnalyzer.Analyze(globalSymbolTable);
        allDiagnostics.AddRange(globalModel.Diagnostics);

        // 5. 代码生成（合并所有节到单个模型）
        IniSemanticModel combinedModel = new()
        {
            Sections = [.. globalModel.Sections],
            GlobalKeys = [.. globalModel.GlobalKeys],
            Diagnostics = globalModel.Diagnostics,
        };

        var outputModel = combinedModel;

        // 6. TreeShaking（默认启用，可通过选项停用）
        if (_options.EnableTreeShaking)
        {
            outputModel = IniTreeShaker.Shake(combinedModel, globalSymbolTable);
            allDiagnostics.AddRange(outputModel.Diagnostics.Where(d => d.Code == Diagnostics.SectionRemoved.Id));
        }

        var output = IniCodeGenerator.Generate(outputModel, _files[0].FileName);
        outputFiles[_files[0].FileName] = output.ToString();

        return new InilynCompilationResult
        {
            OutputFiles = outputFiles,
            SourceMap = sourceMap,
            Diagnostics = allDiagnostics,
        };
    }

    private static FileBuildResult BuildSymbolsForFile(InilynFile file)
    {
        string source = File.ReadAllText(file.FilePath);
        SourceText sourceText = SourceText.Create(source, file.FilePath);
        var syntaxTree = Syntax.Parsing.IniParser.Parse(sourceText);

        IniSymbolTable symbolTable = new(file.FilePath);
        IniSymbolBuilder.Build(syntaxTree, symbolTable);

        return new FileBuildResult(symbolTable, syntaxTree, [.. symbolTable.Diagnostics]);
    }

    private static void BuildSourceMap(IniSyntaxTree syntaxTree, SourceMap sourceMap)
    {
        if (syntaxTree.Root is not IniCompilationUnit compilationUnit)
        {
            return;
        }

        var sourceText = syntaxTree.SourceText;

        foreach (var entry in compilationUnit.Entries)
        {
            if (entry is IniSectionDecl section)
            {
                var (line, column) = sourceText.GetPosition(section.Start);
                SourceMapSection sectionInfo = new()

                {
                    Name = section.Name.Text,
                    Line = line,
                    Column = column,
                };

                foreach (var child in section.Children)
                {
                    if (child is IniKeyValueEntry kv)
                    {
                        var (keyLine, keyColumn) = sourceText.GetPosition(kv.Start);
                        var (valueLine, valueColumn) = sourceText.GetPosition(kv.Value.Start);

                        SourceMapKey keyInfo = new()

                        {
                            Name = kv.Key.Text,
                            Line = keyLine,
                            Column = keyColumn,
                            Value = kv.Value.Text,
                            ValueLine = valueLine,
                            ValueColumn = valueColumn,
                        };

                        sectionInfo.AddKey(kv.Key.Text, keyInfo);
                    }
                }

                sourceMap.AddSection(section.Name.Text, sectionInfo);
            }
        }
    }

    private static IniSymbolTable MergeSymbolTables(IEnumerable<IniSymbolTable> tables)
    {
        IniSymbolTable merged = new();

        foreach (var table in tables)
        {
            foreach (var section in table.Sections)
            {
                if (merged.Sections.TryGetValue(section.Key, out var existing))
                {
                    // 合并键：后者的键覆盖前者的同名键，保留独有的键
                    foreach (var key in section.Value.Keys)
                    {
                        existing.Keys[key.Key] = key.Value;
                    }

                    // 合并 Mixin 引用
                    existing.MixinRefs.AddRange(section.Value.MixinRefs);
                }
                else
                {
                    merged.Sections[section.Key] = section.Value;
                }
            }

            foreach (var kv in table.GlobalKeys)
            {
                merged.GlobalKeys[kv.Key] = kv.Value;
            }

            merged.AllMixinRefs.AddRange(table.AllMixinRefs);
            merged.Diagnostics.AddRange(table.Diagnostics);
        }

        return merged;
    }

    private static IniSemanticModel FilterModelByFile(IniSemanticModel globalModel, IniSymbolTable fileSymbolTable)
    {
        // 筛选出属于当前文件的节
        HashSet<string> fileSectionNames = new(StringComparer.OrdinalIgnoreCase);
        foreach (var section in fileSymbolTable.Sections.Values)
        {
            fileSectionNames.Add(section.Name);
        }

        List<IniSemanticSection> fileSections = [];
        foreach (var section in globalModel.Sections)
        {
            if (fileSectionNames.Contains(section.Name))
            {
                fileSections.Add(section);
            }
        }

        // 筛选出属于当前文件的全局键
        List<IniSemanticKeyValue> fileGlobalKeys = [];
        foreach (var kv in globalModel.GlobalKeys)
        {
            if (fileSymbolTable.GlobalKeys.ContainsKey(kv.Key))
            {
                fileGlobalKeys.Add(kv);
            }
        }

        return new IniSemanticModel
        {
            Sections = fileSections,
            GlobalKeys = fileGlobalKeys,
            Diagnostics = globalModel.Diagnostics,
        };
    }

    private readonly record struct FileBuildResult(IniSymbolTable Table, IniSyntaxTree SyntaxTree, IReadOnlyList<Diagnostic> Diagnostics);
}
