namespace Shimakaze.Sdk.Inilyn.Models.Symbol;

/// <summary>
/// 表示一个键的原始值对象，封装了字符串形式的原始文本。
/// </summary>
public sealed class ValueObject(string rawText)
{
    /// <summary>
    /// 获取该值的原始文本内容。
    /// </summary>
    public string RawText { get; } = rawText;

    /// <inheritdoc/>
    public override string ToString() => RawText;
}
