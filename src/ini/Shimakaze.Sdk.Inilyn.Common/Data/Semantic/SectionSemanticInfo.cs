using Shimakaze.Sdk.Inilyn.Data.Syntax;

namespace Shimakaze.Sdk.Inilyn.Data.Semantic;

/// <summary>
/// 节的语义分析结果（一对一）。
/// </summary>
public sealed record class SectionSemanticInfo
{
    /// <summary>
    /// 主键。
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 所属节 ID。
    /// </summary>
    public Guid SectionId { get; set; }

    /// <summary>
    /// 所属文档 ID（冗余，便于批量查询和级联删除）。
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// 规则组名（如 <c>Rule</c>、<c>Art</c>、<c>Sound</c>）。
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// 节的分类。
    /// </summary>
    public SectionKind SectionKind { get; set; }

    /// <summary>
    /// 推断的类型名（如 <c>InfantryType</c>），可为 <see langword="null"/>。
    /// </summary>
    public string? SectionType { get; set; }

    /// <summary>
    /// 是否从入口可达（<see langword="false"/> 表示可被 TreeShaking 移除）。
    /// </summary>
    public bool IsReachable { get; set; }

    /// <summary>
    /// 导航属性：所属节。
    /// </summary>
    public SectionNode Section { get; set; } = default!;
}
