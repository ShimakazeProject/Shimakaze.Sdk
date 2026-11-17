namespace Shimakaze.Sdk.Analyzer.Common.Rules;

/// <summary>
/// 复合类型定义，描述由分隔符隔开的多个字段。
/// </summary>
public sealed class TypeDefinition
{
    /// <summary>
    /// 类型名。
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 字段分隔符；为 <see langword="null"/> 时按单值处理。
    /// </summary>
    public string? Separator { get; init; }

    /// <summary>
    /// 字段列表。
    /// </summary>
    public List<TypeField> Fields { get; init; } = [];
}

/// <summary>
/// 复合类型中的一个字段。
/// </summary>
/// <param name="Name">字段名。</param>
/// <param name="Type">字段类型名。</param>
public sealed record class TypeField(string Name, string Type);

/// <summary>
/// 枚举定义，用于校验枚举节与枚举类型属性。
/// </summary>
public sealed class EnumDefinition
{
    /// <summary>
    /// 枚举名。
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 允许的取值，比较使用 <see cref="StringComparer.OrdinalIgnoreCase"/>。
    /// </summary>
    public List<string> Values { get; init; } = [];
}
