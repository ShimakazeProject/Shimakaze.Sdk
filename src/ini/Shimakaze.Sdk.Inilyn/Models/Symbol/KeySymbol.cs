namespace Shimakaze.Sdk.Inilyn.Models.Symbol;

/// <summary>
/// 表示配置文件中的一个键（Key），属于某个节并包含一个可选的值。
/// </summary>
public sealed class KeySymbol(SectionSymbol parent, string key, ValueObject? value) : Symbol
{
    /// <summary>
    /// 获取当前键所属的节。
    /// </summary>
    public SectionSymbol Parent { get; } = parent;

    /// <summary>
    /// 获取当前键的名称。
    /// </summary>
    public string Key { get; } = key;

    /// <summary>
    /// 获取当前键的值。可能为 null。
    /// </summary>
    public ValueObject? Value { get; } = value;

    /// <inheritdoc/>
    public override string Name => Key;
}
