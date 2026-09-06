namespace Shimakaze.Sdk.Inilyn.Data.Lexer;

/// <summary>
/// INI 词法记号类型。
/// </summary>
public enum IniTokenType
{
    /// <summary>
    /// 左方括号 <c>[</c>。
    /// </summary>
    LeftBracket,

    /// <summary>
    /// 右方括号 <c>]</c>。
    /// </summary>
    RightBracket,

    /// <summary>
    /// 等号 <c>=</c>。
    /// </summary>
    EqualSign,

    /// <summary>
    /// 冒号 <c>:</c>。
    /// </summary>
    Colon,

    /// <summary>
    /// 逗号 <c>,</c>。
    /// </summary>
    Comma,

    /// <summary>
    /// 分号 <c>;</c>。
    /// </summary>
    Semicolon,

    /// <summary>
    /// 井号 <c>#</c>。
    /// </summary>
    Hash,

    /// <summary>
    /// 非特殊、非空白、非换行的连续字符片段。
    /// </summary>
    Text,

    /// <summary>
    /// 连续空白字符（空格、制表符）。
    /// </summary>
    Whitespace,

    /// <summary>
    /// 换行符（<c>\n</c>、<c>\r\n</c> 或 <c>\r</c>）。
    /// </summary>
    Newline,

    /// <summary>
    /// 文件结束。
    /// </summary>
    EndOfFile,
}