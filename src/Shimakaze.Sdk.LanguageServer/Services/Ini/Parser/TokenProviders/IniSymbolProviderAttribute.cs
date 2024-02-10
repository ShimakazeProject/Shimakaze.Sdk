namespace Shimakaze.Sdk.LanguageServer.Services.Ini.Parser.TokenProviders;

/// <summary>
/// 实现 <see cref="IIniParseProvider"/> 接口的类的属性
/// </summary>
/// <param name="token"></param>
/// <param name="weight"></param>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class IniSymbolProviderAttribute(int token, int weight = 0) : Attribute
{
    /// <summary>
    /// 权重
    /// </summary>
    public int Weight { get; init; } = weight;

    /// <summary>
    /// 提供的 <see cref="IniSymbol.Token"/> 的值
    /// </summary>
    public int Token { get; init; } = token;
}
