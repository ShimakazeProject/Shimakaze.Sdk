namespace Shimakaze.Sdk.Csf;

/// <summary>
/// Csf 写入器
/// </summary>
/// <param name="stream"> 基础流 </param>
/// <param name="leaveOpen"> 退出时是否保持流打开 </param>
public sealed class CsfWriter(Stream stream, bool leaveOpen = false) : ICsfWriter, IDisposable, IAsyncDisposable
{
    private readonly DisposableObject<Stream> _disposable = new(stream, leaveOpen);

    /// <inheritdoc/>
    public void Dispose() => _disposable.Dispose();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _disposable.DisposeAsync();

    /// <inheritdoc />
    public async Task WriteAsync(CsfDocument value, IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        _disposable.Resource.Write(value.Metadata);

        for (int i = 0; i < value.Data.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((float)i / value.Data.Length);

            _disposable.Resource.Write(value.Data[i].Identifier);
            _disposable.Resource.Write(value.Data[i].StringCount);
            _disposable.Resource.Write(value.Data[i].LabelNameLength);
            _disposable.Resource.Write(value.Data[i].LabelName, value.Data[i].LabelNameLength);

            for (int j = 0; j < value.Data[i].Values.Length; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _disposable.Resource.Write(value.Data[i].Values[j].Identifier);
                _disposable.Resource.Write(value.Data[i].Values[j].ValueLength);
                unsafe
                {
                    fixed (char* ptr = value.Data[i].Values[j].Value)
                        CsfConstants.CodingValue((byte*)ptr, value.Data[i].Values[j].ValueLength * sizeof(char));
                }
                _disposable.Resource.Write(value.Data[i].Values[j].Value, value.Data[i].Values[j].ValueLength, true);

                if (value.Data[i].Values[j] is
                    {
                        HasExtra: true,
                        ExtraValue: not null
                    } e)
                {
                    _disposable.Resource.Write(e.ExtraValueLength.Value);
                    _disposable.Resource.Write(e.ExtraValue, e.ExtraValueLength.Value);
                }
            }
        }
    }
}