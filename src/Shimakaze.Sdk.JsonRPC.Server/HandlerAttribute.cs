using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.JsonRPC.Server;

/// <summary>
/// 表示这是一个JsonRpc Handler
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class HandlerAttribute : Attribute
{
    /// <summary>
    /// 这个Handler中的方法/事件的路径
    /// </summary>
    [StringSyntax("Uri")]
    public string? Route { get; init; }

    /// <summary>
    /// 表示这是一个JsonRpc Handler
    /// </summary>
    /// <param name="route"> 这个Handler中的方法/事件的路径 </param>
    public HandlerAttribute([StringSyntax("Uri")]string? route = default) => Route = route;
}