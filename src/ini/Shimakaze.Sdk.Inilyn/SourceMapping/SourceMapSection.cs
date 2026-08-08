namespace Shimakaze.Sdk.Inilyn.SourceMapping;

/// <summary>
/// 源映射节信息。
/// </summary>
public sealed class SourceMapSection
{
    /// <summary>
    /// 节名。
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 行号（1-based）。
    /// </summary>
    public int Line { get; init; }

    /// <summary>
    /// 列号（1-based）。
    /// </summary>
    public int Column { get; init; }

    /// <summary>
    /// 键信息映射表。
    /// </summary>
    public Dictionary<string, SourceMapKey> Keys { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Mixin 引用的节名列表。
    /// </summary>
    public List<string> MixinRefs { get; init; } = [];

    /// <summary>
    /// 添加一个键。
    /// </summary>
    /// <param name="name">键名。</param>
    /// <param name="key">键信息。</param>
    /// <returns>当前实例（支持链式调用）。</returns>
    public SourceMapSection AddKey(string name, SourceMapKey key)
    {
        Keys[name] = key;
        return this;
    }
}
