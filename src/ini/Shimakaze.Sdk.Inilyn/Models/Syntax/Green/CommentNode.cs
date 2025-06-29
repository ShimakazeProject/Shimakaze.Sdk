using Shimakaze.Sdk.Inilyn.Models.Token;

using LSPRange = Draco.Lsp.Model.Range;

namespace Shimakaze.Sdk.Inilyn.Models.Syntax.Green;

/// <summary>
/// 注释节点
/// </summary>
/// <param name="token"></param>
internal sealed class CommentNode(IniToken token) : GreenNode
{
    public override SyntaxKind Kind => SyntaxKind.Comment;
    public override LSPRange Range => token.Range;
    public IniToken Token => token;
}
