using System.Reflection;

namespace Shimakaze.Sdk.JsonRPC.Server;

internal sealed record Target(string Path, MethodInfo Method, object? Object);