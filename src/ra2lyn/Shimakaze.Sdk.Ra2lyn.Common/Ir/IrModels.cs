using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Common.Ir;

/// <summary>
/// 实体（源语言中的“节”）在 IR 中的表示。
/// </summary>
/// <remarks>
/// 这里只有“节”的结构信息：标识、所属组、名称、属性与继承名。
/// 分类、类型、可达性等具体语义由分析阶段以注解的形式给出，不写进内核模型。
/// </remarks>
public sealed class EntityModel
{
    /// <summary>
    /// 实体标识。
    /// </summary>
    public required EntityId Id { get; init; }

    /// <summary>
    /// 所属分组，来自调用方传入或文件分类。
    /// </summary>
    public required string Group { get; init; }

    /// <summary>
    /// 实体名称，比较使用 <see cref="StringComparer.OrdinalIgnoreCase"/>。
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 语法上的继承/混合名称，仅记录不展开。
    /// </summary>
    public required IReadOnlyList<string> BaseNames { get; init; }

    /// <summary>
    /// 属性列表，顺序与源文件一致。
    /// </summary>
    public required IReadOnlyList<PropertyModel> Properties { get; init; }

    /// <summary>
    /// 实体在源文件中的位置。
    /// </summary>
    public required SourceSpan Origin { get; init; }

    /// <summary>
    /// 覆盖式合并时的排序优先级，源自最近的 <c>#pragma priority &lt;n&gt;</c> 指令（TODO.md §1.1.1）。
    /// 数值越大的实体在合并队列中越靠后、覆盖先到者；默认 0。
    /// </summary>
    public int Priority { get; init; }

    /// <summary>
    /// 该实体是否为模板节（<c>[$Name]</c> 或 <c>[$Name] : where Expr</c>，TODO.md §4.2）。
    /// </summary>
    /// <remarks>
    /// 为 <see langword="true"/> 时，<see cref="Name"/> 保留源文本里的 <c>$</c> 前缀，
    /// <see cref="Properties"/> 是"待应用的改动包"，<see cref="TemplateWhere"/> 保存待求值的
    /// <c>where</c> 表达式。<c>TemplateExpansionPass</c> 应用完模板后会把这些实体整体剔除，
    /// 它们不进入 Emit，也不参与后续分析器（TODO.md §4.5）。
    /// </remarks>
    public bool IsTemplate { get; init; }

    /// <summary>
    /// 模板节的 <c>where</c> 表达式原始字符串（TODO.md §4.3）；为 <see langword="null"/> 时表示无条件模板。
    /// </summary>
    /// <remarks>
    /// 仅当 <see cref="IsTemplate"/> 为 <see langword="true"/> 时有意义。表达式按原始文本保存，
    /// 由 <c>TemplateExpansionPass</c> 交给 where 求值器解析，不在降级阶段求值。
    /// </remarks>
    public string? TemplateWhere { get; init; }

    /// <summary>
    /// <see cref="TemplateWhere"/> 在源文件中的位置（已裁剪首尾空白）；无条件模板或位置未知时为 <see langword="default"/>。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 求值器产生的诊断偏移是<b>相对 <see cref="TemplateWhere"/> 文本</b>的，因此 <c>TemplateExpansionPass</c>
    /// 需要这个起点才能把偏移正确换算回源文件位置（而不是用节头区间粗略倒推）。
    /// </para>
    /// <para>
    /// 与 <see cref="Origin"/> 的区别：<see cref="Origin"/> 覆盖整个节头（<c>[...]</c> 到表达式末尾，
    /// 且会随 MergePass 取桶内首个 entity 的位置），<see cref="TemplateWhereSpan"/> 只覆盖表达式本身。
    /// </para>
    /// </remarks>
    public SourceSpan TemplateWhereSpan { get; init; }
}

/// <summary>
/// 实体中的一个属性（源语言中的“键值对”）。
/// </summary>
public sealed class PropertyModel
{
    /// <summary>
    /// 属性标识。
    /// </summary>
    public required PropertyId Id { get; init; }

    /// <summary>
    /// 键名，比较使用 <see cref="StringComparer.OrdinalIgnoreCase"/>。
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// 原始值字符串，不做类型转换。
    /// </summary>
    public required string Value { get; init; }

    /// <summary>
    /// 值的形态：简单值或引用值；由语法决定，不需要规则即可判定。
    /// </summary>
    public PropertyValueKind ValueKind { get; init; } = PropertyValueKind.Simple;

    /// <summary>
    /// 引用值中出现多个目标时的分隔符；为 <see langword="null"/> 表示单值。
    /// </summary>
    public string? ListSeparator { get; init; }

    /// <summary>
    /// 属性在源文件中的位置。
    /// </summary>
    public required SourceSpan Origin { get; init; }

    /// <summary>
    /// 被当前值覆盖掉的历史属性链（TODO.md §1.3）。
    /// 链顺序为从旧到新：列表末尾是最近一次被覆盖的旧值。
    /// 新增 key 时为空列表。
    /// </summary>
    public IReadOnlyList<PropertyHistoryEntry> History { get; init; } = [];

    /// <summary>
    /// 该属性来自哪条模板（TODO.md §4.5）；目标节自己写的属性为 <see langword="null"/>。
    /// </summary>
    /// <remarks>
    /// 取值是模板名，含源文本里的 <c>$</c> 前缀（例如 <c>$Temp1</c>），保留原始大小写。
    /// <see cref="Passes.TemplateExpansionPass"/> 把模板 body 应用到目标节时写入；被覆盖的旧值进
    /// <see cref="History"/> 链，除此之外后续 Pass 原样透传，不重新推导。
    /// <para/>
    /// 用途：模板节本身已被剔除（不进入 Emit、不参与分析器），这个字段是"这个值是哪条模板改的"
    /// 的唯一可追溯来源。模板自己的名字在 INI 里取不到，只能通过该字段从编译期一侧读取。
    /// </remarks>
    public string? TemplateName { get; init; }
}

/// <summary>
/// <see cref="PropertyModel.History"/> 链中的一条记录，
/// 表示一个被后续合并覆盖掉的旧属性值（TODO.md §1.3）。
/// </summary>
/// <param name="Value">被覆盖时的原始值字符串。</param>
/// <param name="Origin">被覆盖属性在源文件中的位置。</param>
/// <param name="Priority">该属性所属 entity 的覆盖优先级。</param>
/// <param name="FrontendIndex">该属性所属前端在编译上下文内的序号。</param>
/// <param name="IsAppend">是否由追加（<c>$</c>）合并产生；当前步骤固定为 <see langword="false"/>。</param>
public sealed record PropertyHistoryEntry(
    string Value,
    SourceSpan Origin,
    int Priority,
    int FrontendIndex,
    bool IsAppend = false);

/// <summary>
/// 实体之间的一条引用。
/// </summary>
/// <param name="From">来源属性；为 <see langword="null"/> 表示入口引用或系统级引用。</param>
/// <param name="To">目标实体，始终是真实实体。</param>
/// <param name="Kind">引用种类；语义由分析阶段解释。</param>
public sealed record class ReferenceModel(
    PropertyId? From,
    EntityId To,
    ReferenceKind Kind);
