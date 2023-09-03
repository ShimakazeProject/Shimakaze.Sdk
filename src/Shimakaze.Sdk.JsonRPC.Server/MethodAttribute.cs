using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.JsonRPC.Server;

/// <summary>
/// 表示这是一个JsonRpc 方法/事件
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class MethodAttribute : Attribute
{
    /// <summary>
    /// 这个方法/事件的路径
    /// </summary>
    [StringSyntax("Uri")]
    public string? Route { get; init; }

    /// <summary>
    /// 表示这是一个JsonRpc 方法/事件
    /// </summary>
    /// <param name="route"> 这个方法/事件的路径 </param>
    public MethodAttribute([StringSyntax("Uri")] string? route = default) => Route = route;
}