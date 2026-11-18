using Shimakaze.Sdk.Ra2lyn.Ini.Directives;

namespace Shimakaze.Sdk.Ra2lyn.Ini;

/// <summary>
/// INI 前端的解析选项。
/// </summary>
public sealed record class IniParseOptions
{
    /// <summary>
    /// 是否在解析后立即降级为 IR，默认为 <see langword="true"/>。
    /// </summary>
    public bool LowerToIr { get; init; } = true;

    /// <summary>
    /// 预处理器已定义的宏；条件表达式中未出现的名字一律视为假。
    /// </summary>
    public IReadOnlyList<string> Defines { get; init; } = [];
}
