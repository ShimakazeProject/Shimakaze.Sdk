using Shimakaze.Sdk.Inilyn.Syntax;

namespace Shimakaze.Sdk.Inilyn.Symbols;

/// <summary>
/// INI 符号的抽象基类。
/// </summary>
public abstract class IniSymbol()
{
    /// <summary>
    /// 符号名称。
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// 声明该符号的语法节点。
    /// </summary>
    public abstract IniSyntaxNode DeclaredAt { get; }
}
