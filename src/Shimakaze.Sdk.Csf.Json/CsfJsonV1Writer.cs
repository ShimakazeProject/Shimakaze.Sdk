using System.Text.Json;

using Shimakaze.Sdk.Csf.Json.Converter.V1;

namespace Shimakaze.Sdk.Csf.Json;

/// <summary>
/// CsfJsonV1Writer.
/// </summary>
public sealed class CsfJsonV1Writer : AsyncWriter<CsfDocument>, ICsfWriter
{
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// 构造器
    /// </summary>
    /// <param name="stream"> 基础流 </param>
    /// <param name="options"> </param>
    /// <param name="leaveOpen"> 退出时是否保持流打开 </param>
    public CsfJsonV1Writer(Stream stream, JsonSerializerOptions? options = null, bool leaveOpen = false) : base(stream, leaveOpen)
    {
        options ??= new();
        foreach (var item in CsfJsonSerializerOptions.Converters)
            options.Converters.Add(item);

        _options = options;
    }

    /// <inheritdoc />
    public override async Task WriteAsync(CsfDocument value, IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        await JsonSerializer.SerializeAsync(BaseStream, value, _options, cancellationToken);
    }
}