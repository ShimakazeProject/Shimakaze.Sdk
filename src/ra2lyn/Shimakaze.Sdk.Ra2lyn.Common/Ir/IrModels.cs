using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Common.Ir;

/// <summary>
/// 实体（源语言中的“节”）在 IR 中的表示。
/// </summary>
public sealed class EntityModel
{
    /// <summary>
    /// 实体标识。
    /// </summary>
    public required EntityId Id { get; init; }

    /// <summary>
    /// 所属规则组，来自文件分类或调用方传入。
    /// </summary>
    public required string Group { get; init; }

    /// <summary>
    /// 实体名称，比较使用 <see cref="StringComparer.OrdinalIgnoreCase"/>。
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 分类结果，由分类 Pass 写入。
    /// </summary>
    public EntityKind Kind { get; set; }

    /// <summary>
    /// 推断出的类型名，可为空。
    /// </summary>
    public string? TypeName { get; set; }

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
    /// 是否从入口可达，由可达性 Pass 写入。
    /// </summary>
    public bool IsReachable { get; set; }
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
    /// 列表分隔符；为 <see langword="null"/> 表示不是列表。
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
/// <param name="From">来源属性；为 <see langword="null"/> 表示系统级引用。</param>
/// <param name="To">目标实体，始终是真实实体。</param>
/// <param name="Kind">引用种类。</param>
public sealed record class ReferenceModel(
    PropertyId? From,
    EntityId To,
    ReferenceKind Kind);
