using Shimakaze.Sdk.Inilyn.Models.Token;

using LSPRange = Draco.Lsp.Model.Range;

namespace Shimakaze.Sdk.Inilyn.Models.Syntax.Green;

/// <summary>
/// 文档注释节点
/// </summary>
/// <param name="token"></param>
internal sealed class DocumentCommentNode(IniToken token) : GreenNode
{
    public override SyntaxKind Kind => SyntaxKind.DocumentComment;
    public override LSPRange Range => token.Range;
    public IniToken Token => token;
}
