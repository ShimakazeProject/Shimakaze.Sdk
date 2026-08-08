namespace Shimakaze.Sdk.Inilyn.Lexer;

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
    /// 注释标记 <c>;</c>。
    /// </summary>
    Comment,

    /// <summary>
    /// 文档注释标记 <c>;;;</c>（单个记号，而非三个 <c>;</c>）。
    /// </summary>
    DocComment,

    /// <summary>
    /// 空白字符（空格或制表符）组成的连续串。
    /// </summary>
    Whitespace,

    /// <summary>
    /// 普通字符串：连续的非特殊、非空白、非换行字符片段。
    /// </summary>
    Text,

    /// <summary>
    /// 预处理指令：以 <c>#</c> 开头的整行（<c>#if</c>、<c>#endif</c>、<c>#region</c> 等）。
    /// </summary>
    PreprocessorDirective,

    /// <summary>
    /// 换行符（<c>\n</c>、<c>\r\n</c> 或 <c>\r</c>）。
    /// </summary>
    Newline,

    /// <summary>
    /// 文件结束。
    /// </summary>
    EndOfFile,
}
