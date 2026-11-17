namespace Shimakaze.Sdk.Ra2lyn.Ini.Syntax;

/// <summary>
/// 无关记号的种类。
/// </summary>
public enum TriviaKind
{
    /// <summary>
    /// 井号注释，文本从 <c>#</c> 开始直到行尾。
    /// </summary>
    HashComment,

    /// <summary>
    /// 分号注释，文本从 <c>;</c> 开始直到行尾。
    /// </summary>
    SemicolonComment,

    /// <summary>
    /// 连续空白字符（空格、制表符）。
    /// </summary>
    Whitespace,

    /// <summary>
    /// 换行符。
    /// </summary>
    Newline,
}
