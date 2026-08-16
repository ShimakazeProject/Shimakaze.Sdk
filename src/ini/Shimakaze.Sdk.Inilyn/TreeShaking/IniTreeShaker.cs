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
/// 被 Mixin 引用的节（如 <c>:[Base1]</c> 的 <c>Base1</c>）内容已内联到引用方，
/// 不再作为独立节保留，除非被键/规则显式指定为入口。
/// </remarks>
public sealed class IniTreeShaker
{
    /// <summary>
    /// 对语义模型执行 TreeShaking。
    /// </summary>
    /// <param name="model">展平后的语义模型。</param>
    /// <param name="symbolTable">符号表（用于识别被 Mixin 引用的节）。</param>
    /// <param name="options">配置选项。</param>
    /// <returns>精简后的语义模型。</returns>
    public static IniSemanticModel Shake(
        IniSemanticModel model,
        IniSymbolTable symbolTable,
        IniTreeShakerOptions? options = null)
    {
        options ??= new IniTreeShakerOptions();

        // 1. 收集被 Mixin 引用的节。这些节的内容已内联到引用方，
        //    不再作为独立节保留，除非被显式入口（键/规则）指定。
        var mixinTargets = CollectMixinTargets(symbolTable);

        // 2. 确定入口节
        var entrySections = DetermineEntrySections(symbolTable, options, mixinTargets);

        // 3. 仅保留入口节（不因 Mixin 引用而保留目标节）
        List<IniSemanticSection> keptSections = [];
        List<Diagnostic> diagnostics = [.. model.Diagnostics];

        foreach (var section in model.Sections)
        {
            if (entrySections.Contains(section.Name))
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

    private static HashSet<string> CollectMixinTargets(IniSymbolTable symbolTable)
    {
        HashSet<string> targets = new(StringComparer.OrdinalIgnoreCase);

        foreach (var section in symbolTable.Sections.Values)
        {
            foreach (var mixinRef in section.MixinRefs)
            {
                targets.Add(mixinRef.ReferencedSection);
            }
        }

        return targets;
    }

    private static HashSet<string> DetermineEntrySections(
        IniSymbolTable symbolTable,
        IniTreeShakerOptions options,
        HashSet<string> mixinTargets)
    {
        HashSet<string> entries = new(StringComparer.OrdinalIgnoreCase);

        // 添加用户显式指定的入口（被键/规则指定的节）
        foreach (string entry in options.EntrySections)
        {
            entries.Add(entry);
        }

        // 将未被其他节引用的节视为入口；但被 Mixin 引用的节内容已内联，不作为独立入口
        if (options.TreatStandaloneSectionsAsEntries)
        {
            foreach (var section in symbolTable.Sections.Values)
            {
                if (!mixinTargets.Contains(section.Name))
                {
                    entries.Add(section.Name);
                }
            }
        }

        return entries;
    }
}
