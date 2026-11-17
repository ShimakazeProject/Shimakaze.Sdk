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
}
