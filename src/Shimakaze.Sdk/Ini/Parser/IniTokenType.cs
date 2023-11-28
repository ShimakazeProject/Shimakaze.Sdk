namespace Shimakaze.Sdk.Ini.Parser;

/// <summary>
/// INI Token Type
/// </summary>
public static class IniTokenType
{
    /// <summary>
    /// Unknown
    /// </summary>
    public const int Unknown = 0;
    /// <summary>
    /// End of File
    /// </summary>
    public const int EOF = -1;
    /// <summary>
    /// CR
    /// </summary>
    public const int CR = '\r';
    /// <summary>
    /// LF
    /// </summary>
    public const int LF = '\n';
    /// <summary>
    /// Space
    /// </summary>
    public const int SPACE = ' ';
    /// <summary>
    /// Tab
    /// </summary>
    public const int TAB = '\t';
    /// <summary>
    /// ;
    /// </summary>
    public const int SEMI = ';';
    /// <summary>
    /// =
    /// </summary>
    public const int EQ = '=';
    /// <summary>
    /// [
    /// </summary>
    public const int START_BRACKET = '[';
    /// <summary>
    /// ]
    /// </summary>
    public const int END_BRACKET = ']';
    // =====================================
    /// <summary>
    /// ; Comment
    /// </summary>
    public const int Comment = 1;
    /// <summary>
    /// [Section]
    /// </summary>
    public const int Section = 2;
    /// <summary>
    /// Key=
    /// </summary>
    public const int Key = 3;
    /// <summary>
    /// =Value
    /// </summary>
    public const int Value = 4;
}