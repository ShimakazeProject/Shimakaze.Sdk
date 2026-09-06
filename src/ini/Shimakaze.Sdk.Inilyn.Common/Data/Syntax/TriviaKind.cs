namespace Shimakaze.Sdk.Inilyn.Data.Syntax;

/// <summary>
/// 无关记号类型。
/// </summary>
public enum TriviaKind
{
    /// <summary>
    /// 井号注释 <c>#</c>。
    /// </summary>
    HashComment,

    /// <summary>
    /// 分号注释 <c>;</c>。
    /// </summary>
    SemicolonComment,

    /// <summary>
    /// 空白字符（空格、制表符）。
    /// </summary>
    Whitespace,

    /// <summary>
    /// 换行符。
    /// </summary>
    Newline,
}
