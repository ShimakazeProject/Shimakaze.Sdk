namespace Shimakaze.Sdk.Inilyn.Models.Symbol;

/// <summary>
/// 表示配置文件中的一个键（Key），属于某个节并包含一个可选的值。
/// </summary>
public sealed class KeySymbol : Symbol
{
    /// <summary>
    /// 获取当前键所属的节。
    /// </summary>
    public SectionSymbol Parent { get; internal set; } = default!;

    /// <summary>
    /// 获取当前键的名称。
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// 获取当前键的值。可能为 null。
    /// </summary>
    public ValueObject? Value { get; }

    /// <inheritdoc/>
    public override string Name => Key;

    internal KeySymbol(string key, ValueObject? value)
    {
        Key = key;
        Value = value;
    }
}
