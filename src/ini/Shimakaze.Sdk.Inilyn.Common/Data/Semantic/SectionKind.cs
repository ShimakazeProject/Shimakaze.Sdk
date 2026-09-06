namespace Shimakaze.Sdk.Inilyn.Data.Semantic;

/// <summary>
/// 节的分类。
/// </summary>
public enum SectionKind
{
    /// <summary>
    /// 未分类（未被任何规则识别）。
    /// </summary>
    Unknown,

    /// <summary>
    /// 注册表节（值是该类型成员的名称）。
    /// </summary>
    Registry,

    /// <summary>
    /// 枚举节（键是枚举成员）。
    /// </summary>
    EnumSection,

    /// <summary>
    /// 全局节（不参与 TreeShaking）。
    /// </summary>
    Global,

    /// <summary>
    /// 实体节（遵循某个节定义）。
    /// </summary>
    Entity,
}
