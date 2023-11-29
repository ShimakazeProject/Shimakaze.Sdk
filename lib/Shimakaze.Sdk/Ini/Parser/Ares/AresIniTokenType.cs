namespace Shimakaze.Sdk.Ini.Parser.Ares;

/// <summary>
/// Ares INI Token Type
/// </summary>
public static class AresIniTokenType
{
    /// <summary>
    /// Ares INI 特有的 继承 Section
    /// </summary>
    /// <remarks>
    /// [Section] : [Base]
    /// </remarks>
    public const int BASE_SECTION = ':';
    /// <summary>
    /// PLUS
    /// </summary>
    /// <remarks>
    /// += Value
    /// </remarks>
    public const int PLUS = '+';
}