namespace Shimakaze.Sdk.Ra2lyn.Ini.Syntax;

/// <summary>
/// 继承列表中的前置分隔符种类。
/// </summary>
public enum SectionSeparator
{
    /// <summary>
    /// 缺少分隔符。
    /// </summary>
    None,

    /// <summary>
    /// 冒号 <c>:</c>，只应出现在继承列表的第一项。
    /// </summary>
    Colon,

    /// <summary>
    /// 逗号 <c>,</c>，只应出现在继承列表的第二项及之后。
    /// </summary>
    Comma,
}
