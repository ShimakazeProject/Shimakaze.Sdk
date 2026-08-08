using Shimakaze.Sdk.Inilyn.Semantic;
using Shimakaze.Sdk.Inilyn.Symbols;
using Shimakaze.Sdk.Inilyn.Syntax;

namespace Shimakaze.Sdk.Inilyn.TreeShaking;

/// <summary>
/// TreeShaking 配置选项。
/// </summary>
public sealed class IniTreeShakerOptions
{
    /// <summary>
    /// 显式指定的入口节名集合。
    /// </summary>
    public IReadOnlyCollection<string> EntrySections { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 是否将未被任何其他节 Mixin 引用的节视为入口（默认 true）。
    /// </summary>
    public bool TreatStandaloneSectionsAsEntries { get; init; } = true;
}

/// <summary>
/// INI TreeShaking 工具。
/// </summary>
/// <remarks>
/// 根据入口节分析引用关系，移除未被任何入口节可达的节。
/// </remarks>
public sealed class IniTreeShaker
{
    /// <summary>
    /// 对语义模型执行 TreeShaking。
    /// </summary>
    /// <param name="model">展平后的语义模型。</param>
    /// <param name="symbolTable">符号表（用于 Mixin 引用关系分析）。</param>
    /// <param name="options">配置选项。</param>
    /// <returns>精简后的语义模型。</returns>
    public static IniSemanticModel Shake(
        IniSemanticModel model,
        IniSymbolTable symbolTable,
        IniTreeShakerOptions? options = null)
    {
        options ??= new IniTreeShakerOptions();

        // 1. 构建引用图：被引用的节 → 引用它的节
        var referencedBy = BuildReferenceGraph(symbolTable);

        // 2. 确定入口节
        var entrySections = DetermineEntrySections(symbolTable, options, referencedBy);

        // 3. 从入口节 BFS，标记可达节
        var reachable = BFS(entrySections, symbolTable);

        // 4. 移除不可达节
        List<IniSemanticSection> keptSections = [];
        List<Diagnostic> diagnostics = [.. model.Diagnostics];

        foreach (var section in model.Sections)
        {
            if (reachable.Contains(section.Name))
            {
                keptSections.Add(section);
            }
            else
            {
                diagnostics.Add(Diagnostic.Create(Diagnostics.SectionRemoved, 0, 0, 0, 0, symbolTable.SourceFileName, section.Name));
            }
        }

        return new IniSemanticModel
        {
            Sections = keptSections,
            GlobalKeys = model.GlobalKeys,
            Diagnostics = diagnostics,
        };
    }

    private static Dictionary<string, HashSet<string>> BuildReferenceGraph(IniSymbolTable symbolTable)
    {
        // referencedBy[target] = { source1, source2, ... } 表示 target 被哪些节引用
        Dictionary<string, HashSet<string>> referencedBy = new(StringComparer.OrdinalIgnoreCase);

        foreach (var section in symbolTable.Sections.Values)
        {
            foreach (var mixinRef in section.MixinRefs)
            {
                if (!referencedBy.TryGetValue(mixinRef.ReferencedSection, out var sources))
                {
                    sources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    referencedBy[mixinRef.ReferencedSection] = sources;
                }

                sources.Add(section.Name);
            }
        }

        return referencedBy;
    }

    private static HashSet<string> DetermineEntrySections(
        IniSymbolTable symbolTable,
        IniTreeShakerOptions options,
        Dictionary<string, HashSet<string>> referencedBy)
    {
        HashSet<string> entries = new(StringComparer.OrdinalIgnoreCase);

        // 添加用户显式指定的入口
        foreach (string entry in options.EntrySections)
        {
            entries.Add(entry);
        }

        // 将未被任何其他节引用的节视为入口
        if (options.TreatStandaloneSectionsAsEntries)
        {
            foreach (var section in symbolTable.Sections.Values)
            {
                if (!referencedBy.ContainsKey(section.Name))
                {
                    entries.Add(section.Name);
                }
            }
        }

        return entries;
    }

    private static HashSet<string> BFS(HashSet<string> entrySections, IniSymbolTable symbolTable)
    {
        HashSet<string> reachable = new(StringComparer.OrdinalIgnoreCase);
        Queue<string> queue = new();

        foreach (string entry in entrySections)
        {
            if (symbolTable.Sections.ContainsKey(entry) && reachable.Add(entry))
            {
                queue.Enqueue(entry);
            }
        }

        while (queue.Count > 0)
        {
            string current = queue.Dequeue();

            if (!symbolTable.Sections.TryGetValue(current, out var section))
            {
                continue;
            }

            foreach (var mixinRef in section.MixinRefs)
            {
                if (reachable.Add(mixinRef.ReferencedSection))
                {
                    queue.Enqueue(mixinRef.ReferencedSection);
                }
            }
        }

        return reachable;
    }
}
