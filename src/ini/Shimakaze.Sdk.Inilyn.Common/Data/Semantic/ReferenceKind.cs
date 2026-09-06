namespace Shimakaze.Sdk.Inilyn.Data.Semantic;

/// <summary>
/// 引用类型的种类。
/// </summary>
public enum ReferenceKind
{
    /// <summary>
    /// 节引用：某个键的值直接引用了另一个节。
    /// </summary>
    SectionRef,

    /// <summary>
    /// 注册表成员引用：某个键的值引用了注册表中的一个成员。
    /// </summary>
    RegistryRef,

    /// <summary>
    /// 发现规则引用：通过发现规则找到的引用。
    /// </summary>
    Discovery,

    /// <summary>
    /// 系统入口引用：节被系统（运行时引擎）固定引用，始终可达。
    /// 对应规则集中的全局节（Global）声明。
    /// </summary>
    Global,
}