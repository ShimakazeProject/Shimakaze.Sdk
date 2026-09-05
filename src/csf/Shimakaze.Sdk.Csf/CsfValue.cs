namespace Shimakaze.Sdk.Csf;

/// <summary>
/// CSF 值
/// </summary>
public sealed record class CsfValue()
{
    /// <summary>
    /// 值
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 额外值
    /// </summary>
    public string? Extra { get; set; }

    /// <summary>
    /// 初始化一个新的 CSF 值实例
    /// </summary>
    /// <param name="value">值</param>
    public CsfValue(string value) : this()
    {
        Value = value;
    }
    /// <summary>
    /// 初始化一个新的 CSF 值实例
    /// </summary>
    /// <param name="value">值</param>
    /// <param name="extra">额外值</param>
    public CsfValue(string value, string? extra) : this(value)
    {
        Extra = extra;
    }
}
