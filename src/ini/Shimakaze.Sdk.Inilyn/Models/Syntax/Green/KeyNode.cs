using Shimakaze.Sdk.Inilyn.Models.Token;

using LSPRange = Draco.Lsp.Model.Range;

namespace Shimakaze.Sdk.Inilyn.Models.Syntax.Green;

/// <summary>
/// 键节点
/// </summary>
/// <param name="token"></param>
internal sealed class KeyNode(IniToken token) : GreenNode
{
    public override SyntaxKind Kind => SyntaxKind.Key;
    public override LSPRange Range => token.Range;
    public IniToken Token => token;
}
