using LSPRange = Draco.Lsp.Model.Range;

namespace Shimakaze.Sdk.Inilyn.Models.Syntax.Green;

/// <summary>
/// 表示一个完整的节节点，例如：
/// 
/// ;;; 文档注释
/// [section] : [base] ; 行尾注释
/// key = value
/// 
/// </summary>
/// <param name="documentComment">节的文档注释（可空）</param>
/// <param name="sectionHeader">节头信息（可空）</param>
/// <param name="sectionData">节的数据块（不可空）</param>
internal sealed class SectionNode(
    SectionDataNode sectionData,
    SectionHeaderNode? sectionHeader = null,
    DocumentCommentBlockNode? documentComment = null) : GreenNode
{
    public override SyntaxKind Kind => SyntaxKind.Section;

    public override LSPRange Range => CombineRange(
        documentComment?.Range,
        sectionHeader?.Range,
        sectionData?.Range);

    /// <summary>
    /// 获取节的文档注释（如果存在）
    /// </summary>
    public DocumentCommentBlockNode? DocumentComment => documentComment;

    /// <summary>
    /// 获取节头信息（如果存在）
    /// </summary>
    public SectionHeaderNode? SectionHeader => sectionHeader;

    /// <summary>
    /// 获取节的数据块
    /// </summary>
    public SectionDataNode SectionData => sectionData;

    public override IEnumerable<GreenNode> GetChildren()
    {
        if (documentComment is not null)
            yield return documentComment;
        if (sectionHeader is not null)
            yield return sectionHeader;
        yield return sectionData;
    }
}
