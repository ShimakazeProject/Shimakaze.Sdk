using System.Text.Json;

using Shimakaze.Sdk.Csf.Json.Converter.V1;

namespace Shimakaze.Sdk.Csf.Json;

/// <summary>
/// CsfJsonV1Reader.
/// </summary>
public static class CsfJsonV1Reader
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<CsfDocument> ReadAsync(Stream stream, JsonSerializerOptions? options = default, CancellationToken cancellationToken = default)
    {
        options.Init(CsfJsonSerializerOptions.Converters);
        CsfDocument? csf = await JsonSerializer.DeserializeAsync<CsfDocument>(stream, options, cancellationToken);
        CsfJsonAsserts.IsNotNull(csf);
        return csf;
    }
}