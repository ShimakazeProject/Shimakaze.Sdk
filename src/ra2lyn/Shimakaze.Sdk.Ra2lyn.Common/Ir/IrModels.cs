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
}

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
