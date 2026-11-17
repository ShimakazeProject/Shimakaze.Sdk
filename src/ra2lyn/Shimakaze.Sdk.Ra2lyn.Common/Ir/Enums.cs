namespace Shimakaze.Sdk.Ra2lyn.Common.Ir;

/// <summary>
/// 属性值的形态：源语言只区分“简单值”与“引用值”，含义由分析阶段解释。
/// </summary>
public enum PropertyValueKind
{
    /// <summary>
    /// 简单值：字面量、数字、字符串等，不需要跨实体解析。
    /// </summary>
    Simple,

    /// <summary>
    /// 引用值：值指向另一个实体（或实体集合），由分析阶段解析并建立引用边。
    /// </summary>
    Reference,
}

/// <summary>
/// 引用的来源种类。
/// </summary>
public enum ReferenceKind
{
    /// <summary>
    /// 属性值直接引用另一个实体。
    /// </summary>
    Direct,

    /// <summary>
    /// 通过某种登记表间接得到的引用。
    /// </summary>
    Indirect,

    /// <summary>
    /// 由分析规则推导得到的引用。
    /// </summary>
    Derived,

    /// <summary>
    /// 由调用方给定（或系统固定）的入口引用，没有来源属性。
    /// </summary>
    Entry,
}
