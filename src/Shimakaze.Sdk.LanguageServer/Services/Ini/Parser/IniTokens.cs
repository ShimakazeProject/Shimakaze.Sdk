namespace Shimakaze.Sdk.LanguageServer.Services.Ini.Parser;

internal static class IniTokens
{
    public const int Unknown = -1;
    /// <summary>
    /// ; comment
    /// </summary>
    public const int Comment = 1;
    /// <summary>
    /// [section]
    /// </summary>
    public const int Section = 2;
    /// <summary>
    /// key = value
    /// </summary>
    public const int Key = 3;
    /// <summary>
    /// key = value
    /// </summary>
    public const int Value = 4;
    // Ares 特殊键
    /// <summary>
    /// [section] : [base]
    /// </summary>
    public const int BaseSection = Section + 10;
    /// <summary>
    /// += value
    /// </summary>
    public const int AddKey = Key + 10;
}
