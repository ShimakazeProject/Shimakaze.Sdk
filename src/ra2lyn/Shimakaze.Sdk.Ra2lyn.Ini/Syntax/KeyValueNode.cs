using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Ini.Syntax;

/// <summary>
/// 键值对节点，例如 <c>Key = Value</c>。既出现在节内，也可能作为顶层条目出现。
/// </summary>
public sealed class KeyValueNode : SyntaxNode
{
    /// <summary>
    /// 键名，保留原始文本（首尾空白已裁剪），允许以 <c>:</c> 开头（如 <c>:SecretBuilding</c>）。
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// 值文本（首尾空白已裁剪）；没有值时为 <see langword="null"/>。
    /// </summary>
    public required string? Value { get; init; }

    /// <summary>
    /// 是否存在等号 <c>=</c>。
    /// </summary>
    public bool HasEquals { get; init; }

    /// <summary>
    /// 前导自由记号：本行之前的注释、空白与换行。
    /// </summary>
    public required IReadOnlyList<Trivia> LeadingTrivia { get; init; }

    /// <summary>
    /// 尾随自由记号：值之后的行尾注释与换行。
    /// </summary>
    public required IReadOnlyList<Trivia> TrailingTrivia { get; init; }

    /// <summary>
    /// 键名在源文件中的范围。
    /// </summary>
    public SourceSpan KeySpan { get; init; }

    /// <summary>
    /// 值在源文件中的范围（已裁剪首尾空白）；没有值时为零宽范围。
    /// </summary>
    public SourceSpan ValueSpan { get; init; }
}
