using Shimakaze.Sdk.Inilyn.Syntax;
using Shimakaze.Sdk.Inilyn.Syntax.Nodes;

namespace Shimakaze.Sdk.Inilyn.Symbols;

/// <summary>
/// 键符号。
/// </summary>
/// <param name="name">键名。</param>
/// <param name="declaredAt">声明该符号的语法节点。</param>
public sealed class IniKeySymbol(
    string name,
    IniKeyValueEntry declaredAt
) : IniSymbol
{
    /// <inheritdoc />
    public override string Name => name;

    /// <inheritdoc />
    public override IniSyntaxNode DeclaredAt => declaredAt;
}
