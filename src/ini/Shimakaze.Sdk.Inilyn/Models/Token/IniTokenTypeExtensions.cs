using Shimakaze.Sdk.Inilyn.Text;

namespace Shimakaze.Sdk.Inilyn.Models.Token;

/// <summary>
/// <see cref="IniTokenType"/>
/// </summary>
public static class IniTokenTypeExtensions
{
    /// <summary>
    /// 从类型创建 Token
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    public static IniToken Create(this IniTokenType type, SourceText value)
        => new(value.GetRange(..), type, value);
}
