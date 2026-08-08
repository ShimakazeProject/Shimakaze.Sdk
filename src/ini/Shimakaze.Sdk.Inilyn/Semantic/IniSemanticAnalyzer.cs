using Shimakaze.Sdk.Inilyn.Symbols;
using Shimakaze.Sdk.Inilyn.Syntax;
using Shimakaze.Sdk.Inilyn.Syntax.Nodes;

namespace Shimakaze.Sdk.Inilyn.Semantic;

/// <summary>
/// INI 语义分析器。
/// </summary>
/// <remarks>
/// <para>
/// 在符号表基础上进行 Mixin 引用验证、循环引用检测和 Mixin 展开，
/// 产出展平后的 <see cref="IniSemanticModel"/>。
/// </para>
/// </remarks>
public sealed class IniSemanticAnalyzer
{
    private readonly List<Diagnostic> _diagnostics = [];
    private string? _filePath;

    /// <summary>
    /// 对给定的符号表执行语义分析。
    /// </summary>
    /// <param name="symbolTable">符号表。</param>
    /// <returns>展平后的语义模型。</returns>
    public static IniSemanticModel Analyze(IniSymbolTable symbolTable)
    {
        IniSemanticAnalyzer analyzer = new() { _filePath = symbolTable.SourceFileName };
        return analyzer.AnalyzeCore(symbolTable);
    }

    private IniSemanticModel AnalyzeCore(IniSymbolTable symbolTable)
    {
        // 1. 验证 Mixin 引用（引用的节是否存在）
        ValidateMixinReferences(symbolTable);

        // 2. 检测循环引用
        DetectCircularReferences(symbolTable);

        // 3. 展平所有节（Mixin 展开）
        var sections = FlattenSections(symbolTable);

        // 4. 收集全局键
        var globalKeys = FlattenGlobalKeys(symbolTable);

        return new IniSemanticModel
        {
            Sections = sections,
            GlobalKeys = globalKeys,
            Diagnostics = _diagnostics,
        };
    }

    private void ValidateMixinReferences(IniSymbolTable symbolTable)
    {
        foreach (var mixinRef in symbolTable.AllMixinRefs)
        {
            if (!symbolTable.Sections.ContainsKey(mixinRef.ReferencedSection))
            {
                _diagnostics.Add(
                    Diagnostic.Create(
                        Diagnostics.MixinSectionNotFound,
                        mixinRef.StartLine,
                        mixinRef.StartColumn,
                        mixinRef.EndLine,
                        mixinRef.EndColumn,
                        _filePath,
                        mixinRef.ReferencedSection
                    )
                );
            }
        }
    }

    private void DetectCircularReferences(IniSymbolTable symbolTable)
    {
        foreach (var section in symbolTable.Sections.Values)
        {
            if (section.MixinRefs.Count == 0)
            {
                continue;
            }

            HashSet<string> visited = new(StringComparer.OrdinalIgnoreCase)
            {
                section.Name
            };

            DetectCircularReferencesCore(section, symbolTable, visited, section);
        }
    }

    private void DetectCircularReferencesCore(
        IniSectionSymbol current,
        IniSymbolTable symbolTable,
        HashSet<string> visited,
        IniSectionSymbol originalSection)
    {
        foreach (var mixinRef in current.MixinRefs)
        {
            if (!symbolTable.Sections.TryGetValue(mixinRef.ReferencedSection, out var referenced))
            {
                continue;
            }

            if (!visited.Add(mixinRef.ReferencedSection))
            {
                // 循环引用检测到
                _diagnostics.Add(
                    Diagnostic.Create(
                        Diagnostics.MixinCircularReference,
                        mixinRef.StartLine,
                        mixinRef.StartColumn,
                        mixinRef.EndLine,
                        mixinRef.EndColumn,
                        _filePath,
                        originalSection.Name
                    )
                );
                continue;
            }

            DetectCircularReferencesCore(referenced, symbolTable, visited, originalSection);
            visited.Remove(mixinRef.ReferencedSection);
        }
    }

    private static List<IniSemanticSection> FlattenSections(IniSymbolTable symbolTable)
    {
        List<IniSemanticSection> result = [];

        foreach (var section in symbolTable.Sections.Values)
        {
            List<IniSemanticKeyValue> keyValues = [];

            // 从左到右处理 Mixin 引用（浅拷贝合并，后者覆盖前者）
            foreach (var mixinRef in section.MixinRefs)
            {
                if (symbolTable.Sections.TryGetValue(mixinRef.ReferencedSection, out var referenced))
                {
                    foreach (var key in referenced.Keys.Values)
                    {
                        keyValues.Add(new IniSemanticKeyValue(key.Name, GetKeyValueText(key), mixinRef.ReferencedSection));
                    }
                }
            }

            // 当前节的键值对覆盖 Mixin 来源
            foreach (var key in section.Keys.Values)
            {
                // 移除已存在的同名键（从 Mixin 来源）
                keyValues.RemoveAll(kv => string.Equals(kv.Key, key.Name, StringComparison.OrdinalIgnoreCase));
                keyValues.Add(new IniSemanticKeyValue(key.Name, GetKeyValueText(key)));
            }

            result.Add(new IniSemanticSection(section.Name, keyValues));
        }

        return result;
    }

    private static List<IniSemanticKeyValue> FlattenGlobalKeys(IniSymbolTable symbolTable)
    {
        List<IniSemanticKeyValue> result = [];

        foreach (var key in symbolTable.GlobalKeys.Values)
        {
            result.Add(new IniSemanticKeyValue(key.Name, GetKeyValueText(key)));
        }

        return result;
    }

    private static string GetKeyValueText(IniKeySymbol key)
    {
        if (key.DeclaredAt is IniKeyValueEntry entry)
        {
            return entry.Value.Text;
        }

        return string.Empty;
    }
}
