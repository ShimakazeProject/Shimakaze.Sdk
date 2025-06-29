using LSPRange = Draco.Lsp.Model.Range;

namespace Shimakaze.Sdk.Inilyn.Models.Syntax.Green;

/// <summary>
/// 文档注释块节点
/// </summary>
internal sealed class DocumentCommentBlockNode(IEnumerable<DocumentCommentNode> comments) : GreenNode
{
    public override SyntaxKind Kind => SyntaxKind.DocumentCommentBlock;

    public override LSPRange Range => CombineRange(comments.Select(c => c.Range));

    public IReadOnlyList<DocumentCommentNode> Comments { get; } = [.. comments];

    public override IEnumerable<GreenNode> GetChildren() => Comments;
}
