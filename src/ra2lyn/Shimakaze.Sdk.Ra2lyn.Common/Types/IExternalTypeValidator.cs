using Shimakaze.Sdk.Ra2lyn.Common.Types;

namespace Shimakaze.Sdk.Ra2lyn.Common.Types;

/// <summary>
/// 外部类型验证器，用于校验由扩展系统管理的类型。
/// </summary>
public interface IExternalTypeValidator
{
    /// <summary>
    /// 此验证器处理的类型名。
    /// </summary>
    string TypeName { get; }

    /// <summary>
    /// 校验值是否符合该类型；通过时返回 <see langword="null"/>，否则返回错误描述。
    /// </summary>
    string? Validate(string? value, ValidationMode mode);
}
