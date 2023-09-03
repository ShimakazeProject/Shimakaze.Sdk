using System.Reflection;

namespace Shimakaze.Sdk.JsonRPC.Server;

/// <summary>
/// JsonRPC 方法
/// </summary>
/// <param name="Route"></param>
/// <param name="Method"></param>
/// <param name="Type"></param>
public sealed record class Target(string Route, MethodInfo Method, Type Type);
