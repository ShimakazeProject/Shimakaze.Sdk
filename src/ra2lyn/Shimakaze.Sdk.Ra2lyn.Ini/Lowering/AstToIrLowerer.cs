using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Ir;
using Shimakaze.Sdk.Ra2lyn.Common.Source;
using Shimakaze.Sdk.Ra2lyn.Ini.Directives;
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
    private readonly IReadOnlyList<PragmaDirective> _pragmas;

    /// <summary>
    /// 初始化降级器。
    /// </summary>
    /// <param name="group">降级得到的实体所属的规则组，由调用方给出，不从扩展名推断。</param>
    /// <param name="pragmas">预处理器收集到的 <c>#pragma priority</c> 列表，用于按节位置计算生效优先级。</param>
    public AstToIrLowerer(string group, IReadOnlyList<PragmaDirective>? pragmas = null)
    {
        _group = group;
        _pragmas = pragmas ?? [];
    }

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
                case TemplateNode template:
                    LowerTemplate(template, sectionNames);
                    break;
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

        _builder.AddEntity(
            section.Name,
            LowerKeyValues(section.Name, section.KeyValues),
            MakeOrigin(section),
            MakeBaseNames(section),
            priority: EffectivePriorityFor(section));
    }

    /// <summary>
    /// 降级模板节（TODO.md §4.2）：实体名保留源文本里的 <c>$</c> 前缀，
    /// <c>where</c> 表达式按原始字符串存进 <see cref="EntityModel.TemplateWhere"/>，
    /// body 的键值对降级为普通属性（待应用的改动包）。
    /// </summary>
    /// <remarks>
    /// 求值与属性注入由后续的 <c>TemplateExpansionPass</c> 完成，本阶段只做结构降级。
    /// 模板 body 的每个键值对都打上来源模板名（<c>$Name</c>，TODO.md §4.5），
    /// 这样属性被应用到目标节后仍能追溯"这个值是哪条模板改的"。
    /// </remarks>
    private void LowerTemplate(TemplateNode template, HashSet<string> sectionNames)
    {
        // 模板名的去重与普通节共用同一个名字集合：模板节也是 entity，同名模板合并后只应用一次。
        string name = string.Concat("$", template.Name);
        if (sectionNames.Add(name) is false)
            _builder.Report(DuplicateTemplate(template, name));

        _builder.AddEntity(
            name,
            LowerKeyValues(name, template.KeyValues, templateName: name),
            MakeOrigin(template),
            baseNames: null,
            priority: EffectivePriorityFor(template),
            isTemplate: true,
            templateWhere: template.WhereExpression,
            // 表达式在源文件里的真实位置：TemplateExpansionPass 靠它把 where 求值诊断的
            // 表达式内偏移换算回源位置（表达式已裁剪首尾空白，偏移即相对这段文本）。
            templateWhereSpan: template.WhereSpan);
    }

    /// <summary>
    /// 把节体 / 模板体降级为属性命令：重复键报 INI103，属性范围取键起点到值终点。
    /// </summary>
    /// <param name="ownerName">节 / 模板名，仅用于重复键诊断。</param>
    /// <param name="keyValues">键值对列表。</param>
    /// <param name="templateName">
    /// 模板 body 的键值对附带的来源模板名（TODO.md §4.5）；普通节体为 <see langword="null"/>。
    /// </param>
    private List<IrBuilder.PropertyCommand> LowerKeyValues(
        string ownerName,
        IReadOnlyList<KeyValueNode> keyValues,
        string? templateName = null)
    {
        List<IrBuilder.PropertyCommand> properties = [];
        HashSet<string> keys = new(StringComparer.OrdinalIgnoreCase);
        foreach (var keyValue in keyValues)
        {
            if (keys.Add(keyValue.Key) is false)
                _builder.Report(DuplicateKey(ownerName, keyValue));

            properties.Add(new()
            {
                Key = keyValue.Key,
                Value = keyValue.Value ?? string.Empty,
                Origin = new(keyValue.Span.Path, keyValue.KeySpan.Start, Math.Max(keyValue.KeySpan.End, keyValue.ValueSpan.End)),
                TemplateName = templateName,
            });
        }

        return properties;
    }

    /// <summary>
    /// 根据节头起始位置，取最后一个行起始偏移 ≤ 节头起始偏移的 pragma 的 priority（TODO.md §1.1.1）。
    /// </summary>
    private int EffectivePriorityFor(SectionNode section)
    {
        if (_pragmas.Count is 0)
            return 0;

        int sectionStart = (section.HeaderSpan.IsEmpty ? section.NameSpan : section.HeaderSpan).Start;
        int priority = 0;
        foreach (var pragma in _pragmas)
        {
            if (pragma.LineIndex > sectionStart)
                break;

            priority = pragma.Priority;
        }

        return priority;
    }

    /// <summary>
    /// 根据节头起始位置，取最后一个行起始偏移 ≤ 节头起始偏移的 pragma 的 priority（TODO.md §1.1.1）。
    /// <para/>
    /// 模板节同样享受 <c>#pragma priority</c> 机制（TODO.md §4.2），因此与普通节共用同一套规则。
    /// </summary>
    private int EffectivePriorityFor(TemplateNode template)
    {
        if (_pragmas.Count is 0)
            return 0;

        int sectionStart = (template.HeaderSpan.IsEmpty ? template.NameSpan : template.HeaderSpan).Start;
        int priority = 0;
        foreach (var pragma in _pragmas)
        {
            if (pragma.LineIndex > sectionStart)
                break;

            priority = pragma.Priority;
        }

        return priority;
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

    /// <summary>
    /// 模板来源：节头范围与普通节一致；模板节的节头覆盖到 <c>where</c> 表达式结尾。
    /// </summary>
    private static SourceSpan MakeOrigin(TemplateNode template)
    {
        SourceSpan header = template.HeaderSpan;
        if (header.IsEmpty is false)
            return header;

        if (template.NameSpan.Path is not null)
            return new(template.NameSpan.Path, template.NameSpan.Start - 1, template.NameSpan.End + 1);

        return template.NameSpan;
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

    private static Diagnostic DuplicateKey(string sectionName, KeyValueNode keyValue)
        => new(
            DiagnosticCodes.DuplicateKey,
            DiagnosticSeverity.Warning,
            $"节 '{sectionName}' 中存在重复的键 '{keyValue.Key}'",
            keyValue.KeySpan);

    private static Diagnostic DuplicateTemplate(TemplateNode template, string name)
        => new(
            DiagnosticCodes.DuplicateSection,
            DiagnosticSeverity.Warning,
            $"重复的节名称: '{name}'",
            template.HeaderSpan.IsEmpty ? template.NameSpan : template.HeaderSpan);
}
