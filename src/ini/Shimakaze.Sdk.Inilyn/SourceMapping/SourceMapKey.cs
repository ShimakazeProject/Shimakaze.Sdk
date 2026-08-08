namespace Shimakaze.Sdk.Inilyn.SourceMapping;

/// <summary>
/// 源映射键信息。
/// </summary>
public sealed class SourceMapKey
{
    /// <summary>
    /// 键名。
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 键的行号（1-based）。
    /// </summary>
    public int Line { get; init; }

    /// <summary>
    /// 键的列号（1-based）。
    /// </summary>
    public int Column { get; init; }

    /// <summary>
    /// 值。
    /// </summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// 值的行号（1-based）。
    /// </summary>
    public int ValueLine { get; init; }

    /// <summary>
    /// 值的列号（1-based）。
    /// </summary>
    public int ValueColumn { get; init; }
}
