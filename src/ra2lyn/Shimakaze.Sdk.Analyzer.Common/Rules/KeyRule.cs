namespace Shimakaze.Sdk.Analyzer.Common.Rules;

/// <summary>
/// 一条键规则：声明属性值的类型，以及可选的列表分隔符。
/// </summary>
/// <param name="Type">值类型名，对应内置类型、复合类型或外部验证器。</param>
/// <param name="List">
/// 列表分隔符；为 <see langword="null"/> 表示该属性不是列表。
/// </param>
public sealed record class KeyRule(string Type, string? List);
