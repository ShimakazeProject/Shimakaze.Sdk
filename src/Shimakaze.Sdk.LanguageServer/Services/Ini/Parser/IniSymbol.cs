namespace Shimakaze.Sdk.LanguageServer.Services.Ini.Parser;

/// <summary>
/// INI符号
/// </summary>
/// <param name="Token"></param>
/// <param name="Line">行号</param>
/// <param name="StartCharacter">起始位置</param>
/// <param name="Length">长度</param>
/// <param name="Word">内容</param>
public record class IniSymbol(
    int Token,
    int Line,
    int StartCharacter,
    int Length,
    string Word
)
{
    /// <summary>
    /// INI符号
    /// </summary>
    /// <param name="token"></param>
    /// <param name="line">行号</param>
    /// <param name="start">起始位置</param>
    /// <param name="length">长度</param>
    /// <param name="text">内容</param>
    public IniSymbol(in int token, in int line, in int start, in int length, in ReadOnlySpan<char> text)
        :this(token, line, start, length, new(text))
    {
    }

    /// <summary>
    /// 结束位置
    /// </summary>
    public int EndCharacter => StartCharacter + Length;
    /// <summary>
    /// 未能识别的符号
    /// </summary>
    /// <param name="line">行号</param>
    /// <param name="start">起始位置</param>
    /// <param name="length">长度</param>
    /// <param name="text">内容</param>
    /// <returns></returns>
    public static IniSymbol Unknown(in int line, in int start, in int length, in ReadOnlySpan<char> text)
        => new(IniTokens.Unknown, line, start, length, new(text));
}
