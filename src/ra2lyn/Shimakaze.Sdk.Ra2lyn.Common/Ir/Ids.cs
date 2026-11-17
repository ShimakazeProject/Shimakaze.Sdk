namespace Shimakaze.Sdk.Ra2lyn.Common.Ir;

/// <summary>
/// 实体标识，编译期内确定且有序。
/// </summary>
/// <param name="Value">从 0 开始递增的序号。</param>
public readonly record struct EntityId(int Value) : IComparable<EntityId>
{
    /// <summary>
    /// 约定：<see cref="Unknown"/> 表示“没有实体”，真实实体的序号始终大于它。
    /// </summary>
    public static EntityId Unknown
        => new(-1);

    /// <inheritdoc/>
    public int CompareTo(EntityId other)
        => Value.CompareTo(other.Value);

    /// <summary>
    /// 按序号比较两个实体标识。
    /// </summary>
    public static bool operator <(EntityId left, EntityId right)
        => left.CompareTo(right) < 0;

    /// <summary>
    /// 按序号比较两个实体标识。
    /// </summary>
    public static bool operator >(EntityId left, EntityId right)
        => left.CompareTo(right) > 0;

    /// <summary>
    /// 按序号比较两个实体标识。
    /// </summary>
    public static bool operator <=(EntityId left, EntityId right)
        => left.CompareTo(right) <= 0;

    /// <summary>
    /// 按序号比较两个实体标识。
    /// </summary>
    public static bool operator >=(EntityId left, EntityId right)
        => left.CompareTo(right) >= 0;
}

/// <summary>
/// 属性标识，编译期内确定且有序。
/// </summary>
/// <param name="Value">从 0 开始递增的序号。</param>
public readonly record struct PropertyId(int Value) : IComparable<PropertyId>
{
    /// <summary>
    /// 约定：<see cref="Unknown"/> 表示“没有属性”，真实属性的序号始终大于它。
    /// </summary>
    public static PropertyId Unknown
        => new(-1);

    /// <inheritdoc/>
    public int CompareTo(PropertyId other)
        => Value.CompareTo(other.Value);

    /// <summary>
    /// 按序号比较两个属性标识。
    /// </summary>
    public static bool operator <(PropertyId left, PropertyId right)
        => left.CompareTo(right) < 0;

    /// <summary>
    /// 按序号比较两个属性标识。
    /// </summary>
    public static bool operator >(PropertyId left, PropertyId right)
        => left.CompareTo(right) > 0;

    /// <summary>
    /// 按序号比较两个属性标识。
    /// </summary>
    public static bool operator <=(PropertyId left, PropertyId right)
        => left.CompareTo(right) <= 0;

    /// <summary>
    /// 按序号比较两个属性标识。
    /// </summary>
    public static bool operator >=(PropertyId left, PropertyId right)
        => left.CompareTo(right) >= 0;
}
