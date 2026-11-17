using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Ir;
using Shimakaze.Sdk.Ra2lyn.Common.Source;
using Shimakaze.Sdk.Ra2lyn.Ini.Syntax;

namespace Shimakaze.Sdk.Ra2lyn.Ini.Lowering;

/// <summary>
/// 语法树到 IR 的降级器：把节降级为实体、键值对降级为属性、继承列表降级为基名。
/// </summary>
/// <remarks>
/// <para>
/// 降级阶段唯一负责语义层面的问题：重复节（INI101）、顶层键值对（INI102）、重复键（INI103）。
/// 比较一律使用 <see cref="StringComparer.OrdinalIgnoreCase"/>，名称大小写保持原样。
/// </para>
/// <para>
/// 明确不做：Mixin 展开、跨文件合并、变量替换。
/// </para>
/// </remarks>
public sealed class AstToIrLowerer
{
    private readonly IrBuilder _builder = new();
    private readonly string _group;

    /// <summary>
    /// 初始化降级器。
    /// </summary>
    /// <param name="group">降级得到的实体所属的规则组，由调用方给出，不从扩展名推断。</param>
    public AstToIrLowerer(string group)
        => _group = group;

    /// <summary>
    /// 降级单个源文件的语法树。
    /// </summary>
    /// <param name="tree">语法分析结果；其中的语法诊断会原样合并进返回结果。</param>
    /// <param name="source">对应的源文件。</param>
    /// <returns>前端结果，包含按源文件顺序排列的实体与全部诊断。</returns>
    public FrontendResult Lower(IniSyntaxTree tree, SourceFile source)
        => Lower(tree, [source]);

    /// <summary>
    /// 降级语法树，并把若干附加源文件一并记入结果。
    /// </summary>
    /// <param name="tree">语法分析结果；其中的语法诊断会原样合并进返回结果。</param>
    /// <param name="sources">前端读取的源文件列表，顺序与返回结果一致。</param>
    /// <returns>前端结果，包含按源文件顺序排列的实体与全部诊断。</returns>
    /// <exception cref="ArgumentException"><paramref name="sources"/> 为空。</exception>
    public FrontendResult Lower(IniSyntaxTree tree, IReadOnlyList<SourceFile> sources)
    {
        ArgumentOutOfRangeException.ThrowIfZero(sources.Count, nameof(sources));

        foreach (var diagnostic in tree.Diagnostics)
            _builder.Report(diagnostic);

        HashSet<string> sectionNames = new(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in tree.Root.Entries)
        {
            switch (entry)
            {
                case SectionNode section:
                    LowerSection(section, sectionNames);
                    break;
                case KeyValueNode keyValue:
                    _builder.Report(KeyOutsideSection(keyValue));
                    break;
            }
        }

        return _builder.Build(_group, sources);
    }

    private void LowerSection(SectionNode section, HashSet<string> sectionNames)
    {
        if (sectionNames.Add(section.Name) is false)
            _builder.Report(DuplicateSection(section));

        List<IrBuilder.PropertyCommand> properties = [];
        HashSet<string> keys = new(StringComparer.OrdinalIgnoreCase);
        foreach (var keyValue in section.KeyValues)
        {
            if (keys.Add(keyValue.Key) is false)
                _builder.Report(DuplicateKey(section, keyValue));

            properties.Add(new()
            {
                Key = keyValue.Key,
                Value = keyValue.Value ?? string.Empty,
                Origin = new(keyValue.Span.Path, keyValue.KeySpan.Start, Math.Max(keyValue.KeySpan.End, keyValue.ValueSpan.End)),
            });
        }

        _builder.AddEntity(
            section.Name,
            properties,
            MakeOrigin(section),
            MakeBaseNames(section));
    }

    /// <summary>
    /// 实体来源：节头 <c>[</c> 到 <c>]</c>（含继承）的范围；缺少方括号时退化为节名范围。
    /// </summary>
    private static SourceSpan MakeOrigin(SectionNode section)
    {
        SourceSpan header = section.HeaderSpan;
        if (header.IsEmpty is false)
            return header;

        if (section.HasOpenBracket)
            return new(section.NameSpan.Path, section.NameSpan.Start - 1, section.NameSpan.End + 1);

        return section.NameSpan;
    }

    /// <summary>
    /// 继承名称：去重、保序、保留原始大小写；跳过残缺继承项中无法使用的名称。
    /// </summary>
    private static List<string> MakeBaseNames(SectionNode section)
    {
        List<string> baseNames = [];
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
        foreach (var inheritance in section.Inheritances)
        {
            if (inheritance.Name.Length is 0)
                continue;

            if (seen.Add(inheritance.Name))
                baseNames.Add(inheritance.Name);
        }

        return baseNames;
    }

    private static Diagnostic DuplicateSection(SectionNode section)
        => new(
            DiagnosticCodes.DuplicateSection,
            DiagnosticSeverity.Warning,
            $"重复的节名称: '{section.Name}'",
            section.HeaderSpan.IsEmpty ? section.NameSpan : section.HeaderSpan);

    private static Diagnostic KeyOutsideSection(KeyValueNode keyValue)
        => new(
            DiagnosticCodes.KeyOutsideSection,
            DiagnosticSeverity.Error,
            $"键值对 '{keyValue.Key}' 不属于任何节",
            keyValue.KeySpan);

    private static Diagnostic DuplicateKey(SectionNode section, KeyValueNode keyValue)
        => new(
            DiagnosticCodes.DuplicateKey,
            DiagnosticSeverity.Warning,
            $"节 '{section.Name}' 中存在重复的键 '{keyValue.Key}'",
            keyValue.KeySpan);
}
