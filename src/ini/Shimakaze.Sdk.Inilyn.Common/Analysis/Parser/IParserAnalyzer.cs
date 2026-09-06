using Shimakaze.Sdk.Inilyn.Data.Lexer;
using Shimakaze.Sdk.Inilyn.Data.Syntax;

namespace Shimakaze.Sdk.Inilyn.Analysis.Parser;

/// <summary>
/// 语法分析器接口。将记号序列解析为语法节点。
/// </summary>
public interface IParserAnalyzer : IAnalyzer
{
    /// <summary>
    /// 将记号序列解析为语法节点。
    /// </summary>
    /// <param name="context">分析器上下文。</param>
    /// <param name="tokens">记号列表。</param>
    /// <returns>语法节点列表。</returns>
    IReadOnlyList<SyntaxNode> Parse(AnalyzerContext context, IReadOnlyList<IniToken> tokens);
}
