namespace Shimakaze.Sdk.Csf;

/// <summary>
/// Csf 读取器
/// </summary>
/// <remarks>
/// 构造 Csf 读取器
/// </remarks>
/// <param name="stream"> 基础流 </param>
/// <param name="leaveOpen"> 退出时是否保持流打开 </param>
public sealed class CsfReader(Stream stream, bool leaveOpen = false) : ICsfReader, IDisposable, IAsyncDisposable
{
    private readonly DisposableObject<Stream> _disposable = new(stream, leaveOpen);

    /// <inheritdoc/>
    public void Dispose() => _disposable.Dispose();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _disposable.DisposeAsync();

    /// <inheritdoc/>
    public async Task<CsfDocument> ReadAsync(IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        CsfDocument csf = new();
        _disposable.Resource.Read(out csf.InternalMetadata);
        CsfAsserts.IsCsfFile(csf.Metadata.Identifier);
        csf.Data = new CsfData[csf.Metadata.LabelCount];

        for (int i = 0; i < csf.Metadata.LabelCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            csf.Data[i] ??= new();
            progress?.Report((float)i / csf.Data.Length);

            _disposable.Resource.Read(out csf.Data[i].InternalIdentifier);
            CsfAsserts.IsLabel(csf.Data[i].Identifier, () => new object[] { i, _disposable.Resource.Position });
            _disposable.Resource.Read(out csf.Data[i].InternalStringCount);
            _disposable.Resource.Read(out csf.Data[i].InternalLabelNameLength);
            _disposable.Resource.Read(out csf.Data[i].InternalLabelName, csf.Data[i].LabelNameLength);

            csf.Data[i].Values = new CsfValue[csf.Data[i].StringCount];
            for (int j = 0; j < csf.Data[i].StringCount; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _disposable.Resource.Read(out csf.Data[i].Values[j].InternalIdentifier);
                CsfAsserts.IsStringOrExtraString(csf.Data[i].Values[j].Identifier, () => new object[] { i, j, _disposable.Resource.Position });

                _disposable.Resource.Read(out csf.Data[i].Values[j].InternalValueLength);
                _disposable.Resource.Read(out csf.Data[i].Values[j].InternalValue, csf.Data[i].Values[j].ValueLength, true);
                unsafe
                {
                    fixed (char* ptr = csf.Data[i].Values[j].Value)
                        CsfConstants.CodingValue((byte*)ptr, csf.Data[i].Values[j].ValueLength * sizeof(char));
                }

                if (csf.Data[i].Values[j].HasExtra)
                {
                    _disposable.Resource.Read(out int length);
                    csf.Data[i].Values[j].ExtraValueLength = length;
                    _disposable.Resource.Read(out csf.Data[i].Values[j].InternalExtraValue, length);
                }
            }
        }

        return csf;
    }
}