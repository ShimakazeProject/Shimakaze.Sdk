namespace Shimakaze.Sdk.JsonRPC.Server;

/// <summary>
/// 表示这是一个JsonRpc 方法/事件
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class MethodAttribute : Attribute
{
    /// <summary>
    /// 这个方法/事件的路径
    /// </summary>
    public string? Route { get; set; }
    /// <summary>
    /// 表示这是一个JsonRpc 方法/事件
    /// </summary>
    /// <param name="route">这个方法/事件的路径</param>
    public MethodAttribute(string? route = null)
    {
        Route = route;
    }
}
