using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Ini.Syntax;

/// <summary>
/// 节头中的一个继承项，例如 <c>[A]:[B],[C]</c> 中的 <c>[B]</c> 与 <c>[C]</c>。
/// </summary>
public sealed class SectionInheritance
{
    /// <summary>
    /// 继承项名称，保留原始大小写，首尾空白已裁剪。
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 是否存在左方括号 <c>[</c>。
    /// </summary>
    public bool HasOpenBracket { get; init; }

    /// <summary>
    /// 是否存在右方括号 <c>]</c>。
    /// </summary>
    public bool HasCloseBracket { get; init; }

    /// <summary>
    /// 相对上一项的前置分隔符：第一项为 <see cref="SectionSeparator.Colon"/>，
    /// 第二项及以后为 <see cref="SectionSeparator.Comma"/>，缺失时为 <see cref="SectionSeparator.None"/>。
    /// </summary>
    public SectionSeparator Separator { get; init; }

    /// <summary>
    /// 继承项在源文件中的范围：从前置分隔符（若存在）或左方括号开始，到右方括号为止。
    /// </summary>
    public SourceSpan Span { get; init; }
}
