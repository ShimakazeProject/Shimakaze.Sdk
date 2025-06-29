using LSPRange = Draco.Lsp.Model.Range;

namespace Shimakaze.Sdk.Inilyn.Models.Syntax.Green;

/// <summary>
/// 表示节的数据部分，包含键值对、注释和编译器指令。
/// </summary>
/// <param name="items">节内的数据项集合</param>
internal sealed class SectionDataNode(IEnumerable<GreenNode> items) : GreenNode
{
    public override SyntaxKind Kind => SyntaxKind.SectionData;

    public override LSPRange Range => CombineRange(items.Select(i => i.Range));

    /// <summary>
    /// 获取所有数据项（键值对、注释、编译器指令等）
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><see cref="KeyValuePairNode"/> 键值对</item>
    /// <item><see cref="CommentNode"/> 普通注释</item>
    /// <item><see cref="DocumentCommentNode"/> 意外写的文档注释 作为普通注释处理</item>
    /// <!--<item><see cref="ErrorNode"/> 语法错误</item>-->
    /// </list>
    /// </remarks>
    public IReadOnlyList<GreenNode> Items { get; } = [.. items];

    public override IEnumerable<GreenNode> GetChildren() => Items;
}
