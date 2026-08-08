namespace Shimakaze.Sdk.Inilyn.Analyzer.RuleSet;

/// <summary>
/// 值类型的种类。
/// </summary>
public enum InilynValueTypeKind
{
    /// <summary>
    /// 内置类型（int/float/boolean/string/percent）。
    /// </summary>
    Builtin,

    /// <summary>
    /// 元组类型（如 Vector3）。
    /// </summary>
    Tuple,

    /// <summary>
    /// 外部资源引用（如 SHP/PAL/VXL）。
    /// </summary>
    External,
}

/// <summary>
/// 元组字段。
/// </summary>
/// <param name="Name">字段名。</param>
/// <param name="Type">字段值类型。</param>
public sealed record class InilynTupleField(string Name, string Type);

/// <summary>
/// 值类型（跨组、跨文件共享）。
/// </summary>
/// <param name="name">类型名。</param>
/// <param name="kind">类型种类。</param>
/// <param name="separator">元组分隔符。</param>
/// <param name="fields">元组字段。</param>
/// <param name="externalKind">外部资源种类（External 时）。</param>
public sealed class InilynValueType(
    string name,
    InilynValueTypeKind kind,
    string? separator = null,
    IReadOnlyList<InilynTupleField>? fields = null,
    string? externalKind = null
)
{
    /// <summary>
    /// 内置类型名。
    /// </summary>
    public static readonly IReadOnlySet<string> BuiltinNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "int", "float", "boolean", "string", "percent",
    };

    /// <summary>
    /// 类型名。
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// 类型种类。
    /// </summary>
    public InilynValueTypeKind Kind { get; } = kind;

    /// <summary>
    /// 元组分隔符（默认 ","）。
    /// </summary>
    public string Separator { get; } = separator ?? ",";

    /// <summary>
    /// 元组字段。
    /// </summary>
    public IReadOnlyList<InilynTupleField> Fields { get; } = fields ?? [];

    /// <summary>
    /// 外部资源种类。
    /// </summary>
    public string? ExternalKind { get; } = externalKind;
}
