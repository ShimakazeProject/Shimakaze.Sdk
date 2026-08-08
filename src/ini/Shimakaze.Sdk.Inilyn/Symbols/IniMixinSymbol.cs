using Shimakaze.Sdk.Inilyn.Syntax;
using Shimakaze.Sdk.Inilyn.Syntax.Nodes;

namespace Shimakaze.Sdk.Inilyn.Symbols;

/// <summary>
/// Mixin 引用符号。
/// </summary>
/// <param name="targetSection">目标节名（拥有 Mixin 声明的节）。</param>
/// <param name="referencedSection">被引用的节名。</param>
/// <param name="declaredAt">声明该符号的语法节点。</param>
/// <param name="startLine">声明位置的起始行号（1-based）。</param>
/// <param name="startColumn">声明位置的起始列号（1-based）。</param>
/// <param name="endLine">声明位置的结束行号（1-based）。</param>
/// <param name="endColumn">声明位置的结束列号（1-based）。</param>
public sealed class IniMixinSymbol(
    string targetSection,
    string referencedSection,
    IniMixinReference declaredAt,
    int startLine,
    int startColumn,
    int endLine,
    int endColumn
) : IniSymbol
{
    /// <summary>
    /// 目标节名（拥有 Mixin 声明的节）。
    /// </summary>
    public string TargetSection => targetSection;

    /// <summary>
    /// 被引用的节名。
    /// </summary>
    public string ReferencedSection => referencedSection;

    /// <summary>
    /// 声明位置的起始行号（1-based）。
    /// </summary>
    public int StartLine => startLine;

    /// <summary>
    /// 声明位置的起始列号（1-based）。
    /// </summary>
    public int StartColumn => startColumn;

    /// <summary>
    /// 声明位置的结束行号（1-based）。
    /// </summary>
    public int EndLine => endLine;

    /// <summary>
    /// 声明位置的结束列号（1-based）。
    /// </summary>
    public int EndColumn => endColumn;

    /// <inheritdoc />
    public override string Name => ReferencedSection;

    /// <inheritdoc />
    public override IniSyntaxNode DeclaredAt => declaredAt;
}
