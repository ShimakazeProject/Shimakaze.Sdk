namespace Shimakaze.Sdk.Inilyn.Lexer;

/// <summary>
/// INI 词法记号。
/// </summary>
/// <param name="Type">记号类型。</param>
/// <param name="Line">记号起始行号（从 1 开始）。</param>
/// <param name="Column">记号起始列号（从 1 开始）。</param>
/// <param name="Offset">记号起始的绝对字符偏移量（从 0 开始）。</param>
/// <param name="Text">记号的词素文本（EOF 为空字符串）。</param>
public sealed record class IniToken(IniTokenType Type, int Line, int Column, int Offset, string Text);
