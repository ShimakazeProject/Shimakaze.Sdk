namespace Shimakaze.Sdk.Inilyn.Semantic;

/// <summary>
/// 展平后的 INI 节（Mixin 已展开）。
/// </summary>
/// <param name="name">节名。</param>
/// <param name="keyValues">该节的所有键值对（已合并 Mixin 来源）。</param>
public sealed class IniSemanticSection(
    string name,
    IReadOnlyList<IniSemanticKeyValue> keyValues
)
{
    /// <summary>
    /// 节名。
    /// </summary>
    public string Name => name;

    /// <summary>
    /// 该节的所有键值对。
    /// </summary>
    public IReadOnlyList<IniSemanticKeyValue> KeyValues => keyValues;
}
