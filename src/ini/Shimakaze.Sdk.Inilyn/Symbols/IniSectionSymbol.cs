using Shimakaze.Sdk.Inilyn.Syntax;
using Shimakaze.Sdk.Inilyn.Syntax.Nodes;

namespace Shimakaze.Sdk.Inilyn.Symbols;

/// <summary>
/// 节符号。
/// </summary>
/// <param name="name">节名。</param>
/// <param name="declaredAt">声明该符号的语法节点。</param>
public sealed class IniSectionSymbol(
    string name,
    IniSectionDecl declaredAt
) : IniSymbol
{
    /// <inheritdoc />
    public override string Name => name;

    /// <inheritdoc />
    public override IniSyntaxNode DeclaredAt => declaredAt;

    /// <summary>
    /// 该节内声明的键符号。
    /// </summary>
    public Dictionary<string, IniKeySymbol> Keys { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 该节的 Mixin 引用列表。
    /// </summary>
    public List<IniMixinSymbol> MixinRefs { get; } = [];
}
