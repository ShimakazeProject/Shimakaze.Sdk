namespace Shimakaze.Sdk.Inilyn.SourceMapping;

/// <summary>
/// 源映射，描述 INI 文件的结构信息。
/// </summary>
public sealed class SourceMap
{
    /// <summary>
    /// 节信息映射表。
    /// </summary>
    public Dictionary<string, SourceMapSection> Sections { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 全局键信息映射表。
    /// </summary>
    public Dictionary<string, SourceMapKey> GlobalKeys { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 添加一个节。
    /// </summary>
    /// <param name="name">节名。</param>
    /// <param name="section">节信息。</param>
    /// <returns>当前实例（支持链式调用）。</returns>
    public SourceMap AddSection(string name, SourceMapSection section)
    {
        Sections[name] = section;
        return this;
    }

    /// <summary>
    /// 添加一个全局键。
    /// </summary>
    /// <param name="name">键名。</param>
    /// <param name="key">键信息。</param>
    /// <returns>当前实例（支持链式调用）。</returns>
    public SourceMap AddGlobalKey(string name, SourceMapKey key)
    {
        GlobalKeys[name] = key;
        return this;
    }
}
