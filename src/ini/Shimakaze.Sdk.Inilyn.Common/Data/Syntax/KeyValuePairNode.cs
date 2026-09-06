namespace Shimakaze.Sdk.Inilyn.Data.Syntax;

/// <summary>
/// 键值对节点，例如 <c>Key = Value</c>。
/// </summary>
public sealed class KeyValuePairNode : SyntaxNode
{
    /// <summary>
    /// 所属段落节点标识。顶层键值对为 <see langword="null"/>。
    /// </summary>
    public Guid? SectionId { get; set; }

    /// <summary>
    /// 所属段落节点。
    /// </summary>
    public SectionNode? Section { get; set; }

    /// <summary>
    /// 键名。缺少键时为 <see langword="null"/>。
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// 是否存在等号 <c>=</c>。
    /// </summary>
    public bool HasEquals { get; set; }

    /// <summary>
    /// 值内容。缺少值时为 <see langword="null"/>。
    /// </summary>
    public string? Value { get; set; }
}
