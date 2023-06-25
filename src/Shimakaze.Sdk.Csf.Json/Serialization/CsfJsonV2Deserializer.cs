using System.Text.Json;

using Shimakaze.Sdk.Csf.Json.Converter.V2;
using Shimakaze.Sdk.IO.Serialization;

namespace Shimakaze.Sdk.Csf.Json.Serialization;

/// <summary>
/// CsfJsonV2Deserializer.
/// </summary>
public sealed class CsfJsonV2Deserializer :
    IDeserializer<CsfDocument>, IAsyncDeserializer<Task<CsfDocument>>,
    IDisposable, IAsyncDisposable
{
    private bool _disposedValue;
    private readonly bool _leaveOpen;
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// 构造器
    /// </summary>
    /// <param name="baseStream">基础流</param>
    /// <param name="options"></param>
    /// <param name="leaveOpen">退出时是否保持流打开</param>
    public CsfJsonV2Deserializer(Stream baseStream, JsonSerializerOptions? options = null, bool leaveOpen = false)
    {
        BaseStream = baseStream;
        _leaveOpen = leaveOpen;

        options ??= new();
        foreach (var item in CsfJsonSerializerOptions.Converters)
            options.Converters.Add(item);

        _options = options;
    }

    /// <summary>
    /// 基础流
    /// </summary>
    public Stream BaseStream { get; }

    /// <inheritdoc/>
    public CsfDocument Deserialize()
    {
        return JsonSerializer.Deserialize<CsfDocument>(BaseStream, _options);
    }

    /// <inheritdoc/>
    public async Task<CsfDocument> DeserializeAsync(CancellationToken cancellationToken = default)
    {
        return await JsonSerializer.DeserializeAsync<CsfDocument>(BaseStream, _options, cancellationToken);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing"></param>
    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (!_leaveOpen)
                    BaseStream.Dispose();
            }

            _disposedValue = true;
        }
    }

    /// <summary>
    /// 异步释放核心
    /// </summary>
    /// <returns></returns>
    private async ValueTask DisposeAsyncCore()
    {
        if (!_leaveOpen)
            await BaseStream.DisposeAsync();
    }

    // ~CsfSerializer()
    // {
    //     Dispose(disposing: false);
    // }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
    }

}
