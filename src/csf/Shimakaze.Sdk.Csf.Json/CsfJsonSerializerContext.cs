using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json;

/// <summary>
/// JsonSerializer 上下文基类
/// </summary>
public abstract class CsfJsonSerializerContext : JsonSerializerContext
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    protected CsfJsonSerializerContext(JsonSerializerOptions? options) : base(options)
    {
        if (options is not null)
            InitializeOptions(options);
    }

    /// <summary>
    /// 初始化 JsonSerializerOptions
    /// </summary>
    /// <param name="options"></param>
    protected abstract void InitializeOptions(JsonSerializerOptions options);
}
