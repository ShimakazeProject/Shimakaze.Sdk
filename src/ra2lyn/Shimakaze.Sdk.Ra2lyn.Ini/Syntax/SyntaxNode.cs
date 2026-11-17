using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Ini.Syntax;

/// <summary>
/// 语法树节点基类。
/// </summary>
public abstract class SyntaxNode
{
    /// <summary>
    /// 节点在源文件中覆盖的范围，由具体子类给出。
    /// </summary>
    public SourceSpan Span { get; init; }
}
