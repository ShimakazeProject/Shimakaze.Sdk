namespace Shimakaze.Sdk.Inilyn.Models.Token;

/// <summary>
/// INI 词法类型
/// </summary>
public enum IniTokenType
{
    /// <summary>
    /// 未知
    /// </summary>
    Unknown,
    /// <summary>
    /// 行终止符
    /// </summary>
    EOL,
    /// <summary>
    /// 左方括号
    /// </summary>
    LeftBracket = 10,
    /// <summary>
    /// 右方括号
    /// </summary>
    RightBracket,
    /// <summary>
    /// 等号
    /// </summary>
    Eq,
    /// <summary>
    /// 分号
    /// </summary>
    Semicolon,
    /// <summary>
    /// 井号
    /// </summary>
    Hash,
    /// <summary>
    /// 冒号
    /// </summary>
    Colon,
    /// <summary>
    /// 三个分号
    /// </summary>
    TripleSemicolon,

    /// <summary>
    /// 值
    /// </summary>
    Value = 20,
}
