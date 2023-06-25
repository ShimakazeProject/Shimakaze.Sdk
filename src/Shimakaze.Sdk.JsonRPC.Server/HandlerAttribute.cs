namespace Shimakaze.Sdk.JsonRPC.Server;

/// <summary>
/// 表示这是一个JsonRpc Handler
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class HandlerAttribute : Attribute
{
    /// <summary>
    /// 这个Handler中的方法/事件的路径
    /// </summary>
    public string? Route { get; set; }

    /// <summary>
    /// 表示这是一个JsonRpc Handler
    /// </summary>
    /// <param name="route">这个Handler中的方法/事件的路径</param>
    public HandlerAttribute(string? route = null)
    {
        Route = route;
    }
}