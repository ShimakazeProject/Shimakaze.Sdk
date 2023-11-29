namespace Shimakaze.Sdk.Ini.Parser;

/// <summary>
/// Token忽略等级
/// </summary>
public enum IniTokenIgnoreLevel
{
    /// <summary>
    /// 不忽略
    /// </summary>
    None,
    /// <summary>
    /// 忽略空白
    /// </summary>
    White,
    /// <summary>
    /// 忽略不包含值的Token
    /// </summary>
    NonValue,
}