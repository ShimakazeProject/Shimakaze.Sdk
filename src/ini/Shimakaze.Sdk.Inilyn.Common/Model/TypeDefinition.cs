namespace Shimakaze.Sdk.Inilyn.Model;

/// <summary>
/// 类型定义，描述一个复合类型（如 Vector3）的字段和分隔符。
/// </summary>
public sealed class TypeDefinition
{
    /// <summary>
    /// 类型名称。
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 字段分隔符。
    /// </summary>
    public string? Separator { get; init; }

    /// <summary>
    /// 类型的字段列表。
    /// </summary>
    public List<TypeField> Fields { get; init; } = [];
}

/// <summary>
/// 类型字段定义。
/// </summary>
/// <param name="Name">字段名称。</param>
/// <param name="Type">字段类型（基础类型名称）。</param>
public sealed record class TypeField(string Name, string Type);
