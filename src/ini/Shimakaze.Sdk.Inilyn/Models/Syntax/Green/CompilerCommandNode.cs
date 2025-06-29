using Shimakaze.Sdk.Inilyn.Models.Token;

using LSPRange = Draco.Lsp.Model.Range;

namespace Shimakaze.Sdk.Inilyn.Models.Syntax.Green;

/// <summary>
/// 编译器指令节点
/// </summary>
/// <param name="token"></param>
internal sealed class CompilerCommandNode(IniToken token) : GreenNode
{
    public override SyntaxKind Kind => SyntaxKind.CompilerCommand;
    public override LSPRange Range => token.Range;
    public IniToken Token => token;
}
