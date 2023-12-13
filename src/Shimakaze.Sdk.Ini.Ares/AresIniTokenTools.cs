namespace Shimakaze.Sdk.Ini.Ares;

/// <summary>
/// Token生成工具
/// </summary>
public static class AresIniTokenTools
{
    /// <summary>
    /// 空白字符
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public static IIniToken White(int token) => new IniToken(token, default);
    /// <summary>
    /// 符号
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public static IIniToken Sign(int token) => new IniToken(token, default);
    /// <summary>
    /// 字符串
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static IIniToken Value(string value) => new IniToken(1, value);

    // ========================== 空白 ==========================

    /// <summary>
    /// EOF
    /// </summary>
    public static IIniToken EOF => White(-1);
    /// <summary>
    /// CR
    /// </summary>
    public static IIniToken CR => White('\r');
    /// <summary>
    /// LF
    /// </summary>
    public static IIniToken LF => White('\n');
    /// <summary>
    /// SPACE
    /// </summary>
    public static IIniToken SPACE => White(' ');
    /// <summary>
    /// TAB
    /// </summary>
    public static IIniToken TAB => White('\t');

    // ========================== 符号 ==========================

    /// <summary>
    /// SEMI
    /// </summary>
    public static IIniToken SEMI => Sign(';');
    /// <summary>
    /// EQ
    /// </summary>
    public static IIniToken EQ => Sign('=');
    /// <summary>
    /// COLON
    /// </summary>
    public static IIniToken COLON => Sign(':');
    /// <summary>
    /// PLUS
    /// </summary>
    public static IIniToken PLUS => Sign('+');
    /// <summary>
    /// BeginBracket
    /// </summary>
    public static IIniToken BeginBracket => Sign('[');
    /// <summary>
    /// EndBracket
    /// </summary>
    public static IIniToken EndBracket => Sign(']');
}

file sealed record class IniToken(int Token, string? Value) : IIniToken
{
    public string Type => $"{(char)Token}"
        .Replace("\r", "CR")
        .Replace("\n", "LF")
        .Replace("\x1", "Value")
        .Replace("\xFFFF", "EOF")
        ;
}
