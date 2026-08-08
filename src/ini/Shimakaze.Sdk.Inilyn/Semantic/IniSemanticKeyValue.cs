namespace Shimakaze.Sdk.Inilyn.Semantic;

/// <summary>
/// 展平后的键值对。
/// </summary>
/// <param name="key">键名。</param>
/// <param name="value">值。</param>
/// <param name="sourceSection">来源节名（若是 Mixin 展开，则为源节名）。</param>
public sealed class IniSemanticKeyValue(
    string key,
    string value,
    string? sourceSection = null
)
{
    /// <summary>
    /// 键名。
    /// </summary>
    public string Key => key;

    /// <summary>
    /// 值。
    /// </summary>
    public string Value => value;

    /// <summary>
    /// 来源节名。若为 <see langword="null"/>，表示该键值对直接声明在当前节。
    /// </summary>
    public string? SourceSection => sourceSection;
}
