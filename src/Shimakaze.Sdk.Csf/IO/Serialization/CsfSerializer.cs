using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.IO.Serialization;

namespace Shimakaze.Sdk.IO.Csf.Serialization;

/// <summary>
/// Csf ���л���
/// </summary>
public class CsfSerializer : ISerializer<CsfDocument>, IAsyncSerializer<CsfDocument, Task>, IDisposable, IAsyncDisposable
{
    private bool _disposedValue;
    /// <summary>
    /// д����
    /// </summary>
    protected CsfWriter _writer;

    /// <summary>
    /// Csf���л��� ������
    /// </summary>
    /// <param name="baseStream">������</param>
    /// <param name="leaveOpen">�˳�ʱ�Ƿ񱣳�����</param>
    /// <exception cref="NotSupportedException">������֧��Seekʱ�׳�</exception>
    public CsfSerializer(Stream baseStream, bool leaveOpen = false)
    {
        _writer = new(baseStream, leaveOpen);
    }

    /// <inheritdoc/>
    public virtual void Serialize(CsfDocument value)
    {
        _writer.WriteMetadataDirect(value.Metadata);
        foreach (var item in value.Data)
            _writer.Write(item);
    }

    /// <inheritdoc/>
    public virtual async Task SerializeAsync(CsfDocument value, CancellationToken cancellationToken = default)
    {
        _writer.WriteMetadataDirect(value.Metadata);
        foreach (var item in value.Data)
            await _writer.WriteAsync(item, cancellationToken);
    }


    /// <summary>
    /// �ͷ���Դ
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _writer.Dispose();
            }

            _disposedValue = true;
        }
    }

    /// <summary>
    /// �첽�ͷź���
    /// </summary>
    /// <returns></returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        await _writer.DisposeAsync();
    }

    // ~CsfSerializer()
    // {
    //     Dispose(disposing: false);
    // }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }
}