namespace Shimakaze.Sdk.Ra2lyn.Common.Ir;

/// <summary>
/// 实体（源语言中的“节”）的分类。
/// </summary>
public enum EntityKind
{
    /// <summary>
    /// 未分类，尚未被任何规则识别。
    /// </summary>
    Unknown,

    /// <summary>
    /// 注册表：其属性值登记了某个类型的成员名。
    /// </summary>
    Registry,

    /// <summary>
    /// 全局：由系统固定引用，始终可达。
    /// </summary>
    Global,

    /// <summary>
    /// 枚举节：其属性键是枚举成员。
    /// </summary>
    EnumSection,

    /// <summary>
    /// 普通实体：遵循某个节定义。
    /// </summary>
    Entity,
}

/// <summary>
/// 引用的来源种类。
/// </summary>
public enum ReferenceKind
{
    /// <summary>
    /// 属性值直接引用另一个实体。
    /// </summary>
    SectionRef,

    /// <summary>
    /// 通过注册表成员得到的引用。
    /// </summary>
    RegistryRef,

    /// <summary>
    /// 发现规则产生的引用。
    /// </summary>
    Discovery,

    /// <summary>
    /// 系统入口引用（如全局实体自身）。
    /// </summary>
    Global,
}
