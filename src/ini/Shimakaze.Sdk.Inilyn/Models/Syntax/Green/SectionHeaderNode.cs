using LSPRange = Draco.Lsp.Model.Range;

namespace Shimakaze.Sdk.Inilyn.Models.Syntax.Green;

/// <summary>
/// 表示节头节点，例如：
/// [section]
/// [derived] : [base]
/// </summary>
/// <param name="sectionName">节名称</param>
/// <param name="inheritSectionName">继承的节名称（可空）</param>
/// <param name="comment">行尾注释（可空）</param>
internal sealed class SectionHeaderNode(
    SectionNameNode sectionName,
    InheritSectionNameNode? inheritSectionName = null,
    CommentNode? comment = null) : GreenNode
{
    public override SyntaxKind Kind => SyntaxKind.SectionHeader;

    public override LSPRange Range => CombineRange(
        sectionName.Range,
        inheritSectionName?.Range,
        comment?.Range);

    public SectionNameNode SectionName => sectionName;
    public InheritSectionNameNode? InheritSectionName => inheritSectionName;
    public CommentNode? Comment => comment;

    public override IEnumerable<GreenNode> GetChildren()
    {
        yield return sectionName;
        if (inheritSectionName is not null)
            yield return inheritSectionName;
        if (comment is not null)
            yield return comment;
    }
}
