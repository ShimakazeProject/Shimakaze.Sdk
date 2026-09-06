using Shimakaze.Sdk.Inilyn.Data.Lexer;

namespace Shimakaze.Sdk.Inilyn.Analysis.Lexer;

/// <summary>
/// 词法分析器接口。将源文本拆分为记号序列。
/// </summary>
public interface ILexerAnalyzer : IAnalyzer
{
    /// <summary>
    /// 对源文本进行词法分析，返回记号列表。
    /// </summary>
    /// <param name="context">分析器上下文。</param>
    /// <returns>记号列表。</returns>
    IReadOnlyList<IniToken> Lex(AnalyzerContext context);
}
