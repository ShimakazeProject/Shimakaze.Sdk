using System.Text.Json;

using Shimakaze.Sdk.Csf.Json.Converter.V2;

namespace Shimakaze.Sdk.Csf.Json;

/// <summary>
/// CsfJsonV2Writer.
/// </summary>
public static class CsfJsonV2Writer
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task WriteAsync(Stream stream, CsfDocument value, JsonSerializerOptions? options = default, CancellationToken cancellationToken = default)
    {
        options.Init(CsfJsonSerializerOptions.Converters);
        await JsonSerializer.SerializeAsync(stream, value, options, cancellationToken);
    }
}