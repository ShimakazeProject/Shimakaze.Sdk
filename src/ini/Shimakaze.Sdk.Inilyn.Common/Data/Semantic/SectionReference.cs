using Shimakaze.Sdk.Inilyn.Data.Syntax;

namespace Shimakaze.Sdk.Inilyn.Data.Semantic;

/// <summary>
/// 引用关系：某个键值对的值引用了另一个节，或系统固定引用全局节。
/// </summary>
public sealed record class SectionReference
{
    /// <summary>
    /// 主键。
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 来源键值对 ID。全局节的系统引用为 <see langword="null"/>。
    /// </summary>
    public Guid? SourceKeyValueId { get; set; }

    /// <summary>
    /// 目标节 ID（<see langword="null"/> 表示引用未解析）。
    /// </summary>
    public Guid? TargetSectionId { get; set; }

    /// <summary>
    /// 引用类型。
    /// </summary>
    public ReferenceKind ReferenceKind { get; set; }

    /// <summary>
    /// 导航属性：来源键值对。全局节的系统引用为 <see langword="null"/>。
    /// </summary>
    public KeyValuePairNode? SourceKeyValue { get; set; }

    /// <summary>
    /// 导航属性：目标节（可能为 <see langword="null"/>）。
    /// </summary>
    public SectionNode? TargetSection { get; set; }
}
