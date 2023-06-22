using System.Text.Json;

using Shimakaze.Sdk.Csf.Json.Converter.V1;
using Shimakaze.Sdk.IO.Serialization;

namespace Shimakaze.Sdk.Csf.Json.Serialization;

/// <summary>
/// CsfJsonV1Serializer.
/// </summary>
public sealed class CsfJsonV1Serializer :
    ISerializer<CsfDocument?>, IAsyncSerializer<CsfDocument?, Task>,
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
    public CsfJsonV1Serializer(Stream baseStream, JsonSerializerOptions? options = null, bool leaveOpen = false)
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
    public void Serialize(in CsfDocument? value)
    {
        JsonSerializer.Serialize(BaseStream, value, _options);
    }

    /// <inheritdoc/>
    public async Task SerializeAsync(CsfDocument? value, CancellationToken cancellationToken = default)
    {
        await JsonSerializer.SerializeAsync(BaseStream, value, _options, cancellationToken);
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
