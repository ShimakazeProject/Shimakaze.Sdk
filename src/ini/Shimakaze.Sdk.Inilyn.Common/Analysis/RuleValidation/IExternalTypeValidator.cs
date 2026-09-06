namespace Shimakaze.Sdk.Inilyn.Analysis.RuleValidation;

/// <summary>
/// 外部类型验证器接口。用于验证由扩展系统管理的类型（如 CSFRef、Art.Animation 等）。
/// </summary>
/// <remarks>
/// <para>
/// 实现此接口可将自定义类型验证逻辑注入分析管道。
/// 例如，CSFRef 类型的值可能需要查询本地化文件以确认引用是否存在。
/// </para>
/// </remarks>
public interface IExternalTypeValidator
{
    /// <summary>
    /// 获取此验证器处理的类型名称。
    /// </summary>
    string TypeName { get; }

    /// <summary>
    /// 验证值是否符合该外部类型的约束。
    /// </summary>
    /// <param name="value">待验证的值字符串。</param>
    /// <param name="mode">验证模式。</param>
    /// <returns>
    /// 验证通过返回 <see langword="null"/>；
    /// 验证失败返回错误描述信息。
    /// </returns>
    string? Validate(string? value, ValidationMode mode);
}
