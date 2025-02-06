using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;

using Shimakaze.Sdk.Csf.Json.Models.V2;

namespace Shimakaze.Sdk.Csf.Json.IO;

/// <summary>
/// 
/// </summary>
public static class CsfJsonV2
{
    /// <summary>
    /// 默认的反序列化设置
    /// </summary>
    public static readonly JsonSerializerOptions DefaultOptions = new()
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
    };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="utf8Json"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<CsfData> ReadAllDataAsync(Stream utf8Json, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= DefaultOptions;

        var data = await JsonSerializer.DeserializeAsync<CsfJsonData>(utf8Json, options, cancellationToken)
            .ConfigureAwait(false);
        Debug.Assert(data is not null);

        return data;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="utf8Json"></param>
    /// <param name="data"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task WriteAllDataAsync(Stream utf8Json, CsfData data, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= DefaultOptions;

        await JsonSerializer.SerializeAsync<CsfJsonData>(utf8Json, data, options, cancellationToken)
            .ConfigureAwait(false);
    }

}
